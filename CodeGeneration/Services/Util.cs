using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Quantumart.QP8.CodeGeneration.Services
{
    public static class Util
    {
        public static T GetAttribute<T>(XElement e, string name, bool required = false, T fallbackValue = default(T))
        {
            try
            {
                var a = e.Attribute(name);
                if (a == null || a.Value == null || a.Value == "")
                {
                    if (required) throw new Exception("value should not be null or empty");
                    return fallbackValue;
                }
                if (typeof(T) == typeof(bool) && a.Value.Length == 1)
                    return (T)(object)Convert.ToBoolean(Convert.ToInt16(a.Value));

                var converter = TypeDescriptor.GetConverter(typeof(T));
                return (T)converter.ConvertFromInvariantString(a.Value);
            }
            catch (Exception ex)
            {
                throw new Exception("problem with element " + name, ex);
            }
        }

        public static T GetElementValue<T>(XElement e, string name, bool required = false)
        {
            try
            {
                var a = e.Elements().FirstOrDefault(x => x.Name == name);
                if (a == null || a.Value == null || a.Value == "")
                {
                    if (required) throw new Exception("value should not be null or empty");
                    return default(T);
                }
                if (typeof(T) == typeof(bool) && a.Value.Length == 1)
                    return (T)(object)Convert.ToBoolean(Convert.ToInt16(a.Value));

                var converter = TypeDescriptor.GetConverter(typeof(T));
                return (T)converter.ConvertFromInvariantString(a.Value);
            }
            catch (Exception ex)
            {
                throw new Exception("problem with element " + name, ex);
            }
        }
    }
}
