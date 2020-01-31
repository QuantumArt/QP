using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Transactions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Session;
using Microsoft.EntityFrameworkCore;
using Microsoft.Net.Http.Headers;
using NLog;
using NLog.Fluent;
using Npgsql;
using Quantumart.QP8.BLL.Facades;
using Quantumart.QP8.BLL.Repository;
using Quantumart.QP8.BLL.Repository.ContentRepositories;
using Quantumart.QP8.BLL.Repository.FieldRepositories;
using Quantumart.QP8.BLL.Repository.Helpers;
using Quantumart.QP8.BLL.Services;
using Quantumart.QP8.Configuration;
using Quantumart.QP8.Constants;
using Quantumart.QP8.Constants.Mvc;
using Quantumart.QP8.DAL;
using Quantumart.QP8.Security;
using Quantumart.QP8.Utils;

namespace Quantumart.QP8.BLL
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public class QPContext
    {
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();
        private static HttpContext HttpContext => new HttpContextAccessor().HttpContext;

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        public static QPModelDataContext EFContext
        {
            get
            {
                QPModelDataContext dbContext;
                if (QPConnectionScope.Current == null)
                {
                    dbContext = CreateDbContext(CurrentDbConnectionInfo);
                }
                else
                {
                    dbContext = CreateDbContextUsingConnection();
                }
                return dbContext;
            }
        }

        public static DatabaseType DatabaseType => QPConnectionScope.Current?.DbType ?? CurrentDbConnectionInfo?.DbType ?? DatabaseType.SqlServer;

        private static QPModelDataContext CreateDbContextUsingConnection()
        {
            var dbType = QPConnectionScope.Current.DbType;
            switch (dbType)
            {
                case DatabaseType.Unknown:
                    throw new ApplicationException("Database type unknown");
                case DatabaseType.SqlServer:
                    return new SqlServerQPModelDataContext(QPConnectionScope.Current.EfConnection);
                case DatabaseType.Postgres:
                    return new NpgSqlQPModelDataContext(QPConnectionScope.Current.EfConnection);
                default:
                    throw new ArgumentOutOfRangeException(nameof(dbType), dbType, null);
            }

        }

        private static QPModelDataContext CreateDbContext(QpConnectionInfo cnnInfo)
        {
            QPModelDataContext dbContext;
            switch (cnnInfo.DbType)
            {
                case DatabaseType.Unknown:
                    throw new ApplicationException("Database type unknown");
                case DatabaseType.SqlServer:
                    dbContext = new SqlServerQPModelDataContext(cnnInfo.ConnectionString);
                    dbContext.Database.ExecuteSqlCommand($"SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED");
                    break;
                case DatabaseType.Postgres:
                    dbContext = new NpgSqlQPModelDataContext(cnnInfo.ConnectionString);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(cnnInfo.DbType), cnnInfo.DbType, null);
            }

            return dbContext;
        }

        public static string GetRecordXmlFilePath() => $"{QPConfiguration.TempDirectory}{CurrentCustomerCode}.xml";

        private static T GetValueFromStorage<T>(AsyncLocal<T> threadStorageData, string key)
        {
            if (UseThreadStorage)
            {
                return threadStorageData.Value;
            }

            if (_externalContextStorage != null && _externalContextStorageKeys != null && _externalContextStorageKeys.Contains(key))
            {
                return _externalContextStorage.GetValue<T>(key);
            }

            if (HttpContext == null)
            {
                return threadStorageData.Value;
            }

            return (T)HttpContext.Items[key];
        }

        private static void SetValueToStorage<T>( AsyncLocal<T> threadStorage, T value, string key)
        {
            if (UseThreadStorage)
            {
                threadStorage.Value = value;
            }

            if (_externalContextStorage != null && _externalContextStorageKeys != null && _externalContextStorageKeys.Contains(key))
            {
                _externalContextStorage.SetValue(value, key);
            }

            if (HttpContext == null)
            {
                threadStorage.Value = value;
            }
            else
            {
                HttpContext.Items[key] = value;
            }
        }

        internal static Dictionary<int, Field> GetFieldCache() => GetValueFromStorage(_fieldCache, HttpContextItems.FieldCache);

        internal static Dictionary<int, StatusType> GetStatusTypeCache() => GetValueFromStorage(_statusTypeCache, HttpContextItems.StatusType);

        internal static Dictionary<int, User> GetUserCache() => GetValueFromStorage(_userCache, HttpContextItems.User);

        internal static Dictionary<int, Content> GetContentCache() => GetValueFromStorage(_contentCache, HttpContextItems.ContentCache);

        internal static Dictionary<int, Site> GetSiteCache() => GetValueFromStorage(_siteCache, HttpContextItems.SiteCache);

        internal static Dictionary<int, List<int>> GetContentFieldCache() => GetValueFromStorage(_contentFieldCache, HttpContextItems.ContentFieldCache);

        private static void ClearInternalStructureCache()
        {
            _siteCache.Value = null;
            _contentCache.Value = null;
            _contentFieldCache.Value = null;
            _fieldCache.Value = null;
            _statusTypeCache.Value = null;
            _userCache.Value = null;
        }

        private static void ClearExternalStructureCache()
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
                Dictionary<int, Site> value = SiteRepository.GetAll().ToDictionary(n => n.Id);
                SetValueToStorage(_siteCache, value, HttpContextItems.SiteCache);
            }

            if (GetContentCache() == null)
            {
                Dictionary<int, Content> value = ContentRepository.GetAll().ToDictionary(n => n.Id);
                SetValueToStorage(_contentCache, value, HttpContextItems.ContentCache);
            }

            IEnumerable<Field> fields = new Field[0];
            if (GetFieldCache() == null || GetContentFieldCache() == null)
            {
                fields = FieldRepository.GetAll();
            }

            if (GetFieldCache() == null)
            {
                Dictionary<int, Field> value = fields.ToDictionary(n => n.Id);
                SetValueToStorage(_fieldCache, value, HttpContextItems.FieldCache);
            }

            if (GetStatusTypeCache() == null)
            {
                Dictionary<int, StatusType> value = StatusTypeRepository.GetAll().ToDictionary(n => n.Id);
                SetValueToStorage(_statusTypeCache, value, HttpContextItems.StatusType);
            }

            if (GetUserCache() == null)
            {
                Dictionary<int, User> value = UserRepository.GetAllUsersList().ToDictionary(n => n.Id);
                SetValueToStorage(_userCache, value, HttpContextItems.User);
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

                SetValueToStorage(_contentFieldCache, dict, HttpContextItems.ContentFieldCache);
            }
        }

        private static readonly AsyncLocal<int?> _currentUserId = new AsyncLocal<int?>();

        private static readonly AsyncLocal<int?> _currentSessionId = new AsyncLocal<int?>();

        private static readonly AsyncLocal<bool?> _isAdmin = new AsyncLocal<bool?>();

        private static readonly AsyncLocal<int[]> _currentGroupIds = new AsyncLocal<int[]>();

        private static readonly AsyncLocal<bool?> _canUnlockItems = new AsyncLocal<bool?>();

        private static readonly AsyncLocal<bool?> _isLive = new AsyncLocal<bool?>();

        private static readonly AsyncLocal<Dictionary<int, Site>> _siteCache = new AsyncLocal<Dictionary<int, Site>>();

        private static readonly AsyncLocal<Dictionary<int, Content>> _contentCache = new AsyncLocal<Dictionary<int, Content>>();

        private static readonly AsyncLocal<Dictionary<int, Field>> _fieldCache = new AsyncLocal<Dictionary<int, Field>>();

        private static readonly AsyncLocal<Dictionary<int, StatusType>> _statusTypeCache = new AsyncLocal<Dictionary<int, StatusType>>();

        private static readonly AsyncLocal<Dictionary<int, User>> _userCache = new AsyncLocal<Dictionary<int, User>>();

        private static readonly AsyncLocal<Dictionary<int, List<int>>> _contentFieldCache = new AsyncLocal<Dictionary<int, List<int>>>();

        private static readonly AsyncLocal<string> _currentCustomerCode = new AsyncLocal<string>();

        private static readonly AsyncLocal<Version> _currentSqlVersion = new AsyncLocal<Version>();

        private static readonly AsyncLocal<QPConnectionScope> _currentConnectionScope = new AsyncLocal<QPConnectionScope>();

        private static readonly AsyncLocal<QpConnectionInfo> _currentDbConnectionInfo = new AsyncLocal<QpConnectionInfo>();

        private static readonly AsyncLocal<BackendActionContext> _backendActionContext = new AsyncLocal<BackendActionContext>();

        private static void SetCurrentUserIdValueToStorage(int? value)
        {
            SetValueToStorage(_currentUserId, value, HttpContextItems.CurrentUserIdKey);
        }

        private static void SetCurrentSessionIdValueToStorage(int? value)
        {
            SetValueToStorage(_currentSessionId, value, HttpContextItems.CurrentSessionIdKey);
        }

        private static void SetCurrentGroupIdsValueToStorage(int[] value)
        {
            SetValueToStorage(_currentGroupIds, value, HttpContextItems.CurrentGroupIdsKey);
        }

        private static void SetIsAdminValueToStorage(bool? value)
        {
            SetValueToStorage(_isAdmin, value, HttpContextItems.IsAdminKey);
        }

        private static void SetCanUnlockItemsValueToStorage(bool? value)
        {
            SetValueToStorage(_canUnlockItems, value, HttpContextItems.CanUnlockItemsKey);
        }

        private static void SetCurrentCustomerCodeValueToStorage(string value)
        {
            SetValueToStorage(_currentCustomerCode, value, HttpContextItems.CurrentCustomerCodeKey);
        }

        private static void SetCurrentSqlVersionValueToStorage(Version value)
        {
            SetValueToStorage(_currentSqlVersion, value, HttpContextItems.CurrentSqlVersionKey);
        }

        public static int CurrentUserId
        {
            get
            {
                var result = GetValueFromStorage(_currentUserId, HttpContextItems.CurrentUserIdKey);

                if (result == null)
                {
                    var claimValue = HttpContext?.User?.FindFirst("Id")?.Value;
                    result = claimValue == null ? 0 : int.Parse(claimValue);
                    SetCurrentUserIdValueToStorage(result);
                }

                return (int)result;
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

        public static int CurrentSessionId
        {
            get
            {
                var result = GetValueFromStorage(_currentSessionId, HttpContextItems.CurrentSessionIdKey);

                if (result == null)
                {
                    var claimValue = HttpContext?.User?.FindFirst(ClaimTypes.Sid).Value;
                    result = claimValue == null ? 0 : int.Parse(claimValue);
                    SetCurrentSessionIdValueToStorage(result);
                }

                return (int)result;
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
            set => SetIsAdminValueToStorage(value);
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
            set => SetCurrentGroupIdsValueToStorage(value);
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
            set => SetCanUnlockItemsValueToStorage(value);
        }

        public static bool IsLive
        {
            get
            {
                var value = GetValueFromStorage(_isLive, HttpContextItems.IsLiveKey);
                return value.HasValue && value.Value;
            }
            set => SetValueToStorage(_isLive, value, HttpContextItems.IsLiveKey);
        }

        public static string CurrentUserName => HttpContext?.User?.FindFirst(ClaimTypes.Name)?.Value ?? "";

        public static string CurrentCultureName
        {
            get
            {
                var claim = HttpContext?.User?.FindFirst("CultureName");
                return claim != null ? claim.Value : QPConfiguration.Options.Globalization.DefaultCulture;
            }
        }

        public static int CurrentLanguageId
        {
            get
            {
                var claim = HttpContext?.User?.FindFirst("LanguageId");
                return claim != null ? int.Parse(claim.Value) : QPConfiguration.Options.Globalization.DefaultLanguageId;
            }
        }

        public static string CurrentCustomerCode
        {
            get
            {
                var result = GetValueFromStorage(_currentCustomerCode, HttpContextItems.CurrentCustomerCodeKey);

                if (result == null)
                {
                    var claim = HttpContext?.User?.FindFirst("CustomerCode");
                    if (claim != null)
                    {
                        result = claim.Value;
                        SetCurrentCustomerCodeValueToStorage(result);
                    }
                }

                return result;
            }
            set => SetCurrentCustomerCodeValueToStorage(value);
        }

        internal static QPConnectionScope CurrentConnectionScope
        {
            get => GetValueFromStorage(_currentConnectionScope, HttpContextItems.CurrentConnectionScopeKey);
            set => SetValueToStorage(_currentConnectionScope, value, HttpContextItems.CurrentConnectionScopeKey);
        }

        internal static BackendActionContext BackendActionContext
        {
            get => GetValueFromStorage(_backendActionContext, HttpContextItems.BackendActionContextKey);
            set => SetValueToStorage(_backendActionContext, value, HttpContextItems.BackendActionContextKey);
        }

        public static Version CurrentSqlVersion
        {
            get
            {
                var result = GetValueFromStorage(_currentSqlVersion, HttpContextItems.CurrentSqlVersionKey);

                if (result == null)
                {
                    result = Common.GetSqlServerVersion(QPConnectionScope.Current.DbConnection);
                    SetCurrentSqlVersionValueToStorage(result);
                }

                return result;
            }
            set => SetCurrentSqlVersionValueToStorage(value);
        }

        public static string CurrentDbConnectionString
        {
            get => CurrentDbConnectionInfo?.ConnectionString;
            set => CurrentDbConnectionInfo = new QpConnectionInfo(value, DatabaseType.SqlServer);
        }

        public static QpConnectionInfo CurrentDbConnectionInfo
        {
            get
            {
                var result = GetValueFromStorage(_currentDbConnectionInfo, HttpContextItems.CurrentDbConnectionStringKey);
                if (result == null)
                {
                    var info = QPConfiguration.GetConnectionInfo(CurrentCustomerCode);
                    if (info != null)
                    {
                        result = new QpConnectionInfo(info.ConnectionString, info.DbType);
                        SetValueToStorage(_currentDbConnectionInfo, result, HttpContextItems.CurrentDbConnectionStringKey);
                    }
                }

                return result;
            }
            set => SetValueToStorage(_currentDbConnectionInfo, value, HttpContextItems.CurrentDbConnectionStringKey);
        }

        public static bool CheckCustomerCode(string customerCode)
        {
            return QPConfiguration.GetCustomerCodes().Contains(customerCode);
        }

        public static QpUser Authenticate(LogOnCredentials data, ref int errorCode, out string message)
        {
            QpUser resultUser = null;
            message = string.Empty;

            var sqlCn = QPConfiguration.GetConnectionInfo(data.CustomerCode);

            using (var dbContext = CreateDbContext(sqlCn))
            {
                try
                {
                    User user;
                    using (var cn = CreateDbConnection(sqlCn))
                    {
                        cn.Open();
                        var dbUser = Common.Authenticate(cn, data.UserName, data.Password, data.UseAutoLogin, false);
                        user = MapperFacade.UserMapper.GetBizObject(dbUser);
                    }

                    if (user != null)
                    {
                        resultUser = new QpUser
                        {
                            Id = user.Id,
                            Name = user.LogOn,
                            CustomerCode = data.CustomerCode,
                            LanguageId = user.LanguageId,
                            MustChangePassword = user.MustChangePassword
                        };

                        using (var cn = CreateDbConnection(sqlCn))
                        {
                            cn.Open();
                            resultUser.Roles = QpRolesManager.GetRolesForUser(Common.IsAdmin(cn, user.Id));
                        }

                        resultUser.SessionId = CreateSuccessfulSession(user, dbContext);

                        if (HttpContext != null)
                        {
                            HttpContext.Items[HttpContextItems.DbContext] = dbContext;
                            QP7Service.SetPassword(data.Password);
                        }
                    }
                    else
                    {
                        CreateFailedSession(data, dbContext);
                    }
                }
                catch (DbException ex)
                {
                    message = ex.Message;
                    switch (ex)
                    {
                        case SqlException sqlEx:
                            errorCode = sqlEx.State;
                            break;
                        case PostgresException npgsqlEx:
                            var code = npgsqlEx.SqlState;
                            if (code.StartsWith("AUTH", StringComparison.InvariantCultureIgnoreCase))
                            {
                                var codeNumberStr = code.Substring(4, 1);
                                if (!int.TryParse(codeNumberStr, out errorCode))
                                {
                                    errorCode = QpAuthenticationErrorNumber.UnknownError;
                                }
                            }
                            else
                            {
                                errorCode = QpAuthenticationErrorNumber.UnknownError;
                            }
                            break;
                        default:
                            errorCode = ex.ErrorCode;
                            break;
                    }

                    CreateFailedSession(data, dbContext);
                }

            }

            return resultUser;
        }

        public static DbConnection CreateDbConnection() => CreateDbConnection(CurrentDbConnectionInfo);

        private static DbConnection CreateDbConnection(QpConnectionInfo cnnInfo)
        {
            switch (cnnInfo.DbType)
            {
                case DatabaseType.Unknown:
                    throw new ApplicationException("Database type unknown");
                case DatabaseType.SqlServer:
                    return new SqlConnection(cnnInfo.ConnectionString);
                case DatabaseType.Postgres:
                    return new NpgsqlConnection(cnnInfo.ConnectionString);
                default:
                    throw new ArgumentOutOfRangeException(nameof(cnnInfo.DbType), cnnInfo.DbType, null);
            }
        }

        public static string LogOut()
        {
            using (var transaction = new TransactionScope(TransactionScopeOption.Suppress, new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted }))
            {
                using (var scope = new QPConnectionScope())
                {
                    var dbContext = EFContext;
                    CloseUserSessions(CurrentUserId, dbContext, DateTime.Now);
                    dbContext.SaveChanges();

                    CommonSecurity.ClearUserToken(scope.DbConnection, CurrentUserId, CurrentSessionId);
                }

                new AuthenticationHelper(new HttpContextAccessor() {HttpContext = HttpContext}, QPConfiguration.Options).SignOut();
                transaction.Complete();

                // AspNetCore equivalent of Session.Abandon()
                HttpContext.Session.Clear();
                if (HttpContext.Request.Cookies.ContainsKey(SessionDefaults.CookieName))
                {
                    HttpContext.Response.Cookies.Delete(SessionDefaults.CookieName);
                }

                BackendActionCache.ResetForUser();

                return QPConfiguration.Options.BackendUrl;
            }
        }

        private static int CreateSuccessfulSession(User user, QPModelDataContext dbContext)
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
                Browser = HttpContext.Request.Headers[HeaderNames.UserAgent].ToString().Left(255),
                IP = GetUserIpAddress(),
                ServerName = Environment.MachineName.Left(255)
            };

            var sessionsLogDal = MapperFacade.SessionsLogMapper.GetDalObject(sessionsLog);
            dbContext.Entry(sessionsLogDal).State = EntityState.Added;
            dbContext.SaveChanges();
            sessionsLog = MapperFacade.SessionsLogMapper.GetBizObject(sessionsLogDal);

            Logger.Trace()
                .Message("User successfully authenticated")
                .Property("sessionsLog", sessionsLog)
                .Property("customerCode", QPContext.CurrentCustomerCode)
                .Write();

            return sessionsLog.SessionId;
        }

        private static void CloseUserSessions(decimal userId, QPModelDataContext dbContext, DateTime currentDt)
        {
            var userSessions = dbContext.SessionsLogSet.Where(s => s.UserId == userId && !s.EndTime.HasValue && !s.IsQP7).ToArray();
            foreach (var us in userSessions)
            {
                us.EndTime = currentDt;
                us.Sid = null;
            }
        }

        private static void CreateFailedSession(LogOnCredentials data, QPModelDataContext dbContext)
        {
            var sessionsLog = new SessionsLog
            {
                AutoLogged = data.UseAutoLogin ? 1 : 0,
                Login = data.UserName.Left(20),
                UserId = null,
                StartTime = DateTime.Now,
                Browser = HttpContext.Request.Headers[HeaderNames.UserAgent].ToString().Left(255),
                IP = GetUserIpAddress(),
                ServerName = Environment.MachineName.Left(255)
            };

            var sessionsLogDal = MapperFacade.SessionsLogMapper.GetDalObject(sessionsLog);
            dbContext.Entry(sessionsLogDal).State = EntityState.Added;
            dbContext.SaveChanges();
        }

        private static string GetUserIpAddress()
        {
            string ipAddress = HttpContext.Request.Headers["X-Forwarded-For"];

            if (string.IsNullOrEmpty(ipAddress))
            {
                ipAddress = HttpContext.Connection.RemoteIpAddress.ToString();
            }

            return ipAddress;
        }

        public static void SetServiceProvider(IServiceProvider provider)
        {
        }

        private static IContextStorage _externalContextStorage;

        private static HashSet<string> _externalContextStorageKeys;

        public static bool UseThreadStorage { get; set; }

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
