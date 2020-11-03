using System.Collections.Generic;
using System.Data;
using CsvHelper.Configuration;
using Newtonsoft.Json;
using Quantumart.QP8.Constants;

namespace Quantumart.QP8.ConsoleDbUpdate.Infrastructure.Models
{
    internal class CsvSettingsModel : BaseSettingsModel
    {
        [JsonProperty]
        public CsvConfiguration CsvConfiguration { get; set; }

        [JsonProperty]
        public bool UpdateExisting { get; set; }

        public CsvSettingsModel(IList<string> filePathes, string customerCode, DatabaseType dbType, string configPath, CsvConfiguration csvConfiguration, bool updateExisting)
            : base(filePathes, customerCode, dbType, configPath)
        {
            CsvConfiguration = csvConfiguration;
            UpdateExisting = updateExisting;
        }
    }
}
