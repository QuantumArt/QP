using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Linq.Expressions;
using System.Web.Mvc;
using System.Linq;

namespace Quantumart.QP8.WebMvc.Extensions.Helpers
{
    public static class HtmlHelperPanelExtensions
    {
		public class Panel : IDisposable
		{
			private bool _disposed;
			private readonly HtmlHelper _html;

			public Panel(HtmlHelper html)
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

					HtmlHelperPanelExtensions.EndPanel(this._html);
				}
			}
		}

		public static Panel BeginPanel(this HtmlHelper html, string id, bool isView, bool isUniqId, Dictionary<string, object> htmlAttributes = null)
		{
			var display = isView ? "block" : "none";

			if (htmlAttributes == null)
				htmlAttributes = new Dictionary<string, object>(0);

			string attrs = String.Join(" ", htmlAttributes.Select(p => String.Format("{0}={1}", p.Key, p.Value)));

			html.ViewContext.Writer.Write(String.Format("<div style=\"display:{1}\" id=\"{0}\" {2}>", isUniqId ? id : html.UniqueId(id), display, attrs));

			return new Panel(html);
		}


		public static Panel BeginPanel(this HtmlHelper html, string id)
        {
			return BeginPanel(html, id, false, false, null);
        }

		public static Panel BeginPanel(this HtmlHelper html, string id, bool disableControls)
		{
			Dictionary<string, object> htmlAttributes = new Dictionary<string, object>(0);
			if (disableControls)
				htmlAttributes.AddData("disable_controls", disableControls.ToString().ToLowerInvariant());
			return BeginPanel(html, id, false, false, htmlAttributes);
		}


		public static Panel BeginPanel(this HtmlHelper html, Dictionary<string, object> htmlAttributes = null)
		{
			if (htmlAttributes == null)
				htmlAttributes = new Dictionary<string, object>(0);

			string attrs = String.Join(" ", htmlAttributes.Select(p => String.Format("{0}={1}", p.Key, p.Value)));

			html.ViewContext.Writer.Write(String.Format("<div {0} >", attrs));

			return new Panel(html);
		}

        public static Panel BeginDocumentPadding(this HtmlHelper html)
        {
            return BeginPanel(html, new Dictionary<string, object>() { { "class", "documentPadding" } });
        }

		/// <summary>
		/// Генерирует начальную часть панели для свертывания/развертывания
		/// </summary>
		/// <param name="html">HTML-хелпер</param>
		/// <param name="expression">LINQ-выражение</param>
		/// <param name="selectedValue">выбранное значение</param>
		/// <returns>объект типа Panel</returns>
        public static Panel BeginPanel<TModel, TValue>(this HtmlHelper<TModel> html, Expression<Func<TModel, TValue>> expression, TValue selectedValue, bool disableControls = false, Dictionary<string, object> htmlAttributes = null)
        {
			string id = QPSelectListItem.GetDependentPanelId(ExpressionHelper.GetExpressionText(expression), selectedValue.ToString());

			if (htmlAttributes == null)
				htmlAttributes = new Dictionary<string, object>(0);

			if (disableControls)
				htmlAttributes.AddData("disable_controls", disableControls.ToString().ToLowerInvariant());

            return html.BeginPanel(id, false, false, htmlAttributes);

        }

		/// <summary>
		/// Генерирует конечную часть панели для свертывания/развертывания
		/// </summary>
		/// <param name="html">HTML-хелпер</param>
        public static void EndPanel(this HtmlHelper html)
        {
            html.ViewContext.Writer.Write("</div>");
        }
    }
}
