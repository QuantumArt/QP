using System.Collections.Generic;
using Newtonsoft.Json;
using Quantumart.QP8.Constants;

namespace Quantumart.QP8.ConsoleDbUpdate.Infrastructure.Models
{
    internal class BaseSettingsModel
    {
        [JsonProperty]
        internal readonly IList<string> FilePathes;

        [JsonProperty]
        internal readonly string CustomerCode;

        [JsonProperty]
        internal DatabaseType DbType;

        [JsonProperty]
        internal readonly string ConfigPath;

        [JsonProperty]
        internal readonly int UserId;

        [JsonProperty]
        internal readonly string QpConfigPath;

        [JsonProperty]
        internal readonly string QpConfigUrl;

        [JsonProperty]
        internal readonly string QpConfigToken;

        [JsonProperty]
        internal string ConnectionString;

        internal string CustomerCodeOrConnectionString
        {
            get
            {
                return string.IsNullOrWhiteSpace(ConnectionString)
                    ? CustomerCode
                    : ConnectionString;
            }
        }

        public BaseSettingsModel(
            IList<string> pathes,
            string customerCode,
            DatabaseType dbType,
            string configPath,
            string qpConfigUrl,
            string qpConfigToken,
            string qpConfigPath,
            string connectionString)
        {
            FilePathes = pathes;
            CustomerCode = customerCode;
            DbType = dbType;
            ConfigPath = configPath;
            UserId = 1;
            QpConfigUrl = qpConfigUrl;
            QpConfigToken = qpConfigToken;
            QpConfigPath = qpConfigPath;
            ConnectionString = connectionString;
        }
    }
}
