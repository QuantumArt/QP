using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Quantumart.QP8.WebMvc.Infrastructure.Settings
{
    public static class JsonSettingsRegistry
    {
        public static JsonSerializerSettings CamelCaseSettings = new JsonSerializerSettings
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            ContractResolver = new CamelCasePropertyNamesContractResolver()
        };

        public static JsonSerializerSettings MicrosoftDateSettings = new JsonSerializerSettings
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            DateFormatHandling = DateFormatHandling.MicrosoftDateFormat,
        };
    }
}
