using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Common;
using System.Data.SqlClient;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.InteropServices;
using System.Xml.Linq;
using Npgsql;
using QP.ConfigurationService.Client;
using QP8.Infrastructure.Helpers;
using QP8.Infrastructure.Logging.Extensions;
using Quantumart.QP8.Configuration.Models;
using Quantumart.QP8.Constants;
using Quantumart.QP8.Utils;
using Quantumart.QPublishing.Database;

namespace Quantumart.QP8.Configuration
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public class QPConfiguration
    {
        internal static string _configPath;

        private static string _tempDirectory;

        internal static string _configServiceUrl;

        internal static string _configServiceToken;

        public static QPublishingOptions Options { get; set; }

        /// <summary>
        /// Проверка, запущен ли пул в 64-битном режиме
        /// </summary>
        public static bool Is64BitMode => IntPtr.Size == 8;

        /// <summary>
        /// Путь в реестре к ключу QP
        /// </summary>
        public static string QpKeyRegistryPath => Is64BitMode ? Registry.Path64 : Registry.Path;

        /// <summary>
        /// Получение переменной приложения из QP конфига
        /// </summary>
        public static string ConfigVariable(string name)
        {
            if (ConfigServiceUrl != null && ConfigServiceToken != null)
            {
                var service = new CachedQPConfigurationService(ConfigServiceUrl, ConfigServiceToken);

                var variables = AsyncHelper.RunSync(() => service.GetVariables());

                var variable = variables.SingleOrDefault(v => v.Name == name);

                return variable?.Value ?? String.Empty;
            }

            XElement elem = XmlConfig
                .Descendants("app_var")
                .SingleOrDefault(n => n.Attribute("app_var_name")?.Value == name);

            return elem?.Value ?? String.Empty;
        }

        public static string GetConnectionString(string customerCode, string appName = "QP8Backend")
        {
            return GetConnectionInfo(customerCode, appName).ConnectionString;
        }

        public static QpConnectionInfo GetConnectionInfo(string customerCode, string appName = "QP8Backend")
        {
            if (!String.IsNullOrWhiteSpace(customerCode))
            {
                string connectionString;
                DatabaseType dbType = default(DatabaseType);
                if (ConfigServiceUrl != null && ConfigServiceToken != null)
                {
                    var service = new CachedQPConfigurationService(ConfigServiceUrl, ConfigServiceToken);

                    var customer = AsyncHelper.RunSync(() => service.GetCustomer(customerCode));

                    // TODO: handle 404

                    if (customer == null)
                    {
                        throw new Exception($"Данный customer code: {customerCode} - отсутствует в конфиге");
                    }

                    connectionString = customer.ConnectionString;
                    dbType = (DatabaseType)(int)customer.DbType;
                }
                else
                {
                    XElement customerElement = XmlConfig
                        .Descendants("customer")
                        .SingleOrDefault(n => n.Attribute("customer_name")?.Value == customerCode);

                    if (customerElement == null)
                    {
                        throw new Exception($"Данный customer code: {customerCode} - отсутствует в конфиге");
                    }

                    connectionString = customerElement.Element("db")?.Value;
                    var dbTypeString = customerElement.Attribute("db_type")?.Value;
                    if (!string.IsNullOrEmpty(dbTypeString) && Enum.TryParse(dbTypeString, true, out DatabaseType parsed))
                    {
                        dbType = parsed;
                    }
                }

                if (!String.IsNullOrEmpty(connectionString))
                {
                    connectionString = TuneConnectionString(connectionString, appName, dbType);
                    return new QpConnectionInfo(connectionString, dbType);
                }
            }

            return null;
        }

        public static List<QaConfigCustomer> GetCustomers(string appName)
        {
            List<QaConfigCustomer> customers;

            if (ConfigServiceUrl != null && ConfigServiceToken != null)
            {
                var service = new CachedQPConfigurationService(ConfigServiceUrl, ConfigServiceToken);

                customers = AsyncHelper.RunSync(() => service.GetCustomers()).ConvertAll(c => new QaConfigCustomer
                {
                    CustomerName = c.Name,
                    ExcludeFromSchedulers = c.ExcludeFromSchedulers,
                    ConnectionString = c.ConnectionString,
                    DbType = (DatabaseType)(int)c.DbType
                });
            }
            else
            {
                customers = GetQaConfiguration().Customers.ToList();
            }

            foreach (QaConfigCustomer entry in customers)
            {
                entry.ConnectionString = TuneConnectionString(entry.ConnectionString, appName, entry.DbType);
            }

            return customers;
        }

        public static List<string> GetCustomerCodes()
        {
            if (ConfigServiceUrl != null && ConfigServiceToken != null)
            {
                var service = new CachedQPConfigurationService(ConfigServiceUrl, ConfigServiceToken);

                var customers = AsyncHelper.RunSync(() => service.GetCustomers());

                return customers.ConvertAll(c => c.Name);
            }

            return GetQaConfiguration().Customers.Select(c => c.CustomerName).ToList();
        }

        public static string TuneConnectionString(string connectionString, string appName = "QpApp", DatabaseType dbType = DatabaseType.SqlServer)
        {
            DbConnectionStringBuilder cnsBuilder;
            if (dbType == DatabaseType.SqlServer)
            {
                var sqlConnectionStringBuilder = new SqlConnectionStringBuilder(connectionString.Replace("Provider=SQLOLEDB;", string.Empty));
                cnsBuilder = sqlConnectionStringBuilder;
                sqlConnectionStringBuilder.ApplicationName = appName;

                if (sqlConnectionStringBuilder.ConnectTimeout < 120)
                {
                    sqlConnectionStringBuilder.ConnectTimeout = 120;
                }
            }
            else
            {
                cnsBuilder = new NpgsqlConnectionStringBuilder(connectionString);
            }

            return cnsBuilder.ToString();
        }

        public static string TempDirectory
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_tempDirectory))
                {
                    if (!string.IsNullOrEmpty(Options.TempDirectory))
                    {
                        _tempDirectory = Options.TempDirectory;
                    }
                    else
                    {
                        _tempDirectory = ConfigVariable(Config.TempKey).ToLowerInvariant();
                    }
                }

                return _tempDirectory;
            }
            set => _tempDirectory = value;
        }

        public static bool UseScheduleService => ConfigVariable(Config.UseScheduleService).ToLowerInvariant() == "yes";

        public static string ApplicationTitle => ConfigVariable(Config.ApplicationTitle);

        public static bool AllowSelectCustomerCode
        {
            get
            {
                return Options.AllowSelectCustomerCode || ConfigVariable(Config.AllowSelectCustomerCode).ToLowerInvariant() == "yes";
            }
        }

        public static bool LogJsonAsString => Options.LogJsonAsString;

        public static string ADsConnectionString => ConfigVariable(Config.ADsConnectionStringKey).ToLowerInvariant();

        public static string ADsConnectionUsername => ConfigVariable(Config.ADsConnectionUsernameKey);

        public static string ADsConnectionPassword => ConfigVariable(Config.ADsConnectionPasswordKey);

        public static string ADsPath => ConfigVariable(Config.ADsPathKey);

        public static string ADsFieldName => ConfigVariable(Config.ADsFieldNameKey).ToLowerInvariant();

        public static XDocument XmlConfig => XDocument.Load(XmlConfigPath);

        public static string XmlConfigPath
        {
            get
            {
                if (_configPath == null)
                {
                    if (!string.IsNullOrEmpty(Options.QpConfigPath))
                    {
                        _configPath = Options.QpConfigPath;
                    }
                    else
                    {
                        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                        {
                            var qKey = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(QpKeyRegistryPath);
                            if (qKey != null)
                            {
                                _configPath = qKey.GetValue(Registry.XmlConfigValue).ToString();
                            }
                            else
                            {
                                throw new Exception("QP is not installed");
                            }
                        }
                    }
                }

                return _configPath;
            }
            set { _configPath = value; }
        }

        public static string ConfigServiceUrl
        {
            get
            {
                if (_configServiceUrl == null)
                {
                    if (!String.IsNullOrEmpty(Options.QpConfigUrl))
                    {
                        _configServiceUrl = Options.QpConfigUrl;
                    }
                    else
                    {
                        _configServiceUrl = String.Empty;
                    }
                }
                return _configServiceUrl != String.Empty ? _configServiceUrl : null;
            }
            set { _configServiceUrl = value; }
        }

        public static string ConfigServiceToken
        {
            get
            {
                if (_configServiceToken == null)
                {
                    if (!String.IsNullOrEmpty(Options.QpConfigToken))
                    {
                        _configServiceToken = Options.QpConfigToken;
                    }
                    else
                    {
                        _configServiceToken = String.Empty;
                    }
                }
                return _configServiceToken != String.Empty ? _configServiceToken : null;
            }
            set { _configServiceToken = value; }
        }

        public static int CommandTimeout => Options.CommandTimeout;


        public static void SetAppSettings(DbConnectorSettings settings)
        {
            settings.MailHost = ConfigVariable(Config.MailHostKey);
            settings.MailLogin = ConfigVariable(Config.MailLoginKey);
            settings.MailPassword = ConfigVariable(Config.MailPasswordKey);
            settings.MailAssemble = ConfigVariable(Config.MailAssembleKey) != "no";
            settings.MailFromName = ConfigVariable(Config.MailFromNameKey);
        }

        private static QaConfiguration GetQaConfiguration() => XmlSerializerHelpers.Deserialize<QaConfiguration>(XmlConfig).LogTraceFormat("Load customers from config {0}");
    }
}
