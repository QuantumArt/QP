using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Linq.Expressions;
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

					HtmlHelperLinkButtonsListExtensions.EndLinkButtonsList(this._html);
				}
			}
		}


		/// <summary>
		/// Генерирует начальную часть списка кнопок-гиперссылок
		/// </summary>
		/// <param name="html">HTML-хелпер</param>
		/// <param name="id">идентификатор списка</param>
		/// <returns>объект типа LinkButtonsList</returns>
		public static LinkButtonsList BeginLinkButtonsList(this HtmlHelper html)
		{
			html.ViewContext.Writer.Write(@"<ul class=""linkButtons group"">");

			return new LinkButtonsList(html);
		}

		/// <summary>
		/// Генерирует конечную часть списка кнопок-гиперссылок
		/// </summary>
		/// <param name="html">HTML-хелпер</param>
		public static void EndLinkButtonsList(this HtmlHelper html)
		{
			html.ViewContext.Writer.Write("</ul>");
		}
	}
}