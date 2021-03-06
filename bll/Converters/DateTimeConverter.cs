using System;
using Newtonsoft.Json;

namespace Quantumart.QP8.BLL.Converters
{
    internal class DateTimeConverter : JsonConverter
    {
        public override bool CanConvert(Type type)
        {
            return type == typeof(DateTime);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            throw new NotSupportedException();
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value == null)
            {
                serializer.Serialize(writer, null);
            }
            else
            {
                string dateString = ((DateTime)value).ToString();
                serializer.Serialize(writer, dateString);
            }
        }
    }
}
