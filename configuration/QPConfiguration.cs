using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.Web.Configuration;
using Quantumart.QP8.Constants;
using System.Collections.Specialized;
using System.Data.SqlClient;

namespace Quantumart.QP8.Configuration
{
    public class QPConfiguration
    {
        private static string _configPath;

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
        /// <param name="name">имя переменной</param>
        /// <returns>значение переменной</returns>
        public static string ConfigVariable(string name)
        {
            var elem = XmlConfig.Descendants("app_var").SingleOrDefault(n => n.Attribute("app_var_name").Value == name);
            return elem?.Value ?? string.Empty;
        }


        /// <summary>
        /// Получение строки подключения к БД из QP конфига
        /// </summary>
        /// <returns>строка подключения</returns>
        public static string ConfigConnectionString(string customerCode, string appName = "QP8Backend")
        {
            if (string.IsNullOrWhiteSpace(customerCode))
            {
                return null;
            }

            var result = XmlConfig.Descendants("customer").Single(n => n.Attribute("customer_name").Value == customerCode).Element("db").Value;
            return TuneConnectionString(result, appName);
        }

        /// <summary>
        /// Получение всех строк подключения к БД из QP конфига
        /// </summary>
        /// <returns>строка подключения</returns>
        public static IEnumerable<string> ConfigConnectionStrings(string appName = null, IEnumerable<string> exceptCustomerCodes = null)
        {
            exceptCustomerCodes = exceptCustomerCodes ?? new string[0];
            var result = XmlConfig.Descendants("customer")
                .Where(n => !exceptCustomerCodes.Contains(n.Attribute("customer_name").Value, StringComparer.InvariantCultureIgnoreCase))
                .Select(n => TuneConnectionString(n.Element("db").Value, appName));

            return result.ToArray();
        }


        public static string[] CustomerCodes
        {
            get
            {
                var result = XmlConfig.Descendants("customer").Select(n => n.Attribute("customer_name").Value);
                return result.ToArray();
            }
        }

        private static string TuneConnectionString(string connectionString, string appName = null)
        {
            var builder = new SqlConnectionStringBuilder(connectionString.Replace("Provider=SQLOLEDB;", ""));
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

        public static string TempDirectory => ConfigVariable(Config.TempKey).ToLowerInvariant();

        public static bool UseScheduleService => ConfigVariable(Config.UseScheduleService).ToLowerInvariant() == "yes";

        public static string ApplicationTitle => ConfigVariable(Config.ApplicationTitle);

        public static bool AllowSelectCustomerCode => ConfigVariable(Config.AllowSelectCustomerCode).ToLowerInvariant() == "yes";

        public static string ADsConnectionString => ConfigVariable(Config.ADsConnectionStringKey).ToLowerInvariant();

        public static string ADsConnectionUsername => ConfigVariable(Config.ADsConnectionUsernameKey);
  
        public static string ADsConnectionPassword => ConfigVariable(Config.ADsConnectionPasswordKey);

        public static string ADsPath => ConfigVariable(Config.ADsPathKey);

        public static string ADsFieldName => ConfigVariable(Config.ADsFieldNameKey).ToLowerInvariant();

        /// <summary>
        /// Конфигурационный файл QP
        /// </summary>
        public static XDocument XmlConfig => XDocument.Load(XmlConfigPath);

        /// <summary>
        /// Путь к конфигурационному файлу в реестре
        /// </summary>
        private static string XmlConfigPath
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
                            _configPath = qKey.GetValue(Registry.XmlConfigValue).ToString();
                        else
                            throw new Exception("QP is not installed");
                    }
                }

                return _configPath;
            }
        }

        /// <summary>
        /// Возвращает кастомную конфигурационную секцию в файле web.config
        /// </summary>
        /// <returns></returns>
        public static QPublishingSection WebConfigSection => WebConfigurationManager.GetSection("qpublishing") as QPublishingSection;

        public static void SetAppSettings(NameValueCollection settings)
        {
            settings[Config.MailHostKey] = ConfigVariable(Config.MailHostKey);
            settings[Config.MailLoginKey] = ConfigVariable(Config.MailLoginKey);
            settings[Config.MailPasswordKey] = ConfigVariable(Config.MailPasswordKey);
            settings[Config.MailAssembleKey] = (ConfigVariable(Config.MailAssembleKey) != "no") ? "yes" : ""; // inverting backend and frontend default logic
            settings[Config.MailFromNameKey] = ConfigVariable(Config.MailFromNameKey);
        }
    }
}
