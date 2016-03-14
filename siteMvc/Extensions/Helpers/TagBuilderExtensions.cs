using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Html;

namespace Quantumart.QP8.WebMvc.Extensions.Helpers
{
	public static class TagBuilderExtensions
	{
		/// <summary>
		/// Добавляет к HTML-элементу HTML5 data-атрибут
		/// </summary>
		/// <param name="tb">построитель тэга</param>
		/// <param name="key">название атрибута</param>
		/// <param name="value">значение атрибута</param>
		/// <param name="replaceExisting">разрешает замену существующего атрибута</param>
		public static void MergeDataAttribute(this TagBuilder tb, string key, string value, bool replaceExisting = true)
		{
			if (!String.IsNullOrWhiteSpace(key))
			{
				tb.MergeAttribute("data-" + key, value, replaceExisting);
			}
		}
	}
}