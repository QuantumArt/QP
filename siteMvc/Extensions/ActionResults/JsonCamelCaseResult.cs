using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Quantumart.QP8.WebMvc.Extensions.ActionResults
{
    public class JsonCamelCaseResult<T> : JsonNetResult<T>
    {
        public JsonCamelCaseResult(T data)
            : base(data)
        {
        }

        public JsonCamelCaseResult(T data, JsonSerializerSettings settings)
            : base(data, settings)
        {
        }

        protected override string SerializeToJson(JsonSerializerSettings settings)
        {
            settings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            return base.SerializeToJson(settings);
        }

        public static implicit operator JsonCamelCaseResult<T>(T data)
        {
            return new JsonCamelCaseResult<T>(data);
        }
    }
}
