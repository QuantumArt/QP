using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Quantumart.QP8.BLL.Facades;
using Quantumart.QP8.BLL.Services.DTO;
using Quantumart.QP8.DAL;
using Quantumart.QP8.DAL.DTO;
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
            var users = QPContext.EFContext.UserSet.Include("Groups").Where(u => u.NTLogOn != null).ToList();
            return MapperFacade.UserMapper.GetBizList(users);
        }

        internal static bool CheckAuthenticate(string login, string password) => QPContext.EFContext.Authenticate(login, password, false, false) != null;

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
                .Include("Groups")
                .Include("LastModifiedByUser")
                .SingleOrDefault(u => u.Id == id);

            return MapperFacade.UserMapper.GetBizObject(dal);
        }

        internal static User UpdateProfile(User user) => UpdateUser(user, true);

        internal static User UpdateProperties(User user) => UpdateUser(user);

        public static bool NewPasswordMathCurrentPassword(int userId, string newPassword)
        {
            using (new QPConnectionScope())
            {
                return Common.NewPasswordMathCurrentPassword(QPConnectionScope.Current.DbConnection, userId, newPassword);
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

            entities.UserSet.Attach(dal);
            entities.ObjectStateManager.ChangeObjectState(dal, EntityState.Modified);
            if (!profileOnly)
            {
                // Save Groups
                var dalDb = entities.UserSet.Include("Groups").Single(u => u.Id == dal.Id);
                var inmemoryGroupIDs = new HashSet<decimal>(user.Groups.Select(g => Converter.ToDecimal(g.Id)));
                var indbGroupIDs = new HashSet<decimal>(dalDb.Groups.Select(g => g.Id));
                foreach (var g in dalDb.Groups.ToArray())
                {
                    if (!inmemoryGroupIDs.Contains(g.Id) && !g.IsReadOnly && !(g.BuiltIn && user.BuiltIn))
                    {
                        entities.UserGroupSet.Attach(g);
                        dalDb.Groups.Remove(g);
                    }
                }
                foreach (var g in MapperFacade.UserGroupMapper.GetDalList(user.Groups.ToList()))
                {
                    if (!indbGroupIDs.Contains(g.Id))
                    {
                        entities.UserGroupSet.Attach(g);
                        dal.Groups.Add(g);
                    }
                }

                //-------------------
            }

            // User Default Filters
            foreach (var f in entities.UserDefaultFilterSet.Where(r => r.UserId == dal.Id))
            {
                entities.UserDefaultFilterSet.DeleteObject(f);
            }
            foreach (var f in MapUserDefaultFilter(user, dal))
            {
                entities.UserDefaultFilterSet.AddObject(f);
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

        internal static string GeneratePassword() => "aA1!" + Guid.NewGuid().ToString("N").Substring(0, 16);

        internal static void UpdatePassword(int userId, string password)
        {
            using (new QPConnectionScope())
            {
                Common.UpdatePassword(QPConnectionScope.Current.DbConnection, userId, password);
            }
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

            entities.UserSet.AddObject(dal);
            entities.SaveChanges();

            // Save Groups
            foreach (var s in MapperFacade.UserGroupMapper.GetDalList(user.Groups.ToList()))
            {
                entities.UserGroupSet.Attach(s);
                dal.Groups.Add(s);
            }

            //---------------

            // User Default Filters
            foreach (var f in MapUserDefaultFilter(user, dal))
            {
                entities.UserDefaultFilterSet.AddObject(f);
            }

            //----------------

            entities.SaveChanges();
            if (!string.IsNullOrEmpty(user.Password))
            {
                UpdatePassword(user.Id, user.Password);
            }

            var updated = MapperFacade.UserMapper.GetBizObject(dal);
            return updated;
        }

        /// <summary>
        /// Создает копию пользователя
        /// </summary>
        internal static int CopyUser(User user, int currentUserId)
        {
            using (var scope = new QPConnectionScope())
            {
                return Common.CopyUser(user.Id, user.LogOn, currentUserId, scope.DbConnection);
            }
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
                .Where(f => f.ArticleIDs.Any())
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
