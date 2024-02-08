using System.Collections.Generic;
using CsvHelper.Configuration;
using Newtonsoft.Json;
using Quantumart.QP8.Constants;

namespace Quantumart.QP8.ConsoleDbUpdate.Infrastructure.Models
{
    internal class CsvSettingsModel : BaseSettingsModel
    {
        [JsonIgnore]
        public CsvConfiguration CsvConfiguration { get; set; }

        [JsonProperty]
        public bool UpdateExisting { get; set; }

        public CsvSettingsModel(
            IList<string> filePathes,
            string customerCode,
            DatabaseType dbType,
            CsvConfiguration csvConfiguration,
            bool updateExisting,
            string connectionString)
            : base(filePathes, customerCode, dbType, connectionString)
        {
            CsvConfiguration = csvConfiguration;
            UpdateExisting = updateExisting;
        }
    }
}
