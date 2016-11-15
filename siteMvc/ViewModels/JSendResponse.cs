using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Quantumart.QP8.WebMvc.Infrastructure.Enums;

namespace Quantumart.QP8.WebMvc.ViewModels
{
    public class JSendResponse : JSendResponse<dynamic> { }

    public class JSendResponse<T>
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public JSendStatus Status { get; set; }

        public T Data { get; set; }

        public string Message { get; set; }

        public int Code { get; set; }
    }
}
