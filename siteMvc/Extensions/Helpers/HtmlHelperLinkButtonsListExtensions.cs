using System;
using System.Web.Mvc;

namespace Quantumart.QP8.WebMvc.Extensions.Helpers
{
    public static class HtmlHelperLinkButtonsListExtensions
    {
        public class LinkButtonsList : IDisposable
        {
            private bool _disposed;
            private readonly HtmlHelper _html;

            public LinkButtonsList(HtmlHelper html)
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
                    EndLinkButtonsList(_html);
                }
            }
        }

        public static LinkButtonsList BeginLinkButtonsList(this HtmlHelper html)
        {
            html.ViewContext.Writer.Write(@"<ul class=""linkButtons group doctab-title"">");
            return new LinkButtonsList(html);
        }

        public static void EndLinkButtonsList(this HtmlHelper html)
        {
            html.ViewContext.Writer.Write("</ul>");
        }
    }
}
