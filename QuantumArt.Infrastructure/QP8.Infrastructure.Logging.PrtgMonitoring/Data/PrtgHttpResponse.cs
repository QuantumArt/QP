using Newtonsoft.Json;

namespace QP8.Infrastructure.Logging.PrtgMonitoring.Data
{
    public class PrtgHttpResponse
    {
        [JsonProperty(PropertyName = "status")]
        public string Status { get; set; }

        [JsonProperty(PropertyName = "Matching Sensors")]
        public string BindingType { get; set; }
    }
}
