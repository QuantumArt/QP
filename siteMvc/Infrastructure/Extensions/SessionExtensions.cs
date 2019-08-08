using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace Quantumart.QP8.WebMvc.Infrastructure.Extensions
{
    public static class SessionExtensions
    {
        private static JsonSerializerSettings _settings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.All,
            PreserveReferencesHandling = PreserveReferencesHandling.All,
        };

        public static void SetValue<T>(this ISession session, string key, T value)
        {
            session.SetString(key, JsonConvert.SerializeObject(value, _settings));
        }

        public static object GetValue(this ISession session, string key)
        {
            var value = session.GetString(key);
            return value == null ? null : JsonConvert.DeserializeObject(value, _settings);
        }

        public static T GetValue<T>(this ISession session, string key)
        {
            var value = session.GetString(key);
            return value == null ? default(T) : JsonConvert.DeserializeObject<T>(value, _settings);
        }
    }
}
