using System;
using System.Text.RegularExpressions;
using System.Web;
using System.IO;

namespace Quantumart.QP8.Utils
{
	public static class Url
	{
		private static Regex _urlFormatRegExp = new Regex(@"^[a-zA-Z0-9\+\.-]+:");

		/// <summary>
		/// Проверяет формат URL
		/// </summary>
		/// <param name="url">URL</param>
		/// <returns>результат проверки (true - корректный формат; false - некорректный)</returns>
		public static bool CheckUrlFormat(string url)
		{
			return _urlFormatRegExp.IsMatch(url);
		}

		/// <summary>
		/// Преобразуется относительный URL 
		/// в абсолютный (с точки зрения приложения) 
		/// </summary>
		/// <param name="url">относительный URL</param>
		/// <returns>абсолютный URL</returns>
		public static string ToAbsolute(string url)
		{
			string result = string.Empty;

			if (url != null)
			{
				url = url.Trim();

				if (url.Length > 0)
				{
					if (!CheckUrlFormat(url) && HttpContext.Current != null && url.StartsWith("~/"))
					{
						result = VirtualPathUtility.ToAbsolute(url);
					}
					else
					{
						result = url;
					}
				}
			}

			return result;
		}

		/// <summary>
		/// Преобразует относительный URL в полный URL (с доменом сайта)
		/// </summary>
		/// <param name="url">относительный URL</param>
		/// <returns>полный URL</returns>
		public static string ToFull(string url)
		{
			string result = string.Empty;

			if (url != null)
			{
				url = url.Trim();

				if (url.Length > 0)
				{
					if (!CheckUrlFormat(url) && HttpContext.Current != null)
					{
						if (url.StartsWith("~/"))
						{
							url = VirtualPathUtility.ToAbsolute(url);
						}

						System.Uri baseUri = new System.Uri(HttpContext.Current.Request.Url.GetLeftPart(UriPartial.Path));
						System.Uri uri = new System.Uri(baseUri, url);
						result = uri.ToString();
					}
					else
					{
						result = url;
					}
				}
			}

			return result;
		}

		/// <summary>
		/// Возвращает строку без первого слэша
		/// </summary>
		/// <param name="url">URL</param>
		/// <returns>обработанный URL</returns>
		public static string RemoveFirstSlash(string url)
		{
			string result = PathUtility.RemoveFirstSlash(url);

			return result;
		}

		/// <summary>
		/// Возвращает URL без последнего слэша
		/// </summary>
		/// <param name="url">URL</param>
		/// <returns>обработанный URL</returns>
		public static string RemoveLastSlash(string url)
		{
			string result = PathUtility.RemoveLastSlash(url);

			return result;
		}

		/// <summary>
		/// Возвращает URL без строки запроса
		/// </summary>
		/// <param name="url">URL</param>
		/// <returns>обработанный URL</returns>
		public static string RemoveQueryString(string url)
		{
			string result = url;

			int queryStringPosition = url.IndexOf("?");
			if (queryStringPosition != -1)
			{
				result = url.Remove(queryStringPosition);
			}

			return result;
		}

		public static bool IsQueryEmpty(string url)
		{
			return url.IndexOf("?") < 0;
		}

		/// <summary>
		/// Соединяет базовый и относительный путь
		/// </summary>
		/// <param name="basePath">базовый путь</param>
		/// <param name="relativePath">относительный путь</param>
		/// <returns>URL</returns>
		public static string Combine(string basePath, string relativePath)
		{
			string result = PathUtility.Combine(basePath, relativePath);

			return result;
		}

		/// <summary>
		/// Соеденияет пути
		/// </summary>
		/// <param name="paths">пути</param>
		/// <returns>URL</returns>
		public static string Combine(params string[] paths)
		{
			string result = PathUtility.Combine(paths);

			return result;
		}
	}
}