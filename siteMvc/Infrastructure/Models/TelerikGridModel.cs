using System.Collections;
using Newtonsoft.Json;

namespace Quantumart.QP8.WebMvc.Infrastructure.Models
{
    public class TelerikGridModel
    {
        [JsonProperty("data")]
        public IEnumerable Data { get; set; }

        [JsonProperty("total")]
        public int Total { get; set; }

        [JsonProperty("aggregates")]
        public object Aggregates { get; set; }
    }
}
