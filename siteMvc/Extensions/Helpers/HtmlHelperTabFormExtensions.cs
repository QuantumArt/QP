using System;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using Quantumart.QP8.Constants;
using Quantumart.QP8.Resources;
using Quantumart.QP8.WebMvc.ViewModels.Abstract;

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
                    throw new ArgumentNullException(nameof(html));
                }

                _html = html;
            }

            public void Dispose()
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }

            protected virtual void Dispose(bool disposing)
            {
                if (!_disposed)
                {
                    _disposed = true;
                    _html.EndTabForm();
                }
            }
        }

        /// <summary>
		/// Генерирует начальную часть контейнера стандартной формы QP8
        /// </summary>
		/// <returns>объект типа TabForm</returns>
		public static TabForm BeginTabForm(this HtmlHelper html, ViewModel model, string actionName = null, string controlerName = null, object routeValues = null)
        {
            if (string.IsNullOrWhiteSpace(actionName) || string.IsNullOrWhiteSpace(controlerName))
            {
                // ReSharper disable once Mvc.ActionNotResolved
                html.BeginForm();
            }
            else
            {
                html.BeginForm(actionName, controlerName, routeValues);
            }

            var writer = html.ViewContext.Writer;
            writer.WriteLine(@"<div class=""editingForm"" id=""{0}"">", html.UniqueId("editingForm"));
            writer.WriteLine(@"<div class=""formLayout"">");

            var summary = html.ValidationSummary(GlobalStrings.ErrorSummary_FormErrors, new { id = model.ValidationSummaryId, style = "margin-top: 0.5em;" });
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
            var writer = html.ViewContext.Writer;
            writer.WriteLine("<input type=\"hidden\" name=\"{0}\" />", Default.ActionCodeHiddenName);
            writer.WriteLine(@"</div>");
            writer.WriteLine(@"</div>");
            html.EndForm();
        }
    }
}
