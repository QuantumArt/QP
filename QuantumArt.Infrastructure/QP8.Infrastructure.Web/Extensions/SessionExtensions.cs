using System.IO;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace QP8.Infrastructure.Web.Extensions
{
    public static class SessionExtensions
    {
        private static readonly JsonSerializer Serializer = JsonSerializer.Create(new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.All,
            PreserveReferencesHandling = PreserveReferencesHandling.All,
        });

        public static bool HasKey(this ISession session, string key)
        {
            return session.TryGetValue(key, out var _);
        }

        public static void SetValue(this ISession session, string key, object value)
        {
            using (var memoryStream = new MemoryStream())
            using (var streamWriter = new StreamWriter(memoryStream))
            using (var jsonWriter = new JsonTextWriter(streamWriter))
            {
                Serializer.Serialize(jsonWriter, value);
                jsonWriter.Flush();
                session.Set(key, memoryStream.ToArray());
            }
        }

        public static object GetValue(this ISession session, string key)
        {
            if (session.TryGetValue(key, out byte[] bytes))
            {
                using (var memoryStream = new MemoryStream(bytes))
                using (var streamReader = new StreamReader(memoryStream))
                using (var jsonReader = new JsonTextReader(streamReader))
                {
                    return Serializer.Deserialize(jsonReader);
                }
            }
            return null;
        }

        public static T GetValue<T>(this ISession session, string key)
        {
            if (session.TryGetValue(key, out byte[] bytes))
            {
                using (var memoryStream = new MemoryStream(bytes))
                using (var streamReader = new StreamReader(memoryStream))
                using (var jsonReader = new JsonTextReader(streamReader))
                {
                    return Serializer.Deserialize<T>(jsonReader);
                }
            }
            return default(T);
        }
    }
}
