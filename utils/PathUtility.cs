using System;
using System.Text.RegularExpressions;
using System.IO;

namespace Quantumart.QP8.Utils
{
	public static class PathUtility
	{
		private static Regex _lastSlashRegExp = new Regex(@"[\/\\]$");
		private static Regex _firstSlashRegExp = new Regex(@"^[\/\\]");

		
		public static string CorrectSlashes(string path, CorrectSlashMode mode)
		{
			string result = path;

			if (mode.HasFlag(CorrectSlashMode.ReplaceDoubleSlashes))
				result = ReplaceDoubleSlashes(result);

			if (mode.HasFlag(CorrectSlashMode.WrapToSlashes))
				result = WrapToSlashes(result);

			if (mode.HasFlag(CorrectSlashMode.RemoveLastSlash))
				result = RemoveLastSlash(result);

			if (mode.HasFlag(CorrectSlashMode.RemoveFirstSlash))
				result = RemoveFirstSlash(result);

			if (mode.HasFlag(CorrectSlashMode.ConvertBackSlashesToSlashes))
				result = ConvertBackSlashesToSlashes(result);

			if (mode.HasFlag(CorrectSlashMode.ConvertSlashesToBackSlashes))
				result = ConvertSlashesToBackSlashes(result);

			return result;
		}
		
		/// <summary>
		/// Возвращает путь без первого слэша
		/// </summary>
		/// <param name="url">путь</param>
		/// <returns>обработанный путь</returns>
		public static string RemoveFirstSlash(string path)
		{
			string result = path;
			if (!String.IsNullOrEmpty(result))
			{
				result = _firstSlashRegExp.Replace(result, "");
			}

			return result;
		}

		/// <summary>
		/// Возвращает путь без последнего слэша
		/// </summary>
		/// <param name="url">путь</param>
		/// <returns>обработанный путь</returns>
		public static string RemoveLastSlash(string path)
		{
			string result = path;
			if (!String.IsNullOrEmpty(result))
			{
				result = _lastSlashRegExp.Replace(result, "");
			}

			return result;
		}

		/// <summary>
		/// Преобразует обычные слэши в обратные
		/// </summary>
		/// <param name="path">путь</param>
		/// <returns>обработанный путь</returns>
		public static string ConvertSlashesToBackSlashes(string path)
		{
			string result = path;
			if (!String.IsNullOrEmpty(result))
			{
				result = result.Replace(@"/", @"\");
			}

			return result;
		}

		/// <summary>
		/// Обертывает строку в слэши
		/// </summary>
		/// <param name="path">путь</param>
		/// <returns>обработанный путь</returns>
		public static string WrapToSlashes(string path)
		{
			string result = path;
			if (!String.IsNullOrEmpty(result))
			{
				if (!_firstSlashRegExp.IsMatch(result))
					result = @"/" + result;
				if (!_lastSlashRegExp.IsMatch(result))
					result = result + @"/";
			}
			return result;
		}

		/// <summary>
		/// Заменяет двойные слэши одинарными
		/// </summary>
		/// <param name="path">путь</param>
		/// <returns>обработанный путь</returns>
		public static string ReplaceDoubleSlashes(string path)
		{
			string result = path;
			if (!String.IsNullOrEmpty(result))
			{
				result = result.Replace(@"\\", @"\");
				result = result.Replace(@"//", @"/");
			}
			return result;
		}

		/// <summary>
		/// Преобразует обратные слэши в обычные
		/// </summary>
		/// <param name="path">путь</param>
		/// <returns>обработанный путь</returns>
		public static string ConvertBackSlashesToSlashes(string path)
		{
			string result = path;
			if (!String.IsNullOrEmpty(result))
			{
				result = result.Replace(@"\", @"/");
			}

			return result;
		}

		/// <summary>
		/// Соединяет базовый и относительный путь
		/// </summary>
		/// <param name="basePath">базовый путь</param>
		/// <param name="relativePath">относительный путь</param>
		/// <returns>путь</returns>
		public static string Combine(string basePath, string relativePath)
		{
			string result = String.Empty;
			string processedBasePath = Converter.ToString(basePath).Trim();
			string processedRelativePath = Converter.ToString(relativePath).Trim();

			if (processedBasePath.Length > 0 && processedRelativePath.Length > 0)
			{
				result = RemoveLastSlash(basePath) + @"/" + RemoveFirstSlash(relativePath);
			}
			else
			{
				if (processedBasePath.Length > 0)
				{
					result = processedBasePath;
				}
				else
				{
					result = processedRelativePath;
				}
			}

			return result;
		}

		/// <summary>
		/// Соеденияет пути
		/// </summary>
		/// <param name="paths">пути</param>
		/// <returns>путь</returns>
		public static string Combine(params string[] paths)
		{
			string result = string.Empty;

			foreach (string path in paths)
			{
				result = Combine(result, path);
			}

			return result;
		}
	}
}
