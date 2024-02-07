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
        internal readonly int UserId;

        [JsonProperty]
        internal string ConnectionString;

        [JsonProperty]
        internal string RecordPath;

        internal string CustomerCodeOrConnectionString => string.IsNullOrWhiteSpace(ConnectionString)
                    ? CustomerCode
                    : ConnectionString;

        public BaseSettingsModel(
            IList<string> pathes,
            string customerCode,
            DatabaseType dbType,
            string connectionString,
            string recordPath)
        {
            FilePathes = pathes;
            CustomerCode = customerCode;
            DbType = dbType;
            UserId = 1;
            ConnectionString = connectionString;
            RecordPath = recordPath;
        }
    }
}
