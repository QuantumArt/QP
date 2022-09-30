using System.Collections.Generic;
using Newtonsoft.Json;
using Quantumart.QP8.Constants;

namespace Quantumart.QP8.ConsoleDbUpdate.Infrastructure.Models
{
    internal class XmlSettingsModel : BaseSettingsModel
    {
        [JsonProperty]
        internal readonly bool GenerateNewFieldIds;

        [JsonProperty]
        internal readonly bool GenerateNewContentIds;

        [JsonProperty]
        internal readonly bool UseGuidSubstitution;

        [JsonProperty]
        internal readonly bool DisableDataIntegrity;

        public XmlSettingsModel(
            IList<string> filePathes,
            string customerCode,
            DatabaseType dbType,
            string configPath,
            bool generateNewFieldIds,
            bool generateNewContentIds,
            bool useGuidSubstitution,
            bool disableDataIntegrity,
            string qpConfigUrl,
            string qpConfigToken,
            string qpConfigPath,
            string connectionString)
            : base(filePathes, customerCode, dbType, configPath, qpConfigUrl, qpConfigToken, qpConfigPath, connectionString)
        {
            GenerateNewFieldIds = generateNewFieldIds;
            GenerateNewContentIds = generateNewContentIds;
            UseGuidSubstitution = useGuidSubstitution;
            DisableDataIntegrity = disableDataIntegrity;
        }
    }
}
