using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Quantumart.QP8.BLL.Models.NotificationSender;

namespace Quantumart.QP8.CdcDataImport.Tarantool.Infrastructure.Data
{
    internal class CdcChangeDto
    {
        [JsonProperty("action")]
        public string Action { get; set; }

        [JsonProperty("changeType")]
        [JsonConverter(typeof(StringEnumConverter))]
        public CdcActionType ChangeType { get; set; }

        [JsonProperty("orderNumber")]
        public int OrderNumber { get; set; }

        [JsonProperty("entity")]
        public CdcEntityDto Entity { get; set; }
    }
}
