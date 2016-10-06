using System.Collections.Generic;
using System.Dynamic;
using System.Web.Script.Serialization;

namespace Quantumart.QP8.Utils
{
    public static class ExpandoObjectExtensions
    {
        /// <summary>
        /// Сериализует ExpandoObject в JSON
        /// </summary>
        /// <param name="obj">динамический объект</param>
        /// <returns>JSON-представление объекта</returns>
        public static string ToJson(this ExpandoObject obj)
        {
            var serializer = new JavaScriptSerializer();
            var dictionary = new Dictionary<string, object>(obj);

            return serializer.Serialize(dictionary);
        }
    }
}
