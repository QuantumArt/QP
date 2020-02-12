using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Data.SqlClient;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Web.Configuration;
using System.Xml.Linq;
using QP.ConfigurationService.Client;
using QP8.Infrastructure.Helpers;
using QP8.Infrastructure.Logging.Extensions;
using Quantumart.QP8.Configuration.Models;
using Quantumart.QP8.Constants;
using Quantumart.QP8.Utils;

namespace Quantumart.QP8.Configuration
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public class QPConfiguration
    {
        internal static string _configPath;

        internal static string _tempDirectory;

        internal static string _configServiceUrl;

        internal static string _configServiceToken;

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
            if (!String.IsNullOrWhiteSpace(customerCode))
            {
                string connectionString;
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
                }

                if (!String.IsNullOrEmpty(connectionString))
                {
                    return TuneConnectionString(connectionString, appName);
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
                    ConnectionString = c.ConnectionString
                });
            }
            else
            {
                customers = GetQaConfiguration().Customers.ToList();
            }

            foreach (QaConfigCustomer entry in customers)
            {
                entry.ConnectionString = TuneConnectionString(entry.ConnectionString, appName);
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

        public static string TuneConnectionString(string connectionString, string appName = null)
        {
            try
            {
                var cnsBuilder = new SqlConnectionStringBuilder(connectionString.Replace("Provider=SQLOLEDB;", string.Empty));
                if (!string.IsNullOrWhiteSpace(appName))
                {
                    cnsBuilder.ApplicationName = appName;
                }
                else
                {
                    cnsBuilder.ApplicationName = string.IsNullOrWhiteSpace(appName)
                        ? cnsBuilder.ApplicationName ?? "QpApp"
                        : cnsBuilder.ApplicationName = appName;
                }

                if (cnsBuilder.ConnectTimeout < 120)
                {
                    cnsBuilder.ConnectTimeout = 120;
                }

                return cnsBuilder.ToString();
            }
            catch
            {
                return connectionString;
            }


        }

        public static string TempDirectory
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_tempDirectory))
                {
                    _tempDirectory = ConfigVariable(Config.TempKey).ToLowerInvariant();
                }

                return _tempDirectory;
            }
            set => _tempDirectory = value;
        }

        public static bool UseScheduleService => ConfigVariable(Config.UseScheduleService).ToLowerInvariant() == "yes";

        public static string ApplicationTitle => ConfigVariable(Config.ApplicationTitle);

        public static bool AllowSelectCustomerCode => ConfigVariable(Config.AllowSelectCustomerCode).ToLowerInvariant() == "yes";

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
                    if (!string.IsNullOrEmpty(WebConfigSection?.QpConfigPath))
                    {
                        _configPath = WebConfigSection.QpConfigPath;
                    }
                    else if (!string.IsNullOrEmpty(AppConfigSection?.QpConfigPath))
                    {
                        _configPath = AppConfigSection.QpConfigPath;
                    }
                    else
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

                return _configPath;
            }
        }

        public static string ConfigServiceUrl
        {
            get
            {
                if (_configServiceUrl == null)
                {
                    if (!String.IsNullOrEmpty(WebConfigSection?.QpConfigUrl))
                    {
                        _configServiceUrl = WebConfigSection.QpConfigUrl;
                    }
                    else if (!String.IsNullOrEmpty(AppConfigSection?.QpConfigUrl))
                    {
                        _configServiceUrl = AppConfigSection.QpConfigUrl;
                    }
                    else
                    {
                        _configServiceUrl = String.Empty;
                    }
                }
                return _configServiceUrl != String.Empty ? _configServiceUrl : null;
            }
        }

        public static string ConfigServiceToken
        {
            get
            {
                if (_configServiceToken == null)
                {
                    if (!String.IsNullOrEmpty(WebConfigSection?.QpConfigToken))
                    {
                        _configServiceToken = WebConfigSection.QpConfigToken;
                    }
                    else if (!String.IsNullOrEmpty(AppConfigSection?.QpConfigToken))
                    {
                        _configServiceToken = AppConfigSection.QpConfigToken;
                    }
                    else
                    {
                        _configServiceToken = String.Empty;
                    }
                }
                return _configServiceToken != String.Empty ? _configServiceToken : null;
            }
        }

        /// <summary>
        /// Возвращает кастомную конфигурационную секцию в файле web.config
        /// </summary>
        public static QPublishingSection WebConfigSection => WebConfigurationManager.GetSection("qpublishing") as QPublishingSection;

        /// <summary>
        /// Возвращает кастомную конфигурационную секцию в файле web.config
        /// </summary>
        public static QPublishingSection AppConfigSection => ConfigurationManager.GetSection("qpublishing") as QPublishingSection;

        public static void SetAppSettings(NameValueCollection settings)
        {
            settings[Config.MailHostKey] = ConfigVariable(Config.MailHostKey);
            settings[Config.MailLoginKey] = ConfigVariable(Config.MailLoginKey);
            settings[Config.MailPasswordKey] = ConfigVariable(Config.MailPasswordKey);
            settings[Config.MailAssembleKey] = ConfigVariable(Config.MailAssembleKey) != "no" ? "yes" : string.Empty;
            settings[Config.MailFromNameKey] = ConfigVariable(Config.MailFromNameKey);
        }

        private static QaConfiguration GetQaConfiguration() => XmlSerializerHelpers.Deserialize<QaConfiguration>(XmlConfig).LogTraceFormat("Load customers from config {0}");
    }
}
