using System.Collections.Generic;
using Newtonsoft.Json;

namespace Quantumart.QP8.ConsoleDbUpdate.Infrastructure.Models
{
    internal class BaseSettingsModel
    {
        [JsonProperty]
        internal readonly IList<string> FilePathes;

        [JsonProperty]
        internal readonly string CustomerCode;

        [JsonProperty]
        internal readonly string ConfigPath;

        [JsonProperty]
        internal readonly int UserId;

        public BaseSettingsModel(IList<string> pathes, string customerCode, string configPath)
        {
            FilePathes = pathes;
            CustomerCode = customerCode;
            ConfigPath = configPath;
            UserId = 1;
        }
    }
}
