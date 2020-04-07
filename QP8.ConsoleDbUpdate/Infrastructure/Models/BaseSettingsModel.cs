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
        internal readonly DatabaseType DbType;

        [JsonProperty]
        internal readonly string ConfigPath;

        [JsonProperty]
        internal readonly int UserId;

        public BaseSettingsModel(IList<string> pathes, string customerCode, DatabaseType dbType, string configPath)
        {
            FilePathes = pathes;
            CustomerCode = customerCode;
            DbType = dbType;
            ConfigPath = configPath;
            UserId = 1;
        }
    }
}
