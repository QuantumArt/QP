using System;
using System.ComponentModel;
using System.Xml.Linq;

namespace Quantumart.QP8.WebMvc.Infrastructure.Extensions
{
    public static class XAttributeExtensions
    {
        public static string GetValueOrDefault(this XAttribute attr, string defaultValue = null) => attr?.Value ?? defaultValue;

        public static T GetValueOrDefault<T>(this XAttribute attr)
            where T : IConvertible
        {
            if (attr == null)
            {
                return default(T);
            }

            return (T)TypeDescriptor.GetConverter(typeof(T)).ConvertFromInvariantString(attr.Value);
        }
    }
}
