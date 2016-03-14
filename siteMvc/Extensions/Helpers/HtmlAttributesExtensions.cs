using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Linq;
using Quantumart.QP8.Utils;

namespace Quantumart.QP8.WebMvc.Extensions.Helpers
{
	public static class HtmlAttributesExtensions
	{
		private static readonly Regex _spacesRegExp = new Regex(@"\s", RegexOptions.Compiled);

		/// <summary>
		/// Возвращает список названий CSS-классов из строкового значения атрибута class
		/// </summary>
		/// <param name="cssClassesRawString">строковое значение атрибута class</param>
		/// <returns>список названий CSS-классов</returns>
		private static List<String> GetCssClasses(string cssClassesRawString)
		{
			string cssClassesString = _spacesRegExp.Replace(Converter.ToString(cssClassesRawString).Trim(), " ");
			List<string> cssClasses = new List<string>();

			if (cssClassesString.Length > 0)
			{
				cssClasses = cssClassesString.Split(' ').ToList();
			}

			return cssClasses;
		}

		/// <summary>
		/// Проверяет наличие CSS-класса в атрибуте class
		/// </summary>
		/// <param name="htmlAttributes">HTML-атрибуты</param>
		/// <param name="cssClassName">название CSS-класса</param>
		/// <returns>результат проверки (true - есть; false - нет)</returns>
		public static bool ContainsCssClass(this Dictionary<string, object> htmlAttributes, string cssClassName)
		{
			bool result = false;

			if (htmlAttributes.ContainsKey("class"))
			{
				List<string> cssClasses = GetCssClasses(Converter.ToString(htmlAttributes["class"]));
				result = cssClasses.Contains(cssClassName);
			}

			return result;
		}

		public static void AddData(this Dictionary<string, object> htmlAttributes, string dataKey, object dataValue)
		{
			htmlAttributes["data-" + dataKey] = Converter.ToString(dataValue);
		}

		/// <summary>
		/// Добавляет CSS-класс в атрибут class
		/// </summary>
		/// <param name="htmlAttributes">HTML-атрибуты</param>
		/// <param name="cssClassName">название CSS-класса</param>
		public static void AddCssClass(this Dictionary<string, object> htmlAttributes, string cssClassName)
		{
			if (htmlAttributes.ContainsKey("class"))
			{
				if (!htmlAttributes.ContainsCssClass(cssClassName))
				{
					string cssClassesString = Converter.ToString(htmlAttributes["class"]).Trim();
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

		/// <summary>
		/// Удаляет CSS-класс из атрибута class
		/// </summary>
		/// <param name="htmlAttributes">HTML-атрибуты</param>
		/// <param name="cssClassName">название CSS-класса</param>
		public static void RemoveCssClass(this Dictionary<string, object> htmlAttributes, string cssClassName)
		{
			if (htmlAttributes.ContainsKey("class"))
			{
				List<string> cssClasses = GetCssClasses(Converter.ToString(htmlAttributes["class"]));
				cssClasses.Remove(cssClassName);

				string cssClassesString = String.Join(" ", cssClasses.ToArray());
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
						htmlAttributes[m.Key] = m.Value;
				}
			}
			return htmlAttributes;
		}
	}
}