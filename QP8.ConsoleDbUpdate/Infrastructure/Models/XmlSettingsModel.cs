using System.Collections.Generic;
using Newtonsoft.Json;

namespace Quantumart.QP8.ConsoleDbUpdate.Infrastructure.Models
{
    internal class XmlSettingsModel : BaseSettingsModel
    {
        [JsonProperty]
        internal readonly bool DisableFieldIdentity;

        [JsonProperty]
        internal readonly bool DisableContentIdentity;

        public XmlSettingsModel(IList<string> filePathes, string customerCode, string configPath, bool disableFieldIdentity, bool disableContentIdentity)
            : base(filePathes, customerCode, configPath)
        {
            DisableFieldIdentity = disableFieldIdentity;
            DisableContentIdentity = disableContentIdentity;
        }
    }
}
