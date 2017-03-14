using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Transactions;
using System.Web;
using Microsoft.Practices.Unity;
using QP8.Infrastructure.Logging;
using Quantumart.QP8.BLL.Extensions;
using Quantumart.QP8.BLL.Facades;
using Quantumart.QP8.BLL.Repository;
using Quantumart.QP8.BLL.Services;
using Quantumart.QP8.Configuration;
using Quantumart.QP8.Constants.Mvc;
using Quantumart.QP8.DAL;
using Quantumart.QP8.Security;
using Quantumart.QP8.Utils;

namespace Quantumart.QP8.BLL
{
    public interface IContextStorage
    {
        T GetValue<T>(string key);

        void SetValue<T>(T value, string key);

        void ResetValue(string key);

        IEnumerable<string> Keys { get; }
    }

    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public class QPContext
    {
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        public static QP8Entities EFContext
        {
            get
            {
                QP8Entities dbContext;
                if (QPConnectionScope.Current == null)
                {
                    dbContext = new QP8Entities(CurrentDbConnectionStringForEntities);
                    dbContext.ExecuteStoreCommand(QPConnectionScope.SetIsolationLevelCommandText);
                }
                else
                {
                    dbContext = new QP8Entities(QPConnectionScope.Current.EfConnection);
                }

                dbContext.ContextOptions.LazyLoadingEnabled = false;
                return dbContext;
            }
        }

        public static string GetRecordXmlFilePath()
        {
            return $"{QPConfiguration.TempDirectory}{CurrentCustomerCode}.xml";
        }

        private static T GetValueFromStorage<T>(T threadStorageData, string key)
        {
            if (UseThreadStorage)
            {
                return threadStorageData;
            }

            if (_externalContextStorage != null && _externalContextStorageKeys != null && _externalContextStorageKeys.Contains(key))
            {
                return _externalContextStorage.GetValue<T>(key);
            }

            if (HttpContext.Current == null)
            {
                return threadStorageData;
            }

            return (T)HttpContext.Current.Items[key];
        }

        private static void SetValueToStorage<T>(ref T threadStorage, T value, string key)
        {
            if (UseThreadStorage)
            {
                threadStorage = value;
            }

            if (_externalContextStorage != null && _externalContextStorageKeys != null && _externalContextStorageKeys.Contains(key))
            {
                _externalContextStorage.SetValue(value, key);
            }

            if (HttpContext.Current == null)
            {
                threadStorage = value;
            }
            else
            {
                HttpContext.Current.Items[key] = value;
            }
        }

        internal static Dictionary<int, Field> GetFieldCache()
        {
            return GetValueFromStorage(_fieldCache, HttpContextItems.FieldCache);
        }

        internal static void SetFieldCache(Dictionary<int, Field> value)
        {
            SetValueToStorage(ref _fieldCache, value, HttpContextItems.FieldCache);
        }

        internal static Dictionary<int, StatusType> GetStatusTypeCache()
        {
            return GetValueFromStorage(_statusTypeCache, HttpContextItems.StatusType);
        }

        internal static void SetStatusTypeCache(Dictionary<int, StatusType> value)
        {
            SetValueToStorage(ref _statusTypeCache, value, HttpContextItems.StatusType);
        }

        internal static Dictionary<int, User> GetUserCache()
        {
            return GetValueFromStorage(_userCache, HttpContextItems.User);
        }

        internal static void SetUserCache(Dictionary<int, User> value)
        {
            SetValueToStorage(ref _userCache, value, HttpContextItems.User);
        }

        internal static Dictionary<int, Content> GetContentCache()
        {
            return GetValueFromStorage(_contentCache, HttpContextItems.ContentCache);
        }

        internal static void SetContentCache(Dictionary<int, Content> value)
        {
            SetValueToStorage(ref _contentCache, value, HttpContextItems.ContentCache);
        }

        internal static Dictionary<int, Site> GetSiteCache()
        {
            return GetValueFromStorage(_siteCache, HttpContextItems.SiteCache);
        }

        internal static void SetSiteCache(Dictionary<int, Site> value)
        {
            SetValueToStorage(ref _siteCache, value, HttpContextItems.SiteCache);
        }

        internal static Dictionary<int, List<int>> GetContentFieldCache()
        {
            return GetValueFromStorage(_contentFieldCache, HttpContextItems.ContentFieldCache);
        }

        internal static void SetContentFieldCache(Dictionary<int, List<int>> value)
        {
            SetValueToStorage(ref _contentFieldCache, value, HttpContextItems.ContentFieldCache);
        }

        internal static void ClearInternalStructureCache()
        {
            _siteCache = null;
            _contentCache = null;
            _contentFieldCache = null;
            _fieldCache = null;
            _statusTypeCache = null;
            _userCache = null;
        }

        internal static void ClearExternalStructureCache()
        {
            if (_externalContextStorage?.Keys != null)
            {
                foreach (var key in _externalContextStorage.Keys)
                {
                    _externalContextStorage.ResetValue(key);
                }
            }
        }

        internal static void LoadStructureCache(bool clearExternal)
        {
            ClearInternalStructureCache();
            if (clearExternal)
            {
                ClearExternalStructureCache();
            }

            if (GetSiteCache() == null)
            {
                SetSiteCache(SiteRepository.GetAll().ToDictionary(n => n.Id));
            }

            if (GetContentCache() == null)
            {
                SetContentCache(ContentRepository.GetAll().ToDictionary(n => n.Id));
            }

            IEnumerable<Field> fields = new Field[0];
            if (GetFieldCache() == null || GetContentFieldCache() == null)
            {
                fields = FieldRepository.GetAll();
            }

            if (GetFieldCache() == null)
            {
                SetFieldCache(fields.ToDictionary(n => n.Id));
            }

            if (GetStatusTypeCache() == null)
            {
                SetStatusTypeCache(StatusTypeRepository.GetAll().ToDictionary(n => n.Id));
            }

            if (GetUserCache() == null)
            {
                SetUserCache(UserRepository.GetAllUsersList().ToDictionary(n => n.Id));
            }

            if (GetContentFieldCache() == null)
            {
                var dict = new Dictionary<int, List<int>>();
                foreach (var item in fields)
                {
                    if (dict.ContainsKey(item.ContentId))
                    {
                        dict[item.ContentId].Add(item.Id);
                    }
                    else
                    {
                        dict.Add(item.ContentId, new List<int> { item.Id });
                    }
                }

                SetContentFieldCache(dict);
            }
        }

        [ThreadStatic]
        private static int? _currentUserId;

        [ThreadStatic]
        private static bool? _isAdmin;

        [ThreadStatic]
        private static int[] _currentGroupIds;

        [ThreadStatic]
        private static bool? _canUnlockItems;

        [ThreadStatic]
        private static bool? _isLive;

        [ThreadStatic]
        private static Dictionary<int, Site> _siteCache;

        [ThreadStatic]
        private static Dictionary<int, Content> _contentCache;

        [ThreadStatic]
        private static Dictionary<int, Field> _fieldCache;

        [ThreadStatic]
        private static Dictionary<int, StatusType> _statusTypeCache;

        [ThreadStatic]
        private static Dictionary<int, User> _userCache;

        [ThreadStatic]
        private static Dictionary<int, List<int>> _contentFieldCache;

        [ThreadStatic]
        private static string _currentCustomerCode;

        [ThreadStatic]
        private static Version _currentSqlVersion;

        [ThreadStatic]
        private static QPConnectionScope _currentConnectionScope;

        [ThreadStatic]
        private static string _currentDbConnectionString;

        [ThreadStatic]
        private static BackendActionContext _backendActionContext;

        [ThreadStatic]
        private static bool _useThreadStorage;

        private static void SetCurrentUserIdValueToStorage(int? value)
        {
            SetValueToStorage(ref _currentUserId, value, HttpContextItems.CurrentUserIdKey);
        }

        private static void SetCurrentGroupIdsValueToStorage(int[] value)
        {
            SetValueToStorage(ref _currentGroupIds, value, HttpContextItems.CurrentGroupIdsKey);
        }

        private static void SetIsAdminValueToStorage(bool? value)
        {
            SetValueToStorage(ref _isAdmin, value, HttpContextItems.IsAdminKey);
        }

        private static void SetCanUnlockItemsValueToStorage(bool? value)
        {
            SetValueToStorage(ref _canUnlockItems, value, HttpContextItems.CanUnlockItemsKey);
        }

        private static void SetCurrentCustomerCodeValueToStorage(string value)
        {
            SetValueToStorage(ref _currentCustomerCode, value, HttpContextItems.CurrentCustomerCodeKey);
        }

        private static void SetCurrentSqlVersionValueToStorage(Version value)
        {
            SetValueToStorage(ref _currentSqlVersion, value, HttpContextItems.CurrentSqlVersionKey);
        }

        public static int CurrentUserId
        {
            get
            {
                var result = GetValueFromStorage(_currentUserId, HttpContextItems.CurrentUserIdKey);
                if (result == null)
                {
                    result = (HttpContext.Current.User.Identity as QpIdentity)?.Id;
                    SetCurrentUserIdValueToStorage(result);
                }

                return result ?? 0;
            }
            set
            {
                if (value == 0)
                {
                    SetCurrentUserIdValueToStorage(null);
                    SetIsAdminValueToStorage(null);
                    SetCurrentGroupIdsValueToStorage(new int[0]);
                }
                else
                {
                    SetCurrentUserIdValueToStorage(value);
                    using (new QPConnectionScope())
                    {
                        SetIsAdminValueToStorage(Common.IsAdmin(QPConnectionScope.Current.DbConnection, value));
                        SetCurrentGroupIdsValueToStorage(Common.GetGroupIds(QPConnectionScope.Current.DbConnection, value));
                    }
                }
            }
        }

        public static bool IsAdmin
        {
            get
            {
                var result = GetValueFromStorage(_isAdmin, HttpContextItems.IsAdminKey);
                if (result == null)
                {
                    using (new QPConnectionScope())
                    {
                        result = Common.IsAdmin(QPConnectionScope.Current.DbConnection, CurrentUserId);
                    }

                    SetIsAdminValueToStorage(result);
                }

                return (bool)result;
            }
            set
            {
                SetIsAdminValueToStorage(value);
            }
        }

        public static int[] CurrentGroupIds
        {
            get
            {
                var result = GetValueFromStorage(_currentGroupIds, HttpContextItems.CurrentGroupIdsKey);
                if (result == null)
                {
                    using (new QPConnectionScope())
                    {
                        result = Common.GetGroupIds(QPConnectionScope.Current.DbConnection, CurrentUserId);
                    }

                    SetCurrentGroupIdsValueToStorage(result);
                }

                return result;
            }
            set
            {
                SetCurrentGroupIdsValueToStorage(value);
            }
        }

        public static bool CanUnlockItems
        {
            get
            {
                var result = GetValueFromStorage(_canUnlockItems, HttpContextItems.CanUnlockItemsKey);
                if (result == null)
                {
                    using (new QPConnectionScope())
                    {
                        result = Common.CanUnlockItems(QPConnectionScope.Current.DbConnection, CurrentUserId);
                    }
                    SetCanUnlockItemsValueToStorage(result);
                }
                return (bool)result;
            }
            set
            {
                SetCanUnlockItemsValueToStorage(value);
            }
        }

        public static bool IsLive
        {
            get
            {
                var value = GetValueFromStorage(_isLive, HttpContextItems.IsLiveKey);
                return value.HasValue && value.Value;
            }
            set
            {
                SetValueToStorage(ref _isLive, value, HttpContextItems.IsLiveKey);
            }
        }

        public static string CurrentUserName => (HttpContext.Current.User.Identity as QpIdentity)?.Name;

        public static string CurrentCustomerCode
        {
            get
            {
                var result = GetValueFromStorage(_currentCustomerCode, HttpContextItems.CurrentCustomerCodeKey);
                if (result == null)
                {
                    if (HttpContext.Current != null && CurrentUserIdentity != null)
                    {
                        result = CurrentUserIdentity.CustomerCode;
                        SetCurrentCustomerCodeValueToStorage(result);
                    }
                }

                return result;
            }
            set
            {
                SetCurrentCustomerCodeValueToStorage(value);
            }
        }

        internal static QPConnectionScope CurrentConnectionScope
        {
            get
            {
                return GetValueFromStorage(_currentConnectionScope, HttpContextItems.CurrentConnectionScopeKey);
            }
            set
            {
                SetValueToStorage(ref _currentConnectionScope, value, HttpContextItems.CurrentConnectionScopeKey);
            }
        }

        internal static BackendActionContext BackendActionContext
        {
            get
            {
                return GetValueFromStorage(_backendActionContext, HttpContextItems.BackendActionContextKey);
            }
            set
            {
                SetValueToStorage(ref _backendActionContext, value, HttpContextItems.BackendActionContextKey);
            }
        }

        public static Version CurrentSqlVersion
        {
            get
            {
                var result = GetValueFromStorage(_currentSqlVersion, HttpContextItems.CurrentSqlVersionKey);
                if (result == null)
                {
                    if (HttpContext.Current != null && CurrentUserIdentity != null)
                    {
                        result = EFContext.GetSqlServerVersion();
                        SetCurrentSqlVersionValueToStorage(result);
                    }
                }

                return result;
            }
            set
            {
                SetCurrentSqlVersionValueToStorage(value);
            }
        }

        public static QpIdentity CurrentUserIdentity => HttpContext.Current != null && HttpContext.Current.User != null ? HttpContext.Current.User.Identity as QpIdentity : null;

        public static string CurrentDbConnectionString
        {
            get
            {
                var result = GetValueFromStorage(_currentDbConnectionString, HttpContextItems.CurrentDbConnectionStringKey);
                if (result == null)
                {
                    result = QPConfiguration.GetConnectionString(CurrentCustomerCode);
                    SetValueToStorage(ref _currentDbConnectionString, result, HttpContextItems.CurrentDbConnectionStringKey);
                }

                return result;
            }
            set
            {
                SetValueToStorage(ref _currentDbConnectionString, value, HttpContextItems.CurrentDbConnectionStringKey);
            }
        }

        private static string CurrentDbConnectionStringForEntities => PreparingDbConnectionStringForEntities(CurrentDbConnectionString);

        private static string PreparingDbConnectionStringForEntities(string connectionString)
        {
            return $"metadata=res://*/QP8Model.csdl|res://*/QP8Model.ssdl|res://*/QP8Model.msl;provider=System.Data.SqlClient;provider connection string=\"{connectionString}\"";
        }

        public static bool CheckCustomerCode(string customerCode)
        {
            return QPConfiguration.XmlConfig.Descendants("customer").Select(n => n.Attribute("customer_name")?.Value).Contains(customerCode);
        }

        public static QpUser Authenticate(LogOnCredentials data, ref int errorCode, out string message)
        {
            QpUser resultUser = null;
            message = string.Empty;

            var sqlCn = QPConfiguration.GetConnectionString(data.CustomerCode);
            using (var dbContext = new QP8Entities(PreparingDbConnectionStringForEntities(sqlCn)))
            {
                try
                {
                    var dbUser = dbContext.Authenticate(data.UserName, data.Password, data.UseAutoLogin, false);
                    var user = MapperFacade.UserMapper.GetBizObject(dbUser);
                    if (user != null)
                    {
                        resultUser = new QpUser
                        {
                            Id = user.Id,
                            Name = user.LogOn,
                            CustomerCode = data.CustomerCode,
                            LanguageId = user.LanguageId,
                        };

                        using (var cn = new SqlConnection(sqlCn))
                        {
                            cn.Open();
                            resultUser.Roles = QpRolesManager.GetRolesForUser(Common.IsAdmin(cn, user.Id));
                        }

                        dbUser.LastLogOn = DateTime.Now;
                        CreateSuccessfulSession(user, dbContext);
                        var context = HttpContext.Current;
                        if (context != null)
                        {
                            context.Items[HttpContextItems.DbContext] = dbContext;
                            QP7Service.SetPassword(data.Password);
                        }
                    }
                    else
                    {
                        CreateFailedSession(data, dbContext);
                    }
                }
                catch (SqlException ex)
                {
                    message = ex.Message;
                    errorCode = ex.State;
                    CreateFailedSession(data, dbContext);
                }
            }

            return resultUser;
        }

        public static string LogOut()
        {
            using (var transaction = new TransactionScope(TransactionScopeOption.Suppress, new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted }))
            {
                using (new QPConnectionScope())
                {
                    var dbContext = EFContext;
                    CloseUserSessions(CurrentUserId, dbContext, DateTime.Now);
                    dbContext.SaveChanges();
                }

                var loginUrl = AuthenticationHelper.LogOut();
                transaction.Complete();
                HttpContext.Current.Session.Abandon();
                return loginUrl;
            }
        }

        private static void CreateSuccessfulSession(User user, QP8Entities dbContext)
        {
            // сбросить sid и установить EndTime для всех сессий пользователя
            var currentDt = DateTime.Now;

            // закрыть открытые сессии пользователя
            CloseUserSessions(user.Id, dbContext, currentDt);

            // Сохранить новую сессию
            var sessionsLog = new SessionsLog
            {
                AutoLogged = user.AutoLogOn ? 1 : 0,
                Login = user.Name.Left(20),
                UserId = user.Id,
                StartTime = currentDt,

                Browser = HttpContext.Current.Request.ServerVariables[RequestServerVariables.HttpUserAgent].Left(255),
                IP = HttpContext.Current.Request.UserHostAddress,
                ServerName = Environment.MachineName.Left(255)
            };

            var sessionsLogDal = MapperFacade.SessionsLogMapper.GetDalObject(sessionsLog);
            dbContext.AddToSessionsLogSet(sessionsLogDal);
            dbContext.SaveChanges();

            Logger.Log.Debug($"User successfully authenticated: {sessionsLog.ToJsonLog()}");
        }

        private static void CloseUserSessions(decimal userId, QP8Entities dbContext, DateTime currentDt)
        {
            var userSessions = dbContext.SessionsLogSet.Where(s => s.UserId == userId && !s.EndTime.HasValue && !s.IsQP7).ToArray();
            foreach (var us in userSessions)
            {
                us.EndTime = currentDt;
                us.Sid = null;
            }
        }

        private static void CreateFailedSession(LogOnCredentials data, QP8Entities dbContext)
        {
            var sessionsLog = new SessionsLog
            {
                AutoLogged = data.UseAutoLogin ? 1 : 0,
                Login = data.UserName.Left(20),
                UserId = null,
                StartTime = DateTime.Now,

                Browser = HttpContext.Current.Request.ServerVariables[RequestServerVariables.HttpUserAgent].Left(255),
                IP = HttpContext.Current.Request.UserHostAddress,
                ServerName = Environment.MachineName.Left(255)
            };

            var sessionsLogDal = MapperFacade.SessionsLogMapper.GetDalObject(sessionsLog);
            dbContext.AddToSessionsLogSet(sessionsLogDal);
            dbContext.SaveChanges();
        }

        public static IUnityContainer CurrentUnityContainer { get; private set; }

        public static void SetUnityContainer(IUnityContainer container)
        {
            CurrentUnityContainer = container;
        }

        private static IContextStorage _externalContextStorage;

        private static HashSet<string> _externalContextStorageKeys;

        public static bool UseThreadStorage
        {
            get { return _useThreadStorage; }
            set { _useThreadStorage = value; }
        }

        public static IContextStorage ExternalContextStorage
        {
            set
            {
                if (value.Keys == null || !value.Keys.Any())
                {
                    throw new ArgumentException("Keys collection is empty");
                }

                _externalContextStorage = value;
                _externalContextStorageKeys = new HashSet<string>(value.Keys);
            }
        }
    }
}
