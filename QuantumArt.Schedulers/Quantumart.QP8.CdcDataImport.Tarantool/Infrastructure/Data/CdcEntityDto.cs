using System.Collections.Generic;
using Newtonsoft.Json;

namespace Quantumart.QP8.CdcDataImport.Tarantool.Infrastructure.Data
{
    internal class CdcEntityDto
    {
        [JsonProperty("entityType")]
        public string EntityType { get; set; }

        [JsonProperty("invariantName")]
        public string InvariantName { get; set; }

        [JsonProperty("columns")]
        public IDictionary<string, object> Columns { get; set; }
    }
}
