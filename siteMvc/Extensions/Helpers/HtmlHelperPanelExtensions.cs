using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Web.Mvc;

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
                    throw new ArgumentNullException(nameof(html));
                }

                _html = html;
            }

            public void Dispose()
            {
                Dispose(true /* disposing */);
                GC.SuppressFinalize(this);
            }

            protected virtual void Dispose(bool disposing)
            {
                if (!_disposed)
                {
                    _disposed = true;

                    _html.EndPanel();
                }
            }
        }

        public static Panel BeginPanel(this HtmlHelper html, string id, bool isView, bool isUniqId, Dictionary<string, object> htmlAttributes = null)
        {
            var display = isView ? "block" : "none";

            if (htmlAttributes == null)
            {
                htmlAttributes = new Dictionary<string, object>(0);
            }

            var attrs = string.Join(" ", htmlAttributes.Select(p => $"{p.Key}={p.Value}"));

            html.ViewContext.Writer.Write("<div style=\"display:{1}\" id=\"{0}\" {2}>", isUniqId ? id : html.UniqueId(id), display, attrs);

            return new Panel(html);
        }


        public static Panel BeginPanel(this HtmlHelper html, string id)
        {
            return BeginPanel(html, id, false, false);
        }

        public static Panel BeginPanel(this HtmlHelper html, string id, bool disableControls)
        {
            var htmlAttributes = new Dictionary<string, object>(0);
            if (disableControls)
            {
                htmlAttributes.AddData("disable_controls", true.ToString().ToLowerInvariant());
            }

            return BeginPanel(html, id, false, false, htmlAttributes);
        }


        public static Panel BeginPanel(this HtmlHelper html, Dictionary<string, object> htmlAttributes = null)
        {
            if (htmlAttributes == null)
            {
                htmlAttributes = new Dictionary<string, object>(0);
            }

            var attrs = string.Join(" ", htmlAttributes.Select(p => $"{p.Key}={p.Value}"));
            html.ViewContext.Writer.Write($"<div {attrs} >");
            return new Panel(html);
        }

        public static Panel BeginDocumentPadding(this HtmlHelper html)
        {
            return BeginPanel(html, new Dictionary<string, object> { { "class", "documentPadding" } });
        }

        /// <summary>
        /// Генерирует начальную часть панели для свертывания/развертывания
        /// </summary>
        [SuppressMessage("ReSharper", "ConditionIsAlwaysTrueOrFalse")]
        public static Panel BeginPanel<TModel, TValue>(this HtmlHelper<TModel> html, Expression<Func<TModel, TValue>> expression, TValue selectedValue, bool disableControls = false, Dictionary<string, object> htmlAttributes = null)
        {
            var id = QPSelectListItem.GetDependentPanelId(ExpressionHelper.GetExpressionText(expression), selectedValue.ToString());
            if (htmlAttributes == null)
            {
                htmlAttributes = new Dictionary<string, object>(0);
            }

            if (disableControls)
            {
                htmlAttributes.AddData("disable_controls", disableControls.ToString().ToLowerInvariant());
            }

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
