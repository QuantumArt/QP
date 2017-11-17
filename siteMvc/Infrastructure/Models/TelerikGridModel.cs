using System.Collections;
using Newtonsoft.Json;
using Telerik.Web.Mvc;

namespace Quantumart.QP8.WebMvc.Infrastructure.Models
{
    public class TelerikGridModel : GridModel
    {
        [JsonProperty("data")]
        public new IEnumerable Data { get; set; }

        [JsonProperty("total")]
        public new int Total { get; set; }

        [JsonProperty("aggregates")]
        public new object Aggregates { get; set; }
    }
}
