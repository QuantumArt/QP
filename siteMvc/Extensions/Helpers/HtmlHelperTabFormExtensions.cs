using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using Quantumart.QP8.Resources;
using Quantumart.QP8.WebMvc.ViewModels;
using Quantumart.QP8.Constants;

namespace Quantumart.QP8.WebMvc.Extensions.Helpers
{
    public static class HtmlHelperTabFormExtensions
    {
		public class TabForm : IDisposable
		{
			private bool _disposed;
			private readonly HtmlHelper _html;

			public TabForm(HtmlHelper html)
			{
				if (html == null)
				{
					throw new ArgumentNullException("html");
				}

				this._html = html;
			}

			public void Dispose()
			{
				Dispose(true /* disposing */);
				GC.SuppressFinalize(this);
			}

			protected virtual void Dispose(bool disposing)
			{
				if (!this._disposed)
				{
					this._disposed = true;

					HtmlHelperTabFormExtensions.EndTabForm(this._html);
				}
			}
		}

        /// <summary>
		/// Генерирует начальную часть контейнера стандартной формы QP8
        /// </summary>
		/// <param name="html">HTML-хелпер</param>
        /// <param name="id">идентификатор контейнера</param>
		/// <returns>объект типа TabForm</returns>
		public static TabForm BeginTabForm(this HtmlHelper html, ViewModel model, string actionName = null, string controlerName = null, object routeValues = null)
        {
			if (String.IsNullOrWhiteSpace(actionName) || String.IsNullOrWhiteSpace(controlerName))
				html.BeginForm();
			else
				html.BeginForm(actionName, controlerName, routeValues);

			TextWriter writer = html.ViewContext.Writer;
			writer.WriteLine(String.Format(@"<div class=""editingForm"" id=""{0}"">", html.UniqueId("editingForm")));
			writer.WriteLine(@"<div class=""formLayout"">");

			MvcHtmlString summary = html.ValidationSummary(GlobalStrings.ErrorSummary_FormErrors, new { id = model.ValidationSummaryId, style = "margin-top: 0.5em;" });
			if (summary != null)
			{
				writer.Write(summary.ToString());
			}

			return new TabForm(html);
        }

		/// <summary>
		/// Генерирует конечную часть контейнера стандартной формы QP8
		/// </summary>
		/// <param name="html">HTML-хелпер</param>
        public static void EndTabForm(this HtmlHelper html)
        {
			TextWriter writer = html.ViewContext.Writer;
			writer.WriteLine(String.Format("<input type=\"hidden\" name=\"{0}\" />", Default.ActionCodeHiddenName));
			writer.WriteLine(@"</div>");
			writer.WriteLine(@"</div>");

            html.EndForm();
        }
    }
}
