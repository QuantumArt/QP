using Newtonsoft.Json;

namespace Quantumart.QP8.CdcDataImport.Tarantool.Infrastructure.Data
{
    internal class CdcChangeDto
    {
        [JsonProperty("action")]
        public string Action { get; set; }

        [JsonProperty("changeType")]
        public string ChangeType { get; set; }

        [JsonProperty("sequenceLsn")]
        public string SequenceLsn { get; set; }

        [JsonProperty("orderNumber")]
        public int OrderNumber { get; set; }

        [JsonProperty("entity")]
        public CdcEntityDto Entity { get; set; }
    }
}
