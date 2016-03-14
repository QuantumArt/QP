using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Web.Mvc;

namespace Quantumart.QP8.WebMvc.Extensions.Helpers
{
    public static class HtmlHelperFieldGroupExtensions
    {
		internal class FieldGroup : IDisposable
		{
			private bool _disposed;
			private readonly HtmlHelper _html;

			public FieldGroup(HtmlHelper html)
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

					HtmlHelperFieldGroupExtensions.EndFieldGroup(this._html);
				}
			}
		}


        /// <summary>
		/// Генерирует начальную часть группы полей стандартной формы QP8
        /// </summary>
		/// <param name="html">HTML-хелпер</param>
        /// <param name="title">заголовок группы</param>
		/// <returns>объект типа FieldGroup</returns>
        public static IDisposable BeginFieldGroup(this HtmlHelper html, string title) {
			html.ViewContext.Writer.WriteLine(String.Format("<fieldset><legend>{0}</legend>", title));
            
            return new FieldGroup(html);
        }

		/// <summary>
		/// Генерирует конечную часть группы полей стандартной формы QP8
		/// </summary>
		/// <param name="html">HTML-хелпер</param>
        public static void EndFieldGroup(this HtmlHelper html)
        {
            html.ViewContext.Writer.WriteLine("</fieldset>");
        }
    }
}
