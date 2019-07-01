using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Quantumart.QP8.BLL.Facades;
using Quantumart.QP8.BLL.Helpers;
using Quantumart.QP8.BLL.Services.DTO;
using Quantumart.QP8.DAL;
using Quantumart.QP8.DAL.Entities;
using Quantumart.QP8.Utils;
using EntityState = Microsoft.EntityFrameworkCore.EntityState;

namespace Quantumart.QP8.BLL.Repository
{
    internal static class UserGroupRepository
    {
        internal static IEnumerable<UserGroup> GetAllGroups() => MapperFacade.UserGroupMapper.GetBizList(QPContext.EFContext.UserGroupSet.ToList());

        internal static IEnumerable<UserGroup> GetNtGroups()
        {
            var groups = QPContext
                .EFContext
                .UserGroupSet
                .Include(x => x.ParentGroupToGroupBinds).ThenInclude(y => y.ParentGroup)
                .Where(f => f.NtGroup != null);
            return MapperFacade.UserGroupMapper.GetBizList(groups.ToList());
        }

        internal static IEnumerable<UserGroupListItem> List(ListCommand cmd, out int totalRecords, List<int> selectedIds)
        {
            using (var scope = new QPConnectionScope())
            {
                cmd.SortExpression = !string.IsNullOrWhiteSpace(cmd.SortExpression) ? TranslateHelper.TranslateSortExpression(cmd.SortExpression) : null;
                var rows = Common.GetUserGroupPage(scope.DbConnection, selectedIds, cmd.SortExpression, cmd.FilterExpression, cmd.StartRecord, cmd.PageSize, out totalRecords);
                return MapperFacade.UserGroupListItemRowMapper.GetBizList(rows.ToList());
            }
        }

        /// <summary>
        /// Возвращает список по ids
        /// </summary>
        /// <returns></returns>
        internal static IEnumerable<UserGroup> GetList(IEnumerable<int> ids)
        {
            IEnumerable<decimal> decIDs = Converter.ToDecimalCollection(ids).Distinct().ToArray();
            return MapperFacade.UserGroupMapper.GetBizList(QPContext.EFContext.UserGroupSet.Where(f => decIDs.Contains(f.Id)).ToList());
        }

        internal static UserGroup GetPropertiesById(int id)
        {
            return MapperFacade.UserGroupMapper.GetBizObject(QPContext.EFContext.UserGroupSet
                .Include(x => x.ParentGroupToGroupBinds).ThenInclude(y => y.ParentGroup)
                .Include(x => x.ChildGroupToGroupBinds).ThenInclude(y => y.ChildGroup)
                .Include(x => x.UserGroupBinds).ThenInclude(y => y.User)
                .Include(x => x.LastModifiedByUser)
                .SingleOrDefault(g => g.Id == id)
            );
        }

        internal static UserGroup GetById(int id)
        {
            return MapperFacade.UserGroupMapper.GetBizObject(QPContext.EFContext.UserGroupSet.SingleOrDefault(g => g.Id == id));
        }

        internal static UserGroup UpdateProperties(UserGroup group)
        {
            var entities = QPContext.EFContext;
            var dal = MapperFacade.UserGroupMapper.GetDalObject(group);
            dal.LastModifiedBy = QPContext.CurrentUserId;

            using (new QPConnectionScope())
            {
                dal.Modified = Common.GetSqlDate(QPConnectionScope.Current.DbConnection);
            }

            entities.Entry(dal).State = EntityState.Modified;

            var dalDb = entities
                .UserGroupSet
                .Include(x => x.ParentGroupToGroupBinds).ThenInclude(y => y.ParentGroup)
                .Include(x => x.UserGroupBinds).ThenInclude(y => y.User)
                .Single(g => g.Id == dal.Id);
            foreach (var pg in dalDb.ParentGroupToGroupBinds.ToArray())
            {
                if (group.ParentGroup == null || pg.ParentGroup.Id != group.ParentGroup.Id)
                {
                    dalDb.ParentGroupToGroupBinds.Remove(pg);
                }
            }

            if (group.ParentGroup != null)
            {
                if (dalDb.ParentGroups.All(g => g.Id != group.ParentGroup.Id))
                {
                    var dalParent = entities.UserGroupSet.Single(g => g.Id == group.ParentGroup.Id);
                    entities.UserGroupSet.Attach(dalParent);
                    var bind = new GroupToGroupBindDAL { ParentGroup = dalParent, ChildGroup = dal};
                    dal.ParentGroupToGroupBinds.Add(bind);
                }
            }

            var inmemoryUserIDs = new HashSet<decimal>(group.Users.Select(u => Converter.ToDecimal(u.Id)));
            var indbUserIDs = new HashSet<decimal>(dalDb.Users.Select(u => u.Id));
            foreach (var u in dalDb.UserGroupBinds.ToArray())
            {
                if (!inmemoryUserIDs.Contains(u.User.Id))
                {
                    dalDb.UserGroupBinds.Remove(u);
                }
            }
            foreach (var u in MapperFacade.UserMapper.GetDalList(group.Users.ToList()))
            {
                if (!indbUserIDs.Contains(u.Id))
                {
                    var bind = new UserUserGroupBindDAL { UserId = u.Id, UserGroupId = group.Id };
                    dal.UserGroupBinds.Add(bind);
                }
            }

            entities.SaveChanges();
            return MapperFacade.UserGroupMapper.GetBizObject(dal);
        }

        internal static UserGroup SaveProperties(UserGroup group)
        {
            var entities = QPContext.EFContext;
            var dal = MapperFacade.UserGroupMapper.GetDalObject(group);
            dal.LastModifiedBy = QPContext.CurrentUserId;
            using (new QPConnectionScope())
            {
                dal.Created = Common.GetSqlDate(QPConnectionScope.Current.DbConnection);
                dal.Modified = dal.Created;
            }

            entities.Entry(dal).State = EntityState.Added;

            if (group.ParentGroup != null)
            {
                var parentDal = MapperFacade.UserGroupMapper.GetDalObject(group.ParentGroup);
                entities.UserGroupSet.Attach(parentDal);
                var bind = new GroupToGroupBindDAL { ParentGroup = parentDal, ChildGroup = dal };
                dal.ParentGroupToGroupBinds.Add(bind);
            }

            foreach (var u in MapperFacade.UserMapper.GetDalList(group.Users.ToList()))
            {
                entities.UserSet.Attach(u);
                var bind = new UserUserGroupBindDAL { UserGroup = dal, User = u };
                dal.UserGroupBinds.Add(bind);
            }

            entities.SaveChanges();
            return MapperFacade.UserGroupMapper.GetBizObject(dal);
        }

        /// <summary>
        /// Отфильтровать пользователей прямо или косвенно входящих в группу администраторов
        /// </summary>
        internal static IEnumerable<int> SelectAdminDescendantGroupUserIDs(IEnumerable<int> userIds, int groupId)
        {
            using (var scope = new QPConnectionScope())
            {
                return Common.UserGroups_SelectAdminDescendantGroupUserIDs(userIds, groupId, scope.DbConnection);
            }
        }

        /// <summary>
        /// Входит ли группа в иерархию группы Администраторы
        /// </summary>
        internal static bool IsGroupAdminDescendant(int groupId)
        {
            using (var scope = new QPConnectionScope())
            {
                return Common.UserGroups_IsGroupAdminDescendant(groupId, scope.DbConnection);
            }
        }

        /// <summary>
        /// Проверит возможность образования цикла в иерархии групп
        /// </summary>
        internal static bool IsCyclePossible(int groupId, int parentGroupId)
        {
            using (var scope = new QPConnectionScope())
            {
                return Common.UserGroups_IsCyclePossible(groupId, parentGroupId, scope.DbConnection);
            }
        }

        /// <summary>
        /// Отфильтровать пользователей прямо входящих в группы использующие параллельный Workflow
        /// </summary>
        internal static IEnumerable<int> SelectWorkflowGroupUserIDs(int[] userIds)
        {
            using (var scope = new QPConnectionScope())
            {
                return Common.UserGroups_SelectWorkflowGroupUserIDs(userIds, scope.DbConnection);
            }
        }

        /// <summary>
        /// Возвращает иерархию группы Администраторы
        /// </summary>
        internal static IEnumerable<DataRow> GetAdministratorsHierarhy()
        {
            using (var scope = new QPConnectionScope())
            {
                return Common.UserGroups_GetAdministratorsHierarhy(scope.DbConnection);
            }
        }

        internal static int CopyGroup(UserGroup group, int currentUserId)
        {
            var entities = QPContext.EFContext;

            var userGroupDal = entities
                .UserGroupSet
                .AsNoTracking()
                .Include(x => x.ParentGroupToGroupBinds)
                .Include(x => x.UserGroupBinds)
                .Include(x => x.SiteAccess)
                .Include(x => x.ContentAccess)
                .Include(x => x.ArticleAccess)
                .Include(x => x.FolderAccess)
                .Include(x => x.ENTITY_TYPE_ACCESS)
                .Include(x => x.ACTION_ACCESS)
                .Single(x => x.Id == group.Id);

            userGroupDal.Id = 0;
            userGroupDal.Name = group.Name;
            userGroupDal.Created = userGroupDal.Modified = DateTime.Now;
            userGroupDal.LastModifiedBy = currentUserId;
            userGroupDal.BuiltIn = false;
            userGroupDal.IsReadOnly = false;

            foreach (var sitePermissionDAL in userGroupDal.SiteAccess)
            {
                sitePermissionDAL.Id = 0;
            }

            foreach (var contentPermissionDAL in userGroupDal.ContentAccess)
            {
                contentPermissionDAL.Id = 0;
            }

            foreach (var articlePermissionDAL in userGroupDal.ArticleAccess)
            {
                articlePermissionDAL.Id = 0;
            }

            foreach (var folderPermissionDAL in userGroupDal.FolderAccess)
            {
                folderPermissionDAL.Id = 0;
            }

            foreach (var entityTypePermissionDAL in userGroupDal.ENTITY_TYPE_ACCESS)
            {
                entityTypePermissionDAL.Id = 0;
            }

            foreach (var actionPermissionDAL in userGroupDal.ACTION_ACCESS)
            {
                actionPermissionDAL.Id = 0;
            }

            var newUserGroupEntityEntry = entities.UserGroupSet.Add(userGroupDal);
            entities.SaveChanges();
            var newUserGroup = newUserGroupEntityEntry.Entity;
            return (int)newUserGroup.Id;

            // using (var scope = new QPConnectionScope())
            // {
            //     return Common.CopyUserGroup(group.Id, group.Name, currentUserId, scope.DbConnection);
            // }
        }

        internal static IEnumerable<ListItem> GetSimpleList(int[] groupIDs)
        {
            return GetAllGroups().Where(g => groupIDs.Contains(g.Id)).Select(g => new ListItem { Value = g.Id.ToString(), Text = g.Name }).ToArray();
        }

        internal static void Delete(int id)
        {
            DefaultRepository.Delete<UserGroupDAL>(id);
        }
    }
}
