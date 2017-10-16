using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data.SqlClient;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Web.Configuration;
using System.Xml.Linq;
using QP8.Infrastructure.Helpers;
using QP8.Infrastructure.Logging.Extensions;
using Quantumart.QP8.Configuration.Models;
using Quantumart.QP8.Constants;

namespace Quantumart.QP8.Configuration
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public class QPConfiguration
    {
        internal static string _configPath;

        internal static string _tempDirectory;

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
            var elem = XmlConfig.Descendants("app_var").SingleOrDefault(n => n.Attribute("app_var_name")?.Value == name);
            return elem?.Value ?? string.Empty;
        }

        public static string GetConnectionString(string customerCode, string appName = "QP8Backend")
        {
            if (!string.IsNullOrWhiteSpace(customerCode))
            {
                var customerElement = XmlConfig.Descendants("customer").SingleOrDefault(n => n.Attribute("customer_name")?.Value == customerCode);
                if (customerElement == null)
                {
                    throw new Exception($"Данный customer code: {customerCode} - отсутствует в конфиге");
                }

                var dbConnectionString = customerElement.Element("db");
                if (dbConnectionString != null)
                {
                    return TuneConnectionString(dbConnectionString.Value, appName);
                }
            }

            return null;
        }

        public static List<QaConfigCustomer> GetCustomers(string appName)
        {
            var customers = GetQaConfiguration().Customers.ToList();
            foreach (var entry in customers)
            {
                entry.ConnectionString = TuneConnectionString(entry.ConnectionString, appName);
            }

            return customers;
        }

        public static List<string> GetCustomerCodes()
        {
            return GetQaConfiguration().Customers.Select(c => c.CustomerName).ToList();
        }

        private static string TuneConnectionString(string connectionString, string appName = null)
        {
            var builder = new SqlConnectionStringBuilder(connectionString.Replace("Provider=SQLOLEDB;", string.Empty));
            if (!string.IsNullOrWhiteSpace(appName))
            {
                builder.ApplicationName = appName;
            }

            if (builder.ConnectTimeout < 120)
            {
                builder.ConnectTimeout = 120;
            }

            return builder.ToString();
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

        /// <summary>
        /// Возвращает кастомную конфигурационную секцию в файле web.config
        /// </summary>
        public static QPublishingSection WebConfigSection => WebConfigurationManager.GetSection("qpublishing") as QPublishingSection;

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
