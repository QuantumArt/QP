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
        public static string ToJson(this ExpandoObject value)
        {
            // TODO: move to json net helpers
            return new JavaScriptSerializer().Serialize(new Dictionary<string, object>(value));
        }
    }
}
