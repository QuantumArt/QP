using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.Text;
using System.Security;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web;
using Quantumart.QP8;
using Quantumart.QP8.Security;
using Quantumart.QP8.Configuration;
using Quantumart.QP8.DAL;
using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Mappers;
using Quantumart.QP8.Utils;
using Quantumart.QPublishing.Database;
using System.Web.Configuration;
using System.Data.EntityClient;
using System.Data.Metadata.Edm;
using Quantumart.QP8.BLL.Repository;
using System.Transactions;
using Microsoft.Practices.Unity;
using Quantumart.QP8.BLL.Services;

namespace Quantumart.QP8.BLL
{
    public interface IContextStorage
	{
		T GetValue<T>(string key);
		void SetValue<T>(T value, string key);
		void ResetValue(string key);
		IEnumerable<string> Keys { get; }
	}
	
	public class QPContext
    {
		private static readonly string CurrentUserIdKey = "CurrentUserId";
		private static readonly string CurrentCustomerCodeKey = "CurrentCustomerCode";
		private static readonly string CurrentSqlVersionKey = "CurrentSqlVersion";
		private static readonly string IsAdminKey = "IsAdmin";
		private static readonly string CanUnlockItemsKey = "CanUnlockItems";
		private static readonly string IsLiveKey = "IsLive";
		
		/// <summary>
        /// текущий объект ObjectContext для работы с ADO.Net Entity Framework
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        public static QP8Entities EFContext
        {
			get
			{

				QP8Entities dbContext = null;
                if (QPConnectionScope.Current == null)
                {
                    dbContext = new QP8Entities(CurrentDBConnectionStringForEntities);
                    dbContext.ExecuteStoreCommand(QPConnectionScope.SetIsolationLevelCommandText);					
                }
                else
                {
                    dbContext = new QP8Entities(QPConnectionScope.Current.EFConnection);
                }
				dbContext.ContextOptions.LazyLoadingEnabled = false;
				return dbContext;				
			}            			
        }

		private static T GetValueFromStorage<T>(T threadStorage, string key)
		{
			if (_ExternalContextStorage != null && _ExternalContextStorageKeys != null && _ExternalContextStorageKeys.Contains(key))
				return _ExternalContextStorage.GetValue<T>(key);
			else if (HttpContext.Current == null)
				return threadStorage;
			else
				return (T)HttpContext.Current.Items[key];	
		}


		private static void SetValueToStorage<T>(ref T threadStorage, T value, string key)
		{
			if (_ExternalContextStorage != null && _ExternalContextStorageKeys != null && _ExternalContextStorageKeys.Contains(key))
				_ExternalContextStorage.SetValue<T>(value, key);
			if (HttpContext.Current == null)
				threadStorage = value;
			else
				HttpContext.Current.Items[key] = value;
		}

		#region Cache

			internal static Dictionary<int, Field> GetFieldCache()
			{
				return GetValueFromStorage<Dictionary<int, Field>>(_FieldCache, "_FieldCache");
			}

			internal static void SetFieldCache(Dictionary<int, Field> value)
			{
				SetValueToStorage(ref _FieldCache, value, "_FieldCache");
			}

			internal static Dictionary<int, StatusType> GetStatusTypeCache()
			{
				return GetValueFromStorage<Dictionary<int, StatusType>>(_StatusTypeCache, "_StatusType");
			}

			internal static Dictionary<int, User> GetUserCache()
			{
				return GetValueFromStorage<Dictionary<int, User>>(_UserCache, "_User");
			}

			internal static void SetStatusTypeCache(Dictionary<int, StatusType> value)
			{
				SetValueToStorage(ref _StatusTypeCache, value, "_StatusType");
			}

			internal static void SetUserCache(Dictionary<int, User> value)
			{
				SetValueToStorage(ref _UserCache, value, "_User");
			}

			internal static Dictionary<int, Content> GetContentCache()
			{
				return GetValueFromStorage<Dictionary<int, Content>>(_ContentCache, "_ContentCache");
			}

			internal static void SetContentCache(Dictionary<int, Content> value)
			{
				SetValueToStorage(ref _ContentCache, value, "_ContentCache");
			}

			internal static Dictionary<int, Site> GetSiteCache()
			{
				return GetValueFromStorage<Dictionary<int, Site>>(_SiteCache, "_SiteCache");
			}

			internal static void SetSiteCache(Dictionary<int, Site> value)
			{
				SetValueToStorage(ref _SiteCache, value, "_SiteCache");
			}

			internal static Dictionary<int, List<int>> GetContentFieldCache()
			{
				return GetValueFromStorage<Dictionary<int, List<int>>>(_ContentFieldCache, "_ContentFieldCache");
			}

			internal static void SetContentFieldCache(Dictionary<int, List<int>> value)
			{
				SetValueToStorage(ref _ContentFieldCache, value, "_ContentFieldCache");
			}

			internal static void ClearInternalStructureCache()
			{
				_SiteCache = null;
				_ContentCache = null;
				_ContentFieldCache = null;
				_FieldCache = null;
				_StatusTypeCache = null;
				_UserCache = null;
			}
			
			internal static void ClearExternalStructureCache()
			{
				if (_ExternalContextStorage != null && _ExternalContextStorage.Keys != null)
				{
					foreach (var key in _ExternalContextStorage.Keys)
					{
						_ExternalContextStorage.ResetValue(key);
					}
				}
			}

			internal static void LoadStructureCache(bool clearExternal)
			{
				ClearInternalStructureCache();
				if (clearExternal)
					ClearExternalStructureCache();

				if (GetSiteCache() == null)
					SetSiteCache(SiteRepository.GetAll().ToDictionary(n => n.Id));

				if (GetContentCache() == null)
					SetContentCache(ContentRepository.GetAll().ToDictionary(n => n.Id));

				IEnumerable<Field> fields = null;
				if (GetFieldCache() == null || GetContentFieldCache() == null)
					fields = FieldRepository.GetAll();

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
					Dictionary<int, List<int>> dict = new Dictionary<int, List<int>>();
					foreach (var item in fields)
					{
						if (dict.ContainsKey(item.ContentId))
							dict[item.ContentId].Add(item.Id);
						else
							dict.Add(item.ContentId, new List<int> { item.Id });
					}
					SetContentFieldCache(dict);
				}

			}

		#endregion

		[ThreadStatic]
		private static int? _CurrentUserId = null;

		[ThreadStatic]
		private static bool? _IsAdmin = null;

		[ThreadStatic]
		private static bool? _CanUnlockItems = null;

		[ThreadStatic]
		private static bool? _IsLive = null;

		[ThreadStatic]
		private static Dictionary<int, Site> _SiteCache;

		[ThreadStatic]
		private static Dictionary<int, Content> _ContentCache;

		[ThreadStatic]
		private static Dictionary<int, Field> _FieldCache;

		[ThreadStatic]
		private static Dictionary<int, StatusType> _StatusTypeCache;

		[ThreadStatic]
		private static Dictionary<int, User> _UserCache;

		[ThreadStatic]
		private static Dictionary<int, List<int>> _ContentFieldCache;

		[ThreadStatic]
		private static string _CurrentCustomerCode;

		[ThreadStatic]
		private static Version _CurrentSqlVersion;


		private static void SetCurrentUserIdValueToStorage(int? value)
		{
			SetValueToStorage<int?>(ref _CurrentUserId, value, CurrentUserIdKey);
		}

		private static void SetIsAdminValueToStorage(bool? value)
		{
			SetValueToStorage<bool?>(ref _IsAdmin, value, IsAdminKey);
		}
			
		private static void SetCanUnlockItemsValueToStorage(bool? value)
		{
			SetValueToStorage<bool?>(ref _CanUnlockItems, value, CanUnlockItemsKey);
		}

		private static void SetIsLiveValueToStorage(bool value)
		{
			SetValueToStorage<bool?>(ref _IsLive, value, IsLiveKey);
		}

		private static void SetCurrentCustomerCodeValueToStorage(string value)
		{
			SetValueToStorage<string>(ref _CurrentCustomerCode, value, CurrentCustomerCodeKey);
		}

		private static void SetCurrentSqlVersionValueToStorage(Version value)
		{
			SetValueToStorage<Version>(ref _CurrentSqlVersion, value, CurrentSqlVersionKey);
		}

		public static int CurrentUserId
		{
			get
			{
				int? result = GetValueFromStorage<int?>(_CurrentUserId, CurrentUserIdKey);
				if (result == null)
				{
					result = (HttpContext.Current.User.Identity as QPIdentity).Id;
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
				}
				else
				{
					SetCurrentUserIdValueToStorage(value);
					using (new QPConnectionScope())
					{
						SetIsAdminValueToStorage(Common.IsAdmin(QPConnectionScope.Current.DbConnection, value));
					}
				}
			}
		}
		

		public static bool IsAdmin
		{
			get
			{
				bool? result = GetValueFromStorage<bool?>(_IsAdmin, IsAdminKey);
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

		public static bool CanUnlockItems
		{
			get
			{
				bool? result = GetValueFromStorage<bool?>(_CanUnlockItems, CanUnlockItemsKey);
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
				var value = GetValueFromStorage<bool?>(_IsLive, IsLiveKey);
				return (value.HasValue) ? value.Value : false ;
			}
			set
			{
				SetValueToStorage<bool?>(ref _IsLive, value, IsLiveKey);
			}
		}

		/// <summary>
		/// имя текущего пользователя
		/// </summary>
		public static string CurrentUserName
		{
			get
			{
				return (HttpContext.Current.User.Identity as QPIdentity).Name;
			}
		}

        /// <summary>
        /// текущий код клиента
        /// </summary>
        public static string CurrentCustomerCode
        {
            
            get
            {
				string result = GetValueFromStorage<string>(_CurrentCustomerCode, CurrentCustomerCodeKey);
				if (result == null)
				{
					if (HttpContext.Current == null || CurrentUserIdentity == null)
						result = null;
				else
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
				Version result = GetValueFromStorage<Version>(_CurrentSqlVersion, CurrentSqlVersionKey);
				if (result == null)
				{
					if (HttpContext.Current == null || CurrentUserIdentity == null)
						result = null;
					else
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

		public static QPIdentity CurrentUserIdentity 
		{ 
			get 
			{
				return (HttpContext.Current != null && HttpContext.Current.User != null) ? HttpContext.Current.User.Identity as QPIdentity : null;
			} 
		}

        /// <summary>
        /// текущая строка подключения к БД
        /// </summary>
        public static string CurrentDBConnectionString
        {
            get
            {
                return QPConfiguration.ConfigConnectionString(QPContext.CurrentCustomerCode);
            }
        }

        /// <summary>
        /// текущая строка подключения к БД для ADO.Net Entity Framework
        /// </summary>
        private static string CurrentDBConnectionStringForEntities
        {
            get
            {
                return QPContext.PreparingDBConnectionStringForEntities(CurrentDBConnectionString);
            }
        }

        /// <summary>
        /// Подготавливает строку подключения к БД для использования в Entity Framework
        /// </summary>
        /// <param name="connectionString">строка подключения к БД</param>
        /// <returns>строка подключения к БД для Entity Framework</returns>
        private static string PreparingDBConnectionStringForEntities(string connectionString)
        {
            return string.Format("metadata=res://*/QP8Model.csdl|res://*/QP8Model.ssdl|res://*/QP8Model.msl;provider=System.Data.SqlClient;provider connection string=\"{0}\"", connectionString);
        }

        /// <summary>
        /// Проверяет существование кода клиента
        /// </summary>
        /// <param name="customerCode">код клиента</param>
        /// <returns>результат проверки (true - существует; false - не существует)</returns>
        public static bool CheckCustomerCode(string customerCode)
        {
            return QPConfiguration.XmlConfig.Descendants("customer").Select(n => n.Attribute("customer_name").Value).Contains(customerCode);
        }

		#region Log In/Out
		/// <summary>
		/// Возвращает информацию о пользователя по его логину и паролю
		/// </summary>
		/// <param name="login">логин</param>
		/// <param name="password">пароль</param>
		/// <param name="customerCode">код клиента</param>
		/// <param name="useNtLogin">разрешает Windows-аутентификацию</param>
		/// <param name="errorCode">код ошибки</param>
		/// <returns>информация о пользователе</returns>
		public static QPUser Authenticate(LogOnCredentials data, ref int errorCode, out string message)
		{
			User user = null;
			QPUser resultUser = null;
			message = String.Empty;

			using (QP8Entities dbContext = new QP8Entities(QPContext.PreparingDBConnectionStringForEntities(QPConfiguration.ConfigConnectionString(data.CustomerCode))))
			{
				try
				{
					UserDAL dbUser = dbContext.Authenticate(data.UserName, data.Password, data.UseAutoLogin, false);
					user = MappersRepository.UserMapper.GetBizObject(dbUser);

					if (user != null)
					{
						resultUser = new QPUser
						{
							Id = (int)user.Id,
							Name = user.LogOn,
							CustomerCode = data.CustomerCode,
							LanguageId = user.LanguageId,
							Roles = new string[0]
						};

						CreateSuccessfulSession(user, dbContext);

						HttpContext context = HttpContext.Current;
						if (context != null && context.Items != null)
						{
							context.Items[Constants.ApplicationConfigurationKeys.DBContext] = dbContext;
                            QP7Service.SetPassword(data.Password);
                        }
					}
					else
						CreateFaildSession(data, dbContext);
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
			using (var transaction = new TransactionScope(TransactionScopeOption.Suppress, new TransactionOptions {IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted}))
			{
				using (QPConnectionScope scope = new QPConnectionScope())
				{
					var dbContext = EFContext;
					CloseUserSessions(CurrentUserId, dbContext, DateTime.Now);
					dbContext.SaveChanges();
				}

				string loginUrl = AuthenticationHelper.LogOut();

				transaction.Complete();

                HttpContext.Current.Session.Abandon();

				
				return loginUrl;
			}
		} 
		#endregion

		#region User Session Log
		/// <summary>
		/// Создать сессию при успешном логине
		/// </summary>
		/// <param name="user"></param>
		/// <param name="dbContext"></param>
		private static void CreateSuccessfulSession(User user, QP8Entities dbContext)
		{
			// сбросить sid и установить EndTime для всех сессий пользователя
			DateTime currentDT = DateTime.Now;

			// закрыть открытые сессии пользователя
			CloseUserSessions(user.Id, dbContext, currentDT);

			// Сохранить новую сессию
			SessionsLog sessionsLog = new SessionsLog
			{
				AutoLogged = user.AutoLogOn ? 1 : 0,
				Login = user.Name.Left(20),
				UserId = user.Id,
				StartTime = currentDT,

				Browser = HttpContext.Current.Request.ServerVariables["HTTP_USER_AGENT"].Left(255),
				IP = HttpContext.Current.Request.UserHostAddress,
				ServerName = Environment.MachineName.Left(255)
			};
			SessionsLogDAL sessionsLogDAL = MappersRepository.SessionsLogMapper.GetDalObject(sessionsLog);
			dbContext.AddToSessionsLogSet(sessionsLogDAL);
			dbContext.SaveChanges();

		}

		/// <summary>
		/// закрыть открытые сессии пользователя
		/// </summary>
		/// <param name="userId"></param>
		/// <param name="dbContext"></param>
		/// <param name="currentDT"></param>
		private static void CloseUserSessions(decimal userId, QP8Entities dbContext, DateTime currentDT)
		{
			var userSessions =
						 dbContext.SessionsLogSet
						 .Where(s => s.UserId == userId && 
									 !s.EndTime.HasValue &&
									 !s.IsQP7)
						 .ToArray();
			foreach (var us in userSessions)
			{
				us.EndTime = currentDT;
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
			SessionsLog sessionsLog = new SessionsLog
			{
				AutoLogged = data.UseAutoLogin ? 1 : 0,
				Login = data.UserName.Left(20),
				UserId = null,
				StartTime = DateTime.Now,

				Browser = HttpContext.Current.Request.ServerVariables["HTTP_USER_AGENT"].Left(255),
				IP = HttpContext.Current.Request.UserHostAddress,
				ServerName = Environment.MachineName.Left(255)
			};
			SessionsLogDAL sessionsLogDAL = MappersRepository.SessionsLogMapper.GetDalObject(sessionsLog);
			dbContext.AddToSessionsLogSet(sessionsLogDAL);
			dbContext.SaveChanges();
		} 
		#endregion

		public static IUnityContainer CurrentUnityContainer { get;  private set; } 
		public static void SetUnityContainer(IUnityContainer container)
		{
			CurrentUnityContainer = container;
		}

		private static IContextStorage _ExternalContextStorage;
		private static HashSet<string> _ExternalContextStorageKeys;
		public static IContextStorage ExternalContextStorage { 
			set
			{
				if (value.Keys == null || !value.Keys.Any())
					throw new ArgumentException("Keys collection is empty");

				_ExternalContextStorage = value;
				_ExternalContextStorageKeys = new HashSet<string>(value.Keys);
			}
		}
	
	}
}
