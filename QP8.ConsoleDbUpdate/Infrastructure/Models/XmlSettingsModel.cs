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
            bool generateNewFieldIds,
            bool generateNewContentIds,
            bool useGuidSubstitution,
            bool disableDataIntegrity,
            string connectionString,
            string recordPath)
            : base(filePathes, customerCode, dbType, connectionString, recordPath)
        {
            GenerateNewFieldIds = generateNewFieldIds;
            GenerateNewContentIds = generateNewContentIds;
            UseGuidSubstitution = useGuidSubstitution;
            DisableDataIntegrity = disableDataIntegrity;
        }
    }
}
