using System.Collections.Generic;

namespace Quantumart.QP8.ConsoleDbUpdate.Infrastructure.Models
{
    internal class XmlSettingsModel : BaseSettingsModel
    {
        internal readonly bool DisableFieldIdentity;
        internal readonly bool DisableContentIdentity;

        public XmlSettingsModel(IList<string> filePathes, string customerCode, string configPath, bool disableFieldIdentity, bool disableContentIdentity)
            : base(filePathes, customerCode, configPath)
        {
            DisableFieldIdentity = disableFieldIdentity;
            DisableContentIdentity = disableContentIdentity;
        }
    }
}
