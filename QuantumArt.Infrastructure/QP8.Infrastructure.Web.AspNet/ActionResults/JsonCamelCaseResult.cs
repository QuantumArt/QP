using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace QP8.Infrastructure.Web.ActionResults
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

        public static implicit operator JsonCamelCaseResult<T>(T data) => new JsonCamelCaseResult<T>(data);
    }
}
