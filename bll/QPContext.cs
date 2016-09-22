using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Transactions;
using System.Web;
using Microsoft.Practices.Unity;
using Quantumart.QP8.BLL.Mappers;
using Quantumart.QP8.BLL.Repository;
using Quantumart.QP8.BLL.Services;
using Quantumart.QP8.Configuration;
using Quantumart.QP8.Constants;
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
        private const string CurrentUserIdKey = "CurrentUserId";
        private const string CurrentGroupIdsKey = "CurrentGroupIds";
        private const string CurrentCustomerCodeKey = "CurrentCustomerCode";
        private const string CurrentSqlVersionKey = "CurrentSqlVersion";
        private const string IsAdminKey = "IsAdmin";
        private const string CanUnlockItemsKey = "CanUnlockItems";
        private const string IsLiveKey = "IsLive";

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

        private static T GetValueFromStorage<T>(T threadStorage, string key)
        {
            if (_externalContextStorage != null && _externalContextStorageKeys != null && _externalContextStorageKeys.Contains(key))
            {
                return _externalContextStorage.GetValue<T>(key);
            }

            if (HttpContext.Current == null)
            {
                return threadStorage;
            }

            return (T)HttpContext.Current.Items[key];
        }

        private static void SetValueToStorage<T>(ref T threadStorage, T value, string key)
        {
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
            return GetValueFromStorage(_fieldCache, "_FieldCache");
        }

        internal static void SetFieldCache(Dictionary<int, Field> value)
        {
            SetValueToStorage(ref _fieldCache, value, "_FieldCache");
        }

        internal static Dictionary<int, StatusType> GetStatusTypeCache()
        {
            return GetValueFromStorage(_statusTypeCache, "_StatusType");
        }

        internal static Dictionary<int, User> GetUserCache()
        {
            return GetValueFromStorage(_userCache, "_User");
        }

        internal static void SetStatusTypeCache(Dictionary<int, StatusType> value)
        {
            SetValueToStorage(ref _statusTypeCache, value, "_StatusType");
        }

        internal static void SetUserCache(Dictionary<int, User> value)
        {
            SetValueToStorage(ref _userCache, value, "_User");
        }

        internal static Dictionary<int, Content> GetContentCache()
        {
            return GetValueFromStorage(_contentCache, "_ContentCache");
        }

        internal static void SetContentCache(Dictionary<int, Content> value)
        {
            SetValueToStorage(ref _contentCache, value, "_ContentCache");
        }

        internal static Dictionary<int, Site> GetSiteCache()
        {
            return GetValueFromStorage(_siteCache, "_SiteCache");
        }

        internal static void SetSiteCache(Dictionary<int, Site> value)
        {
            SetValueToStorage(ref _siteCache, value, "_SiteCache");
        }

        internal static Dictionary<int, List<int>> GetContentFieldCache()
        {
            return GetValueFromStorage(_contentFieldCache, "_ContentFieldCache");
        }

        internal static void SetContentFieldCache(Dictionary<int, List<int>> value)
        {
            SetValueToStorage(ref _contentFieldCache, value, "_ContentFieldCache");
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

        private static void SetCurrentUserIdValueToStorage(int? value)
        {
            SetValueToStorage(ref _currentUserId, value, CurrentUserIdKey);
        }

        private static void SetCurrentGroupIdsValueToStorage(int[] value)
        {
            SetValueToStorage(ref _currentGroupIds, value, CurrentGroupIdsKey);
        }

        private static void SetIsAdminValueToStorage(bool? value)
        {
            SetValueToStorage(ref _isAdmin, value, IsAdminKey);
        }

        private static void SetCanUnlockItemsValueToStorage(bool? value)
        {
            SetValueToStorage(ref _canUnlockItems, value, CanUnlockItemsKey);
        }

        private static void SetCurrentCustomerCodeValueToStorage(string value)
        {
            SetValueToStorage(ref _currentCustomerCode, value, CurrentCustomerCodeKey);
        }

        private static void SetCurrentSqlVersionValueToStorage(Version value)
        {
            SetValueToStorage(ref _currentSqlVersion, value, CurrentSqlVersionKey);
        }

        public static int CurrentUserId
        {
            get
            {
                var result = GetValueFromStorage(_currentUserId, CurrentUserIdKey);
                if (result == null)
                {
                    result = (HttpContext.Current.User.Identity as QPIdentity)?.Id;
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
                var result = GetValueFromStorage(_isAdmin, IsAdminKey);
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
                var result = GetValueFromStorage(_currentGroupIds, CurrentGroupIdsKey);
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
                var result = GetValueFromStorage(_canUnlockItems, CanUnlockItemsKey);
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
                var value = GetValueFromStorage(_isLive, IsLiveKey);
                return value.HasValue && value.Value;
            }
            set
            {
                SetValueToStorage(ref _isLive, value, IsLiveKey);
            }
        }

        public static string CurrentUserName => (HttpContext.Current.User.Identity as QPIdentity)?.Name;

        public static string CurrentCustomerCode
        {
            get
            {
                var result = GetValueFromStorage(_currentCustomerCode, CurrentCustomerCodeKey);
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

        public static Version CurrentSqlVersion
        {
            get
            {
                var result = GetValueFromStorage(_currentSqlVersion, CurrentSqlVersionKey);
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

        public static QPIdentity CurrentUserIdentity => HttpContext.Current != null && HttpContext.Current.User != null ? HttpContext.Current.User.Identity as QPIdentity : null;

        private static string _currentDbConnectionString;

        public static string CurrentDbConnectionString
        {
            get
            {
                return _currentDbConnectionString ?? QPConfiguration.ConfigConnectionString(CurrentCustomerCode);
            }
            set
            {
                _currentDbConnectionString = value;
            }
        }

        private static string CurrentDbConnectionStringForEntities => PreparingDbConnectionStringForEntities(CurrentDbConnectionString);

        private static string PreparingDbConnectionStringForEntities(string connectionString)
        {
            return $"metadata=res://*/QP8Model.csdl|res://*/QP8Model.ssdl|res://*/QP8Model.msl;provider=System.Data.SqlClient;provider connection string=\"{connectionString}\"";
        }

        [SuppressMessage("ReSharper", "PossibleNullReferenceException")]
        public static bool CheckCustomerCode(string customerCode)
        {
            return QPConfiguration.XmlConfig.Descendants("customer").Select(n => n.Attribute("customer_name").Value).Contains(customerCode);
        }

        /// <summary>
        /// Возвращает информацию о пользователя по его логину и паролю
        /// </summary>
        /// <param name="data">данные логина</param>
        /// <param name="errorCode">код ошибки</param>
        /// <param name="message">сообщение</param>
        /// <returns>информация о пользователе</returns>
        public static QPUser Authenticate(LogOnCredentials data, ref int errorCode, out string message)
        {
            QPUser resultUser = null;
            message = string.Empty;

            using (var dbContext = new QP8Entities(PreparingDbConnectionStringForEntities(QPConfiguration.ConfigConnectionString(data.CustomerCode))))
            {
                try
                {
                    var dbUser = dbContext.Authenticate(data.UserName, data.Password, data.UseAutoLogin, false);
                    var user = MappersRepository.UserMapper.GetBizObject(dbUser);

                    if (user != null)
                    {
                        resultUser = new QPUser
                        {
                            Id = user.Id,
                            Name = user.LogOn,
                            CustomerCode = data.CustomerCode,
                            LanguageId = user.LanguageId,
                            Roles = new string[0]
                        };

                        CreateSuccessfulSession(user, dbContext);

                        var context = HttpContext.Current;
                        if (context != null)
                        {
                            context.Items[ApplicationConfigurationKeys.DBContext] = dbContext;
                            QP7Service.SetPassword(data.Password);
                        }
                    }
                    else
                    {
                        CreateFaildSession(data, dbContext);
                    }
                }
                catch (SqlException ex)
                {
                    message = ex.Message;
                    errorCode = ex.State;
                    CreateFaildSession(data, dbContext);
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

        /// <summary>
        /// Создать сессию при успешном логине
        /// </summary>
        /// <param name="user"></param>
        /// <param name="dbContext"></param>
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

                Browser = HttpContext.Current.Request.ServerVariables["HTTP_USER_AGENT"].Left(255),
                IP = HttpContext.Current.Request.UserHostAddress,
                ServerName = Environment.MachineName.Left(255)
            };

            var sessionsLogDal = MappersRepository.SessionsLogMapper.GetDalObject(sessionsLog);
            dbContext.AddToSessionsLogSet(sessionsLogDal);
            dbContext.SaveChanges();
        }

        /// <summary>
        /// закрыть открытые сессии пользователя
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="dbContext"></param>
        /// <param name="currentDt"></param>
        private static void CloseUserSessions(decimal userId, QP8Entities dbContext, DateTime currentDt)
        {
            var userSessions =
                         dbContext.SessionsLogSet
                         .Where(s => s.UserId == userId &&
                                     !s.EndTime.HasValue &&
                                     !s.IsQP7)
                         .ToArray();
            foreach (var us in userSessions)
            {
                us.EndTime = currentDt;
                us.Sid = null;
            }
        }

        /// <summary>
        /// Создать сессию при неудачном логине
        /// </summary>
        /// <param name="data"></param>
        /// <param name="dbContext"></param>
        private static void CreateFaildSession(LogOnCredentials data, QP8Entities dbContext)
        {
            var sessionsLog = new SessionsLog
            {
                AutoLogged = data.UseAutoLogin ? 1 : 0,
                Login = data.UserName.Left(20),
                UserId = null,
                StartTime = DateTime.Now,

                Browser = HttpContext.Current.Request.ServerVariables["HTTP_USER_AGENT"].Left(255),
                IP = HttpContext.Current.Request.UserHostAddress,
                ServerName = Environment.MachineName.Left(255)
            };

            var sessionsLogDal = MappersRepository.SessionsLogMapper.GetDalObject(sessionsLog);
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
