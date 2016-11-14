using System.Collections.Generic;
using CsvHelper.Configuration;
using Newtonsoft.Json;

namespace Quantumart.QP8.ConsoleDbUpdate.Infrastructure.Models
{
    internal class CsvSettingsModel : BaseSettingsModel
    {
        [JsonProperty]
        public CsvConfiguration CsvConfiguration { get; set; }

        public CsvSettingsModel(IList<string> filePathes, string customerCode, string configPath, CsvConfiguration csvConfiguration)
            : base(filePathes, customerCode, configPath)
        {
            CsvConfiguration = csvConfiguration;
        }
    }
}
