using System;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Quantumart.QP8.WebMvc.Extensions.Helpers
{
    public static class HtmlHelperFieldGroupExtensions
    {
        private class FieldGroup : IDisposable
        {
            private bool _disposed;
            private readonly IHtmlHelper _html;

            public FieldGroup(IHtmlHelper html)
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
                    _html.EndFieldGroup();
                }
            }
        }

        /// <summary>
        /// Генерирует начальную часть группы полей стандартной формы QP8
        /// </summary>
        /// <param name="html">HTML-хелпер</param>
        /// <param name="title">заголовок группы</param>
        /// <returns>объект типа FieldGroup</returns>
        public static IDisposable BeginFieldGroup(this IHtmlHelper html, string title)
        {
            html.ViewContext.Writer.WriteLine("<fieldset><legend>{0}</legend>", title);
            return new FieldGroup(html);
        }

        /// <summary>
        /// Генерирует конечную часть группы полей стандартной формы QP8
        /// </summary>
        /// <param name="html">HTML-хелпер</param>
        public static void EndFieldGroup(this IHtmlHelper html)
        {
            html.ViewContext.Writer.WriteLine("</fieldset>");
        }
    }
}
