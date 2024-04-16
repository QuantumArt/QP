using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace QP8.Infrastructure.Extensions
{
    public static class ObjectExtensions
    {
        private const string Null = "NULL";

        public static string ToJsonLog(this object entry) => entry.ToJson(Formatting.None, new CamelCasePropertyNamesContractResolver());

        public static string ToJsonLog(this object entry, IContractResolver resolver) => entry.ToJson(Formatting.None, resolver);

        public static string ToJson(this object entry, Formatting formatting = Formatting.Indented) => entry.ToJson(formatting, new CamelCasePropertyNamesContractResolver(), StringEscapeHandling.EscapeHtml);

        public static string ToJson(this object entry, Formatting formatting, IContractResolver resolver, StringEscapeHandling escapeHandling = StringEscapeHandling.Default)
        {
            if (entry == null)
            {
                return Null;
            }

            return JsonConvert.SerializeObject(entry, new JsonSerializerSettings
            {
                Formatting = formatting,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                ContractResolver = resolver,
                DateFormatHandling = DateFormatHandling.MicrosoftDateFormat,
                StringEscapeHandling = escapeHandling
            });
        }
    }
}
