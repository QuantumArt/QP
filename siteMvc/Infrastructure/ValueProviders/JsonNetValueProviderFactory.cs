using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Dynamic;
using System.Globalization;
using System.IO;
using System.Web.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Quantumart.QP8.WebMvc.Infrastructure.ValueProviders
{
    public class JsonNetValueProviderFactory : ValueProviderFactory
    {
        public override IValueProvider GetValueProvider(ControllerContext controllerContext)
        {
            if (controllerContext == null)
            {
                throw new ArgumentNullException(nameof(controllerContext));
            }

            var jsonData = GetDeserializedObject(controllerContext);
            if (jsonData == null)
            {
                return null;
            }

            var backingStore = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            var backingStoreWrapper = new EntryLimitedDictionary(backingStore);

            AddToBackingStore(backingStoreWrapper, string.Empty, jsonData);
            return new DictionaryValueProvider<object>(backingStoreWrapper.InnerDictionary, CultureInfo.CurrentCulture);
        }

        private static object GetDeserializedObject(ControllerContext controllerContext)
        {
            if (!controllerContext.HttpContext.Request.ContentType.StartsWith("application/json", StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            var streamReader = new StreamReader(controllerContext.HttpContext.Request.InputStream);
            var jsonReader = new JsonTextReader(streamReader);
            if (!jsonReader.Read())
            {
                return null;
            }

            var jsonSerializer = new JsonSerializer();
            jsonSerializer.Converters.Add(new ExpandoObjectConverter());

            object jsonData;
            if (jsonReader.TokenType == JsonToken.StartArray)
            {
                jsonData = jsonSerializer.Deserialize<List<ExpandoObject>>(jsonReader);
            }
            else
            {
                jsonData = jsonSerializer.Deserialize<ExpandoObject>(jsonReader);
            }

            return jsonData;
        }

        private static void AddToBackingStore(EntryLimitedDictionary backingStore, string prefix, object value)
        {
            if (value is IDictionary<string, object> d)
            {
                foreach (var entry in d)
                {
                    AddToBackingStore(backingStore, MakePropertyKey(prefix, entry.Key), entry.Value);
                }

                return;
            }

            if (value is IList l)
            {
                for (var i = 0; i < l.Count; i++)
                {
                    AddToBackingStore(backingStore, MakeArrayKey(prefix, i), l[i]);
                }

                return;
            }

            backingStore.Add(prefix, value);
        }

        private static string MakeArrayKey(string prefix, int index) => prefix + "[" + index.ToString(CultureInfo.InvariantCulture) + "]";

        private static string MakePropertyKey(string prefix, string propertyName) => string.IsNullOrEmpty(prefix) ? propertyName : prefix + "." + propertyName;

        private class EntryLimitedDictionary
        {
            private static readonly int MaximumDepth = GetMaximumDepth();
            private int _itemCount;

            internal readonly IDictionary<string, object> InnerDictionary;

            public EntryLimitedDictionary(IDictionary<string, object> innerDictionary)
            {
                InnerDictionary = innerDictionary;
            }

            public void Add(string key, object value)
            {
                if (++_itemCount > MaximumDepth)
                {
                    throw new InvalidOperationException("Request too large");
                }

                InnerDictionary.Add(key, value);
            }

            private static int GetMaximumDepth()
            {
                var appSettings = ConfigurationManager.AppSettings;
                {
                    var valueArray = appSettings.GetValues("aspnet:MaxJsonDeserializerMembers");
                    if (valueArray != null && valueArray.Length > 0)
                    {
                        if (int.TryParse(valueArray[0], out var result))
                        {
                            return result;
                        }
                    }
                }

                return 1000;
            }
        }
    }
}
