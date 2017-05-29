using Newtonsoft.Json;

namespace QP8.Infrastructure.Web.Responses
{
    public class NginxResponse
    {
        public NginxResponse(object objectToSend)
        {
            Params = new[] { objectToSend };
        }

        [JsonProperty("params")]
        public object[] Params { get; set; }
    }
}
