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
            string configPath,
            CsvConfiguration csvConfiguration,
            bool updateExisting,
            string qpConfigUrl,
            string qpConfigToken,
            string qpConfigPath,
            string connectionString)
            : base(filePathes, customerCode, dbType, configPath, qpConfigUrl, qpConfigToken, qpConfigPath, connectionString)
        {
            CsvConfiguration = csvConfiguration;
            UpdateExisting = updateExisting;
        }
    }
}
