using System.Collections.Generic;
using CsvHelper.Configuration;

namespace Quantumart.QP8.ConsoleDbUpdate.Infrastructure.Models
{
    internal class CsvSettingsModel : BaseSettingsModel
    {
        public CsvConfiguration CsvConfiguration { get; set; }

        public CsvSettingsModel(IList<string> filePathes, string customerCode, string configPath, CsvConfiguration csvConfiguration)
            : base(filePathes, customerCode, configPath)
        {
            CsvConfiguration = csvConfiguration;
        }
    }
}
