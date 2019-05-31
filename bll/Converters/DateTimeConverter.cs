using System;
using Newtonsoft.Json;

namespace Quantumart.QP8.BLL.Converters
{
    class DateTimeConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return true;
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            DateTimeOffset date = (DateTime)value;
            serializer.Serialize(writer, date.ToString("d/M/yyyy hh:mm:ss"));
        }
    }
}
