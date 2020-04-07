using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Quantumart.QP8.WebMvc.Extensions.Helpers
{
    public static class HtmlHelperPanelExtensions
    {
        private class Panel : IDisposable
        {
            private bool _disposed;
            private readonly IHtmlHelper _html;

            public Panel(IHtmlHelper html)
            {
                _html = html ?? throw new ArgumentNullException(nameof(html));
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

        public static IDisposable BeginPanel(this IHtmlHelper html, string id, bool isView, bool isUniqId, Dictionary<string, object> htmlAttributes = null, bool reverse = false)
        {
            var display = isView ? "block" : "none";

            if (htmlAttributes == null)
            {
                htmlAttributes = new Dictionary<string, object>(0);
            }

            if (reverse)
            {
                htmlAttributes.AddData("reverse", true.ToString().ToLowerInvariant());
            }

            var attrs = string.Join(" ", htmlAttributes.Select(p => $"{p.Key}={p.Value}"));

            html.ViewContext.Writer.Write(
                "<div style=\"display:{1}\" id=\"{0}\" {2}>", isUniqId ? id : html.UniqueId(id), display, attrs);

            return new Panel(html);
        }

        public static IDisposable BeginPanel(this IHtmlHelper html, string id)
        {
            return BeginPanel(html, id, false, false);
        }

        public static IDisposable BeginPanel(this IHtmlHelper html, string id, bool disableControls)
        {
            var htmlAttributes = new Dictionary<string, object>(0);
            if (disableControls)
            {
                htmlAttributes.AddData("disable_controls", true.ToString().ToLowerInvariant());
            }

            return BeginPanel(html, id, false, false, htmlAttributes);
        }

        public static IDisposable BeginPanel(this IHtmlHelper html, Dictionary<string, object> htmlAttributes = null)
        {
            if (htmlAttributes == null)
            {
                htmlAttributes = new Dictionary<string, object>(0);
            }

            var attrs = string.Join(" ", htmlAttributes.Select(p => $"{p.Key}={p.Value}"));
            html.ViewContext.Writer.Write($"<div {attrs} >");
            return new Panel(html);
        }

        public static IDisposable BeginDocumentPadding(this IHtmlHelper html)
        {
            return BeginPanel(html, new Dictionary<string, object> { { "class", "documentPadding" } });
        }

        /// <summary>
        /// Генерирует начальную часть панели для свертывания/развертывания
        /// </summary>
        [SuppressMessage("ReSharper", "ConditionIsAlwaysTrueOrFalse")]
        public static IDisposable BeginPanel<TModel, TValue>(this IHtmlHelper<TModel> html, Expression<Func<TModel, TValue>> expression, TValue selectedValue, bool disableControls = false, Dictionary<string, object> htmlAttributes = null)
        {
            var id = QPSelectListItem.GetDependentPanelId(
                html.ModelExpressionProvider().GetExpressionText(expression),
                selectedValue.ToString());

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
        public static void EndPanel(this IHtmlHelper html)
        {
            html.ViewContext.Writer.Write("</div>");
        }
    }
}
