using System;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Quantumart.QP8.WebMvc.Extensions.Helpers
{
    public static class HtmlHelperLinkButtonsListExtensions
    {
        private class LinkButtonsList : IDisposable
        {
            private bool _disposed;
            private readonly IHtmlHelper _html;

            public LinkButtonsList(IHtmlHelper html)
            {
                _html = html ?? throw new ArgumentNullException(nameof(html));
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
                    EndLinkButtonsList(_html);
                }
            }
        }

        public static IDisposable BeginLinkButtonsList(this IHtmlHelper html)
        {
            html.ViewContext.Writer.Write(@"<ul class=""linkButtons group doctab-title"">");
            return new LinkButtonsList(html);
        }

        public static void EndLinkButtonsList(this IHtmlHelper html)
        {
            html.ViewContext.Writer.Write("</ul>");
        }
    }
}
