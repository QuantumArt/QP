using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Quantumart.QP8.BLL.Facades;
using Quantumart.QP8.BLL.ListItems;
using Quantumart.QP8.BLL.Services.DTO;
using Quantumart.QP8.Constants;
using Quantumart.QP8.DAL;
using Quantumart.QP8.DAL.DTO;
using Quantumart.QP8.DAL.Entities;
using Quantumart.QP8.Utils;

namespace Quantumart.QP8.BLL.Repository
{
    internal class UserRepository
    {
        internal static IEnumerable<UserListItem> List(ListCommand cmd, UserListFilter filter, IEnumerable<int> selectedIDs, out int totalRecords)
        {
            using (var scope = new QPConnectionScope())
            {
                var options = new UserPageOptions
                {
                    SortExpression = !string.IsNullOrWhiteSpace(cmd.SortExpression) ? UserListItem.TranslateSortExpression(cmd.SortExpression) : null,
                    StartRecord = cmd.StartRecord,
                    PageSize = cmd.PageSize,
                    SelectedIDs = selectedIDs
                };

                if (filter != null)
                {
                    options.Email = filter.Email;
                    options.FirstName = filter.FirstName;
                    options.LastName = filter.LastName;
                    options.Login = filter.Login;
                }

                var rows = Common.GetUserPage(scope.DbConnection, options, out totalRecords);
                return MapperFacade.UserListItemRowMapper.GetBizList(rows.ToList());
            }
        }

        /// <summary>
        /// Возвращает список по ids
        /// </summary>
        internal static IEnumerable<User> GetList(IEnumerable<int> ids)
        {
            var result = new List<User>();
            var cache = QPContext.GetUserCache();
            if (cache != null)
            {
                result.AddRange(ids.Select(id => cache.ContainsKey(id) ? cache[id] : GetRealById(id)));
            }
            else
            {
                IEnumerable<decimal> decIDs = Converter.ToDecimalCollection(ids).Distinct().ToArray();
                result = MapperFacade.UserMapper.GetBizList(QPContext.EFContext.UserSet.Where(f => decIDs.Contains(f.Id)).ToList());
            }

            return result;
        }

        internal static List<User> GetNtUsers()
        {
            var users = QPContext.EFContext.UserSet
                .Include(x => x.UserGroupBinds).ThenInclude(y=> y.UserGroup)
                .Where(u => u.NTLogOn != null).ToList();
            return MapperFacade.UserMapper.GetBizList(users);
        }

        internal static bool CheckAuthenticate(string login, string password) => Common.Authenticate(QPConnectionScope.Current.DbConnection, login, password, false, false) != null;

        internal static bool GetUserMustChangePassword(int userId) => QPContext.EFContext.UserSet.Where(w => w.Id == userId).Select(s => s.MustChangePassword).Single();
        internal static User GetById(int id, bool stopRecursion = false)
        {
            var result = GetByIdFromCache(id);
            if (result != null)
            {
                return result;
            }

            return GetRealById(id, stopRecursion);
        }

        internal static User GetByIdFromCache(int id)
        {
            User result = null;
            var cache = QPContext.GetUserCache();
            if (cache != null && cache.ContainsKey(id))
            {
                result = cache[id];
            }

            return result;
        }

        private static User GetRealById(int id, bool stopRecursion = false)
        {
            var user = MapperFacade.UserMapper.GetBizObject(QPContext.EFContext.UserSet.SingleOrDefault(u => u.Id == id));
            if (!stopRecursion && user != null)
            {
                user.LastModifiedByUser = GetById(user.LastModifiedBy, true);
            }

            return user;
        }

        internal static User GetPropertiesById(int id)
        {
            var dal = QPContext.EFContext.UserSet
                .Include(x => x.UserGroupBinds).ThenInclude(y => y.UserGroup)
                .Include(x => x.LastModifiedByUser)
                .SingleOrDefault(u => u.Id == id);

            return MapperFacade.UserMapper.GetBizObject(dal);
        }

        internal static User UpdateProfile(User user) => UpdateUser(user, true);

        internal static User UpdateProperties(User user) => UpdateUser(user);

        public static bool NewPasswordMathCurrentPassword(int userId, string newPassword)
        {
            using (var scope = new QPConnectionScope())
            {
                return Common.NewPasswordMatchCurrentPassword(scope.DbConnection, userId, newPassword);
            }
        }

        private static User UpdateUser(User user, bool profileOnly = false)
        {
            var entities = QPContext.EFContext;
            var dal = MapperFacade.UserMapper.GetDalObject(user);
            dal.LastModifiedBy = QPContext.CurrentUserId;
            using (new QPConnectionScope())
            {
                dal.Modified = Common.GetSqlDate(QPConnectionScope.Current.DbConnection);
            }

            entities.Entry(dal).State = EntityState.Modified;

            if (!profileOnly)
            {
                // Save Groups
                var dalDb = entities.UserSet
                    .Include(x => x.UserGroupBinds).ThenInclude(y => y.UserGroup)
                    .Single(u => u.Id == dal.Id);
                var inmemoryGroupIDs = new HashSet<decimal>(user.Groups.Select(g => Converter.ToDecimal(g.Id)));
                var indbGroupIDs = new HashSet<decimal>(dalDb.Groups.Select(g => g.Id));
                foreach (var g in dalDb.UserGroupBinds.ToArray())
                {
                    if (!inmemoryGroupIDs.Contains(g.UserGroupId) && !g.UserGroup.IsReadOnly && !(g.UserGroup.BuiltIn && user.BuiltIn))
                    {
                        dalDb.UserGroupBinds.Remove(g);
                    }
                }
                foreach (var g in MapperFacade.UserGroupMapper.GetDalList(user.Groups.ToList()))
                {
                    if (!indbGroupIDs.Contains(g.Id))
                    {
                        entities.UserGroupSet.Attach(g);
                        var bind = new UserUserGroupBindDAL { User = dal, UserGroup = g };
                        dal.UserGroupBinds.Add(bind);
                    }
                }

                //-------------------
            }

            // User Default Filters
            foreach (var f in entities.UserDefaultFilterSet.Where(r => r.UserId == dal.Id))
            {
                entities.Entry(f).State = EntityState.Deleted;
            }
            foreach (var f in MapUserDefaultFilter(user, dal))
            {
                entities.Entry(f).State = EntityState.Added;
            }

            //--------------------------

            entities.SaveChanges();

            if (!string.IsNullOrEmpty(user.Password))
            {
                UpdatePassword(user.Id, user.Password);
            }

            var updated = MapperFacade.UserMapper.GetBizObject(dal);
            return updated;
        }

        /// <summary>
        /// Возвращает список всех пользователей
        /// </summary>
        internal static IEnumerable<User> GetAllUsersList()
        {
            return MapperFacade.UserMapper.GetBizList(QPContext.EFContext.UserSet.OrderBy(u => u.LogOn).ToList());
        }

        /// <summary>
        /// Возвращает список всех пользователей с заполненными группами
        /// </summary>
        internal static IEnumerable<User> GetAllUsersListWithGroups()
        {
            return MapperFacade.UserMapper.GetBizList(QPContext.EFContext.UserSet
                .Include(x => x.UserGroupBinds).ThenInclude(y=> y.UserGroup).OrderBy(u => u.LogOn).ToList());
        }

        internal static string GeneratePassword() => "aA1!" + Guid.NewGuid().ToString("N").Substring(0, 16);

        internal static void UpdatePassword(int userId, string password)
        {
            using (new QPConnectionScope())
            {
                Common.UpdatePassword(QPConnectionScope.Current.DbConnection, userId, password);
            }
        }

        private static void ChangeInsertBindTriggerState(bool enable)
        {
            Common.ChangeTriggerState(QPContext.CurrentConnectionScope.DbConnection, "ti_add_to_everyone_group", enable);
        }

        internal static User SaveProperties(User user)
        {
            var entities = QPContext.EFContext;
            var dal = MapperFacade.UserMapper.GetDalObject(user);
            dal.LastModifiedBy = QPContext.CurrentUserId;
            using (new QPConnectionScope())
            {
                dal.Created = Common.GetSqlDate(QPConnectionScope.Current.DbConnection);
                dal.Modified = dal.Created;
                dal.PasswordModified = dal.Created;
            }

            var everyoneGroups = UserGroupRepository.GetEveryoneGroups();
            foreach (var everyoneGroup in everyoneGroups)
            {
                if (dal.UserGroupBinds.All(x => x.UserGroupId != everyoneGroup.Id))
                {
                    var newBind = new UserUserGroupBindDAL();
                    newBind.UserGroupId = everyoneGroup.Id;
                    dal.UserGroupBinds.Add(newBind);
                    entities.Add(newBind);
                }
            }

            entities.Entry(dal).State = EntityState.Added;


            if (QPContext.DatabaseType == DatabaseType.SqlServer)
            {
                ChangeInsertBindTriggerState(false);
            }

            entities.SaveChanges();


            // Save Groups
            foreach (var s in MapperFacade.UserGroupMapper.GetDalList(user.Groups.ToList()))
            {
                entities.UserGroupSet.Attach(s);
                var userGroupBind = new UserUserGroupBindDAL{User = dal, UserGroup = s};
                dal.UserGroupBinds.Add(userGroupBind);
            }

            // User Default Filters
            foreach (var f in MapUserDefaultFilter(user, dal))
            {
                entities.Entry(f).State = EntityState.Added;
            }

            entities.SaveChanges();
            if (!string.IsNullOrEmpty(user.Password))
            {
                UpdatePassword(user.Id, user.Password);
            }

            if (QPContext.DatabaseType == DatabaseType.SqlServer)
            {
                ChangeInsertBindTriggerState(true);
            }

            var updated = MapperFacade.UserMapper.GetBizObject(dal);
            return updated;
        }

        /// <summary>
        /// Создает копию пользователя
        /// </summary>
        internal static int CopyUser(User user, int currentUserId)
        {
            var entities = QPContext.EFContext;

            var userDal = entities
                .UserSet
                .AsNoTracking()
                .Include(x => x.UserGroupBinds)
                .Include(x => x.SiteAccess)
                .Include(x => x.ContentAccess)
                .Include(x => x.AccessRules)
                .Include(x => x.FolderAccess)
                .Include(x => x.ENTITY_TYPE_ACCESS)
                .Include(x => x.ACTION_ACCESS)
                .Include(x => x.DefaultFilter)
                .First(x => x.Id == user.Id);


            userDal.Id = 0;
            userDal.PASSWORD = GeneratePassword();
            userDal.Created = userDal.Modified = DateTime.Now;
            userDal.LastModifiedBy = currentUserId;
            userDal.BuiltIn = false;
            userDal.LogOn = user.LogOn;
            foreach (var contentPermissionDAL in userDal.ContentAccess)
            {
                contentPermissionDAL.Id = 0;
            }

            foreach (var sitePermissionDAL in userDal.SiteAccess)
            {
                sitePermissionDAL.Id = 0;
            }

            foreach (var userDalAccessRule in userDal.AccessRules)
            {
                userDalAccessRule.Id = 0;
            }

            foreach (var folderPermissionDAL in userDal.FolderAccess)
            {
                folderPermissionDAL.Id = 0;
            }

            foreach (var entityTypePermissionDAL in userDal.ENTITY_TYPE_ACCESS)
            {
                entityTypePermissionDAL.Id = 0;
            }

            foreach (var actionPermissionDAL in userDal.ACTION_ACCESS)
            {
                actionPermissionDAL.Id = 0;
            }

            if (QPContext.DatabaseType == DatabaseType.SqlServer)
            {
                ChangeInsertBindTriggerState(false);
            }

            var newUserEntityEntry = entities.UserSet.Add(userDal);
            entities.SaveChanges();

            if (QPContext.DatabaseType == DatabaseType.SqlServer)
            {
                ChangeInsertBindTriggerState(true);
            }

            var newUser = newUserEntityEntry.Entity;
            return (int)newUser.Id;

        }

        internal static IEnumerable<User> GetUsers(IEnumerable<int> userIDs)
        {
            return GetAllUsersList().Where(u => userIDs.Contains(u.Id)).ToArray();
        }

        internal static IEnumerable<ListItem> GetSimpleList(IEnumerable<int> userIDs)
        {
            return GetUsers(userIDs)
                .Select(u => new ListItem { Value = u.Id.ToString(), Text = u.Name })
                .ToArray();
        }

        internal static void Delete(int id)
        {
            DefaultRepository.Delete<UserDAL>(id);
        }

        internal static IEnumerable<UserDefaultFilter> GetContentDefaultFilters(int userId)
        {
            return QPContext.EFContext.UserDefaultFilterSet
                .Where(r => r.UserId == userId)
                .ToArray()
                .GroupBy(r => r.ContentId)
                .Select(g => new UserDefaultFilter
                {
                    ArticleIDs = Converter.ToInt32Collection(g.Select(r => r.ArticleId)).ToList(),
                    ContentId = Converter.ToInt32(g.Key),
                    UserId = userId
                }).ToArray();
        }

        private static IEnumerable<UserDefaultFilterItemDAL> MapUserDefaultFilter(User biz, IQpEntityObject dal)
        {
            return biz.ContentDefaultFilters
                .Where(f => f.ArticleIDs.Any() && f.ContentId.HasValue)
                .SelectMany(f =>
                    f.ArticleIDs.Select(aid => new UserDefaultFilterItemDAL
                        {
                            UserId = dal.Id,
                            ContentId = f.ContentId.Value,
                            ArticleId = aid
                        }
                    )
                );
        }
    }
}
