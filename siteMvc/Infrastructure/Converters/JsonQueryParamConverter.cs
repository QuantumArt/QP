using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Quantumart.QP8.BLL;

namespace Quantumart.QP8.WebMvc.Infrastructure.Converters
{
    internal class JsonQueryParamConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) => throw new NotImplementedException();

        public override bool CanConvert(Type objectType) => true;

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType != JsonToken.StartArray)
            {
                throw new NotImplementedException("Unknown object was passed to deserializer");
            }

            var jData = JArray.Load(reader);
            var searchQueryParams = jData.ToObject<IList<ArticleSearchQueryParam>>();
            foreach (var query in searchQueryParams)
            {
                for (var i = 0; i < query.QueryParams.Length; i++)
                {
                    if (query.QueryParams[i] is JArray jArray)
                    {
                        query.QueryParams[i] = jArray.ToObject<object[]>();
                    }
                }
            }

            return searchQueryParams;
        }
    }
}
