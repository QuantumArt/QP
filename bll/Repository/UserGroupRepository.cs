using System.Collections.Generic;
using System.Data;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Quantumart.QP8.BLL.Facades;
using Quantumart.QP8.BLL.Helpers;
using Quantumart.QP8.BLL.Services.DTO;
using Quantumart.QP8.DAL;
using Quantumart.QP8.Utils;
using EntityState = Microsoft.EntityFrameworkCore.EntityState;

namespace Quantumart.QP8.BLL.Repository
{
    internal static class UserGroupRepository
    {
        internal static IEnumerable<UserGroup> GetAllGroups() => MapperFacade.UserGroupMapper.GetBizList(QPContext.EFContext.UserGroupSet.ToList());

        internal static IEnumerable<UserGroup> GetNtGroups()
        {
            var groups = QPContext.EFContext.UserGroupSet.Include("ParentGroups").Where(f => f.NtGroup != null);
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
                .Include(x => x.ParentGroupToGroupBinds)
                .ThenInclude(y => y.ParentGroup)
                .Include(x => x.ChildGroupToGroupBinds)
                .ThenInclude(y => y.ChildGroup)
                .Include(x => x.UserGroupBinds)
                .ThenInclude(y => y.User)
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
            foreach (var pg in dalDb.ParentGroups.ToArray())
            {
                if (group.ParentGroup == null || pg.Id != group.ParentGroup.Id)
                {
                    entities.UserGroupSet.Attach(pg);
#warning Закомментировано при переезде на EF CORE. Надо пофиксить и раскомментить.
                    // dalDb.ParentGroups.Remove(pg);
                }
            }

            if (group.ParentGroup != null)
            {
                if (dalDb.ParentGroups.All(g => g.Id != group.ParentGroup.Id))
                {
                    var dalParent = entities.UserGroupSet.Single(g => g.Id == group.ParentGroup.Id);
                    entities.UserGroupSet.Attach(dalParent);
                    #warning Закомментировано при переезде на EF CORE. Надо пофиксить и раскомментить.
                    // dal.ParentGroups.Add(dalParent);
                }
            }

            var inmemoryUserIDs = new HashSet<decimal>(group.Users.Select(u => Converter.ToDecimal(u.Id)));
            var indbUserIDs = new HashSet<decimal>(dalDb.Users.Select(u => u.Id));
            foreach (var u in dalDb.Users.ToArray())
            {
                if (!inmemoryUserIDs.Contains(u.Id))
                {
                    entities.UserSet.Attach(u);
#warning Закомментировано при переезде на EF CORE. Надо пофиксить и раскомментить.
                    // dalDb.Users.Remove(u);
                }
            }
            foreach (var u in MapperFacade.UserMapper.GetDalList(group.Users.ToList()))
            {
                if (!indbUserIDs.Contains(u.Id))
                {
                    entities.UserSet.Attach(u);
#warning Закомментировано при переезде на EF CORE. Надо пофиксить и раскомментить.
                    // dal.Users.Add(u);
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
#warning Закомментировано при переезде на EF CORE. Надо пофиксить и раскомментить.
                // dal.ParentGroups.Add(parentDal);
            }

            foreach (var u in MapperFacade.UserMapper.GetDalList(group.Users.ToList()))
            {
                entities.UserSet.Attach(u);
#warning Закомментировано при переезде на EF CORE. Надо пофиксить и раскомментить.
                // dal.Users.Add(u);
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
            using (var scope = new QPConnectionScope())
            {
                return Common.CopyUserGroup(group.Id, group.Name, currentUserId, scope.DbConnection);
            }
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
