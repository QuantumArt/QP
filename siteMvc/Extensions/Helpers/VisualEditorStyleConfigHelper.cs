using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Web;
using System.Web.Mvc;
using Quantumart.QP8.BLL;

namespace Quantumart.QP8.WebMvc.Extensions.Helpers
{
	public static class VisualEditorStyleConfigHelper
	{
		public static MvcHtmlString IncludeFormat(VisualEditorStyle format)
		{
			string styleString = format.StylesItems.Count() == 0 ? string.Empty : ",styles:{" + String.Join(",", format.StylesItems.Select(x => String.Format("'{0}' : '{1}'",
				x.Name.Trim().Replace(' ', '_'), x.ItemValue)).ToArray()) + "}";
			
			string attrString = format.AttributeItems.Count() == 0 ? string.Empty : ",attributes:{" + String.Join(",", format.AttributeItems.Select(x =>
				String.Format("'{0}' : '{1}'", x.Name, x.ItemValue)).ToArray()) + "}";

			string overrides = String.IsNullOrEmpty(format.OverridesTag) ? string.Empty : string.Format(",overrides: '{0}' ", format.OverridesTag);

			string result = String.Format("config.format_{0} = {{name: '{3}', element : '{0}' {1} {2} {4} }};",
				format.Tag, styleString, attrString, format.Name, overrides);
			
			return MvcHtmlString.Create(result);
		}

		private static string IncludeStyle(VisualEditorStyle style)
		{
			string styleString = style.StylesItems.Count() == 0 ? string.Empty : ",styles:{" + String.Join(",", style.StylesItems.Select(x => String.Format("'{0}' : '{1}'",
				x.Name.Trim().Replace(' ', '_'), x.ItemValue)).ToArray()) + "}";

			string attrString = style.AttributeItems.Count() == 0 ? string.Empty : ",attributes:{" + String.Join(",", style.AttributeItems.Select(x =>
				String.Format("'{0}' : '{1}'", x.Name, x.ItemValue)).ToArray()) + "}";

			string overrides = String.IsNullOrEmpty(style.OverridesTag) ? string.Empty : string.Format(",overrides: '{0}' ", style.OverridesTag);
			
			string result = String.Format("{{ name : '{0}', element : '{1}' {2} {3} {4} }}",
				style.Name, style.Tag, overrides, attrString, styleString);

			return result;
		}

		public static MvcHtmlString StylesIncludeBlock(IEnumerable<VisualEditorStyle> styles)
		{
			string result = string.Join(",", styles.Select(s => IncludeStyle(s)).ToArray());
			return MvcHtmlString.Create(result);
		}		
	}
}