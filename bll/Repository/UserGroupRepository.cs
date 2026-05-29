using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Quantumart.QP8.BLL.Helpers;
using Quantumart.QP8.BLL.ListItems;
using Quantumart.QP8.BLL.Repository.ArticleRepositories;
using Quantumart.QP8.BLL.Repository.ContentRepositories;
using Quantumart.QP8.DAL;
using Quantumart.QP8.DAL.Entities;
using Quantumart.QP8.Utils;
using EntityState = Microsoft.EntityFrameworkCore.EntityState;

namespace Quantumart.QP8.BLL.Repository
{
    internal static class UserGroupRepository
    {
        internal static IEnumerable<UserGroup> GetAllGroups() => QPContext.Map<UserGroup[]>(QPContext.EFContext.UserGroupSet.ToList());

        internal static IEnumerable<UserGroup> GetNtGroups()
        {
            var groups = QPContext
                .EFContext
                .UserGroupSet
                .Include(x => x.ParentGroupToGroupBinds).ThenInclude(y => y.ParentGroup)
                .Where(f => f.NtGroup != null);
            return QPContext.Map<UserGroup[]>(groups.ToList());
        }

        internal static IEnumerable<UserGroupListItem> List(ListCommand cmd, out int totalRecords, List<int> selectedIds)
        {
            using var scope = new QPConnectionScope();
            cmd.SortExpression = !string.IsNullOrWhiteSpace(cmd.SortExpression) ? TranslateHelper.TranslateSortExpression(cmd.SortExpression) : null;
            var rows = Common.GetUserGroupPage(scope.DbConnection, selectedIds, cmd.SortExpression, cmd.FilterExpression, cmd.StartRecord, cmd.PageSize, out totalRecords);
            return QPContext.Map<UserGroupListItem[]>(rows.ToList());
        }

        internal static IEnumerable<UserGroup> GetEveryoneGroups() => QPContext.Map<UserGroup[]>(
            QPContext.EFContext.UserGroupSet.Where(x => x.IsReadOnly && x.BuiltIn).ToList()
            );


        /// <summary>
        /// Возвращает список по ids
        /// </summary>
        /// <returns></returns>
        internal static IEnumerable<UserGroup> GetList(IEnumerable<int> ids)
        {
            IEnumerable<decimal> decIDs = Converter.ToDecimalCollection(ids).Distinct().ToArray();
            return QPContext.Map<UserGroup[]>(QPContext.EFContext.UserGroupSet.Where(f => decIDs.Contains(f.Id)).ToList());
        }

        internal static UserGroup GetPropertiesById(int id)
        {
            return QPContext.Map<UserGroup>(QPContext.EFContext.UserGroupSet
                .Include(x => x.ParentGroupToGroupBinds).ThenInclude(y => y.ParentGroup)
                .Include(x => x.ChildGroupToGroupBinds).ThenInclude(y => y.ChildGroup)
                .Include(x => x.UserGroupBinds).ThenInclude(y => y.User)
                .Include(x => x.LastModifiedByUser)
                .SingleOrDefault(g => g.Id == id)
            );
        }

        internal static UserGroup GetById(int id)
        {
            return QPContext.Map<UserGroup>(QPContext.EFContext.UserGroupSet.SingleOrDefault(g => g.Id == id));
        }

        internal static UserGroup UpdateProperties(UserGroup group)
        {
            var entities = QPContext.EFContext;
            var dal = QPContext.Map<UserGroupDAL>(group);
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
            foreach (var u in QPContext.Map<UserDAL[]>(group.Users.ToList()))
            {
                if (!indbUserIDs.Contains(u.Id))
                {
                    var bind = new UserUserGroupBindDAL { UserId = u.Id, UserGroupId = group.Id };
                    dal.UserGroupBinds.Add(bind);
                }
            }

            entities.SaveChanges();
            return QPContext.Map<UserGroup>(dal);
        }

        internal static UserGroup SaveProperties(UserGroup group)
        {
            var entities = QPContext.EFContext;
            var dal = QPContext.Map<UserGroupDAL>(group);
            dal.LastModifiedBy = QPContext.CurrentUserId;
            using (new QPConnectionScope())
            {
                dal.Created = Common.GetSqlDate(QPConnectionScope.Current.DbConnection);
                dal.Modified = dal.Created;
            }

            entities.Entry(dal).State = EntityState.Added;

            if (group.ParentGroup != null)
            {
                var parentDal = QPContext.Map<UserGroupDAL>(group.ParentGroup);
                entities.UserGroupSet.Attach(parentDal);
                var bind = new GroupToGroupBindDAL { ParentGroup = parentDal, ChildGroup = dal };
                dal.ParentGroupToGroupBinds.Add(bind);
            }

            foreach (var u in QPContext.Map<UserDAL[]>(group.Users.ToList()))
            {
                entities.UserSet.Attach(u);
                var bind = new UserUserGroupBindDAL { UserGroup = dal, User = u };
                dal.UserGroupBinds.Add(bind);
            }

            entities.SaveChanges();
            return QPContext.Map<UserGroup>(dal);
        }

        /// <summary>
        /// Отфильтровать пользователей прямо или косвенно входящих в группу администраторов
        /// </summary>
        internal static IEnumerable<int> SelectAdminDescendantGroupUserIDs(IEnumerable<int> userIds, int groupId)
        {
            using var scope = new QPConnectionScope();
            return Common.UserGroups_SelectAdminDescendantGroupUserIDs(userIds, groupId, scope.DbConnection);
        }

        /// <summary>
        /// Входит ли группа в иерархию группы Администраторы
        /// </summary>
        internal static bool IsGroupAdminDescendant(int groupId)
        {
            using var scope = new QPConnectionScope();
            return Common.UserGroups_IsGroupAdminDescendant(groupId, scope.DbConnection);
        }

        /// <summary>
        /// Проверит возможность образования цикла в иерархии групп
        /// </summary>
        internal static bool IsCyclePossible(int groupId, int parentGroupId)
        {
            using var scope = new QPConnectionScope();
            return Common.UserGroups_IsCyclePossible(groupId, parentGroupId, scope.DbConnection);
        }

        /// <summary>
        /// Отфильтровать пользователей прямо входящих в группы использующие параллельный Workflow
        /// </summary>
        internal static IEnumerable<int> SelectWorkflowGroupUserIDs(int[] userIds)
        {
            using var scope = new QPConnectionScope();
            return Common.UserGroups_SelectWorkflowGroupUserIDs(userIds, scope.DbConnection);
        }

        /// <summary>
        /// Возвращает иерархию группы Администраторы
        /// </summary>
        internal static IEnumerable<DataRow> GetAdministratorsHierarhy()
        {
            using var scope = new QPConnectionScope();
            return Common.UserGroups_GetAdministratorsHierarhy(scope.DbConnection);
        }

        internal static int CopyGroup(UserGroup group, int currentUserId)
        {
            var entities = QPContext.EFContext;

            var userGroupDal = entities.UserGroupSet
                .Include(p => p.UserGroupBinds)
                .Include(p => p.ParentGroupToGroupBinds)
                .Single(x => x.Id == group.Id);
            entities.Entry(userGroupDal).Collection(p => p.SiteAccess).Load();
            entities.Entry(userGroupDal).Collection(p => p.FolderAccess).Load();
            entities.Entry(userGroupDal).Collection(p => p.ENTITY_TYPE_ACCESS).Load();
            entities.Entry(userGroupDal).Collection(p => p.ACTION_ACCESS).Load();

            entities.Entry(userGroupDal).State = EntityState.Detached;
            userGroupDal.Id = 0;
            userGroupDal.Name = group.Name;
            userGroupDal.Created = userGroupDal.Modified = DateTime.Now;
            userGroupDal.LastModifiedBy = currentUserId;
            userGroupDal.BuiltIn = false;
            userGroupDal.IsReadOnly = false;

            foreach (var sitePermissionDAL in userGroupDal.SiteAccess)
            {
                entities.Entry(sitePermissionDAL).State = EntityState.Detached;
                sitePermissionDAL.Created = sitePermissionDAL.Modified = DateTime.Now;
                sitePermissionDAL.Id = 0;
            }

            foreach (var folderPermissionDAL in userGroupDal.FolderAccess)
            {
                entities.Entry(folderPermissionDAL).State = EntityState.Detached;
                folderPermissionDAL.Created = folderPermissionDAL.Modified = DateTime.Now;
                folderPermissionDAL.Id = 0;
            }

            foreach (var entityTypePermissionDAL in userGroupDal.ENTITY_TYPE_ACCESS)
            {
                entities.Entry(entityTypePermissionDAL).State = EntityState.Detached;
                entityTypePermissionDAL.Created = entityTypePermissionDAL.Modified = DateTime.Now;
                entityTypePermissionDAL.Id = 0;
            }

            foreach (var actionPermissionDAL in userGroupDal.ACTION_ACCESS)
            {
                entities.Entry(actionPermissionDAL).State = EntityState.Detached;
                actionPermissionDAL.Created = actionPermissionDAL.Modified = DateTime.Now;
                actionPermissionDAL.Id = 0;
            }

            var newUserGroupEntityEntry = entities.UserGroupSet.Add(userGroupDal);
            entities.SaveChanges();
            var newUserGroup = newUserGroupEntityEntry.Entity;
            var result = (int)newUserGroup.Id;
            ContentRepository.CopyAccessForGroup(group.Id, result);
            ArticleRepository.CopyAccessForGroup(group.Id, result);

            return result;



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
            var entities = QPContext.EFContext;
            var group = entities
                .UserGroupSet
                .Include(x => x.UserGroupBinds)
                .Include(x => x.ParentGroupToGroupBinds)
                .Include(x => x.ChildGroupToGroupBinds)
                .FirstOrDefault(x => x.Id == id);

            if (group == null)
            {
                return;
            }

            foreach (var userGroupBind in group.UserGroupBinds)
            {
                entities.Remove(userGroupBind);
            }

            foreach (var parentGroupToGroupBind in group.ParentGroupToGroupBinds)
            {
                entities.Remove(parentGroupToGroupBind);
            }

            foreach (var childGroupToGroupBind in group.ChildGroupToGroupBinds)
            {
                entities.Remove(childGroupToGroupBind);
            }

            entities.Remove(group);
            entities.SaveChanges();

            // DefaultRepository.Delete<UserGroupDAL>(id);
        }
    }
}
