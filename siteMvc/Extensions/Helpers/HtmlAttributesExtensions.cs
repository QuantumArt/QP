using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Quantumart.QP8.Utils;

namespace Quantumart.QP8.WebMvc.Extensions.Helpers
{
    public static class HtmlAttributesExtensions
    {
        private static readonly Regex SpacesRegExp = new Regex(@"\s", RegexOptions.Compiled);

        private static List<string> GetCssClasses(string cssClassesRawString)
        {
            var cssClassesString = SpacesRegExp.Replace(Converter.ToString(cssClassesRawString).Trim(), " ");
            var cssClasses = new List<string>();
            if (cssClassesString.Length > 0)
            {
                cssClasses = cssClassesString.Split(' ').ToList();
            }

            return cssClasses;
        }

        public static bool ContainsCssClass(this Dictionary<string, object> htmlAttributes, string cssClassName)
        {
            var result = false;
            if (htmlAttributes.ContainsKey("class"))
            {
                var cssClasses = GetCssClasses(Converter.ToString(htmlAttributes["class"]));
                result = cssClasses.Contains(cssClassName);
            }

            return result;
        }

        public static void AddData(this Dictionary<string, object> htmlAttributes, string dataKey, object dataValue)
        {
            htmlAttributes["data-" + dataKey] = Converter.ToString(dataValue);
        }

        public static void CopyValueIfExists(this Dictionary<string, object> htmlAttributes, Dictionary<string, object> fromHtmlAttributes, string dataKey)
        {
            if (fromHtmlAttributes.ContainsKey(dataKey))
            {
                htmlAttributes[dataKey] = fromHtmlAttributes[dataKey];
            }
        }

        public static object GetAndRemove(this Dictionary<string, object> htmlAttributes, string dataKey)
        {
            if (!htmlAttributes.ContainsKey(dataKey))
            {
                return null;
            }

            var result = htmlAttributes[dataKey];
            htmlAttributes.Remove(dataKey);

            return result;
        }

        public static void AddCssClass(this Dictionary<string, object> htmlAttributes, string cssClassName)
        {
            if (htmlAttributes.ContainsKey("class"))
            {
                if (!htmlAttributes.ContainsCssClass(cssClassName))
                {
                    var cssClassesString = Converter.ToString(htmlAttributes["class"]).Trim();
                    if (cssClassesString.Length > 0)
                    {
                        cssClassesString += " " + cssClassName;
                    }
                    else
                    {
                        cssClassesString = cssClassName;
                    }

                    htmlAttributes["class"] = cssClassesString;
                }
            }
            else
            {
                htmlAttributes.Add("class", cssClassName);
            }
        }

        public static void RemoveCssClass(this Dictionary<string, object> htmlAttributes, string cssClassName)
        {
            if (htmlAttributes.ContainsKey("class"))
            {
                var cssClasses = GetCssClasses(Converter.ToString(htmlAttributes["class"]));
                cssClasses.Remove(cssClassName);

                var cssClassesString = string.Join(" ", cssClasses.ToArray());
                htmlAttributes["class"] = cssClassesString;
            }
        }

        public static Dictionary<TKey, TValue> Merge<TKey, TValue>(this Dictionary<TKey, TValue> htmlAttributes, Dictionary<TKey, TValue> mergedAttributes, bool @override = false)
        {
            if (mergedAttributes != null)
            {
                foreach (var m in mergedAttributes)
                {
                    if (!htmlAttributes.ContainsKey(m.Key) || @override)
                    {
                        htmlAttributes[m.Key] = m.Value;
                    }
                }
            }

            return htmlAttributes;
        }
    }
}
