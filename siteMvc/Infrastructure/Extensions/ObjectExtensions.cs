using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Quantumart.QP8.WebMvc.Infrastructure.Extensions
{
    public static class ObjectExtensions
    {
        private const string Null = "NULL";

        public static string ToJsonLog(this object entry)
        {
            return entry.ToJson(Formatting.None);
        }

        public static string ToJson(this object entry, Formatting formatting = Formatting.Indented)
        {
            if (entry == null)
            {
                return Null;
            }

            return JsonConvert.SerializeObject(entry, new JsonSerializerSettings
            {
                Formatting = formatting,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            });
        }
    }
}
