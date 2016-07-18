using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Quantumart.QP8.EntityFramework.Services
{
    public static class RootUtil
    {
        public static T GetAttribute<T>(XElement e, string name, bool required = false, T fallbackValue = default(T))
        {
            return Util.GetAttribute<T>(e, name, required, fallbackValue);
        }

        public static T GetElementValue<T>(XElement e, string name, bool required = false)
        {
            return Util.GetElementValue<T>(e, name, required);
        }

        public static string ToPascal(string input)
        {
            if (input == null) return null;

            switch (input)
            {
                case "STUB_FIELD": return "StubField";
            }

            return string.Join("", input.Split('_').Select(x => x.Trim()).Where(x => !string.IsNullOrEmpty(x))
                .Select(x => x.Length > 1 ? (x[0] + x.Substring(1).ToLower()) : x));
        }
    }
}
