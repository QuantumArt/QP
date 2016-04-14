using System;
using System.Text;
using System.Collections.Generic;
using System.Dynamic;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using Quantumart.QP8;
using Quantumart.QP8.Utils;
using Quantumart.QP8.BLL;
using Quantumart.QP8.Resources;
using Quantumart.QP8.WebMvc.ViewModels;
using Quantumart.QP8.Constants;
using Quantumart.QP8.WebMvc.ViewModels.Field;

namespace Quantumart.QP8.WebMvc.Extensions.Helpers
{
	public static class HtmlHelperPageExtensions
	{
		#region Grid Helpers

		/// <summary>
		/// Генерирует JS-код инициализации страницы
		/// </summary>
		/// <param name="html">HTML-хелпер</param>
		/// <param name="model">модель</param>
		/// <returns>код</returns>
		public static MvcHtmlString PrepareInitScript(this HtmlHelper html, ViewModel model)
		{
			StringBuilder sb = new StringBuilder();
			sb.AppendLine(@"<script>");
			sb.AppendFormatLine(@"var {0} = new Quantumart.QP8.BackendDocumentContext({1}, {2});",
				model.ContextObjectName, model.MainComponentParameters.ToJson(), model.MainComponentOptions.ToJson()
			);
			sb.AppendLine(@"</script>");

			return MvcHtmlString.Create(sb.ToString());
		}

		/// <summary>
		/// Выполняет JS-код инициализации страницы
		/// </summary>
		/// <param name="html">HTML-хелпер</param>
		/// <param name="model">модель</param>
		/// <returns>код</returns>
		public static MvcHtmlString RunInitScript(this HtmlHelper html, ViewModel model)
		{
			StringBuilder sb = new StringBuilder();
			sb.AppendLine(@"<script>");
			sb.AppendFormatLine(@"{0}.init();", model.ContextObjectName);
			sb.AppendLine(@"</script>");
			return MvcHtmlString.Create(sb.ToString());
		}

		public static MvcHtmlString CustomScript(this HtmlHelper html, string script, string contextObjectName)
		{
			StringBuilder sb = new StringBuilder();
			if (!String.IsNullOrEmpty(script))
			{
				string contextPlaceholder = "QP_CURRENT_CONTEXT";

				sb.AppendLine(@"<script>");
				sb.AppendLine(script.Replace(contextPlaceholder, contextObjectName));
				sb.AppendLine(@"</script>");
			}
			return MvcHtmlString.Create(sb.ToString());
		}

		/// <summary>
		/// Генерирует и выполняет JS-код инициализации страницы
		/// </summary>
		/// <param name="html">HTML-хелпер</param>
		/// <param name="model">модель</param>
		/// <returns>код</returns>
		public static MvcHtmlString InitScript(this HtmlHelper html, ViewModel model)
		{
			return MvcHtmlString.Create(html.PrepareInitScript(model).ToString() + html.RunInitScript(model).ToString());
		}
		#endregion
	}
}
