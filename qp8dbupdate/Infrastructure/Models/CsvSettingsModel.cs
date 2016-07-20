using System.Collections.Generic;

namespace Quantumart.QP8.ConsoleDbUpdate.Infrastructure.Models
{
    internal class CsvSettingsModel : BaseSettingsModel
    {
        public CsvSettingsModel(IList<string> filePathes, string customerCode, string configPath)
            : base(filePathes, customerCode, configPath)
        {
        }
    }
}
