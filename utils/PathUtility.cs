using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace Quantumart.QP8.Utils
{
    public static class PathUtility
    {
        private static readonly Regex LastSlashRegExp = new Regex(@"[\/\\]$");
        private static readonly Regex FirstSlashRegExp = new Regex(@"^[\/\\]");

        /// <summary>
        /// Исправляет слэши
        /// </summary>
		public static string CorrectSlashes(string path, CorrectSlashMode mode)
        {
            var result = path;

            if (mode.HasFlag(CorrectSlashMode.ReplaceDoubleSlashes))
            {
                result = ReplaceDoubleSlashes(result);
            }

            if (mode.HasFlag(CorrectSlashMode.WrapToSlashes))
            {
                result = WrapToSlashes(result);
            }

            if (mode.HasFlag(CorrectSlashMode.RemoveLastSlash))
            {
                result = RemoveLastSlash(result);
            }

            if (mode.HasFlag(CorrectSlashMode.RemoveFirstSlash))
            {
                result = RemoveFirstSlash(result);
            }

            if (mode.HasFlag(CorrectSlashMode.ConvertBackSlashesToSlashes))
            {
                result = ConvertBackSlashesToSlashes(result);
            }

            if (mode.HasFlag(CorrectSlashMode.ConvertSlashesToBackSlashes))
            {
                result = ConvertSlashesToBackSlashes(result);
            }

            return result;
        }

        /// <summary>
        /// Возвращает путь без первого слэша
        /// </summary>
        public static string RemoveFirstSlash(string path)
        {
            var result = path;
            if (!string.IsNullOrEmpty(result))
            {
                result = FirstSlashRegExp.Replace(result, string.Empty);
            }

            return result;
        }

        /// <summary>
        /// Возвращает путь без последнего слэша
        /// </summary>
        public static string RemoveLastSlash(string path)
        {
            var result = path;
            if (!string.IsNullOrEmpty(result))
            {
                result = LastSlashRegExp.Replace(result, string.Empty);
            }

            return result;
        }

        /// <summary>
        /// Преобразует обычные слэши в обратные
        /// </summary>
        public static string ConvertSlashesToBackSlashes(string path)
        {
            var result = path;
            if (!string.IsNullOrEmpty(result) && RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                result = result.Replace(@"/", @"\");
            }

            return result;
        }

        /// <summary>
        /// Обертывает строку в слэши
        /// </summary>
        public static string WrapToSlashes(string path)
        {
            var result = path;
            if (!string.IsNullOrEmpty(result))
            {
                if (!FirstSlashRegExp.IsMatch(result))
                {
                    result = @"/" + result;
                }

                if (!LastSlashRegExp.IsMatch(result))
                {
                    result = result + @"/";
                }
            }

            return result;
        }

        /// <summary>
        /// Заменяет двойные слэши одинарными
        /// </summary>
        public static string ReplaceDoubleSlashes(string path)
        {
            var result = path;
            if (!string.IsNullOrEmpty(result))
            {
                result = result.Replace(@"\\", @"\");
                result = result.Replace(@"//", @"/");
            }
            return result;
        }

        /// <summary>
        /// Преобразует обратные слэши в обычные
        /// </summary>
        public static string ConvertBackSlashesToSlashes(string path)
        {
            var result = path;
            if (!string.IsNullOrEmpty(result))
            {
                result = result.Replace(@"\", @"/");
            }

            return result;
        }

        /// <summary>
        /// Соединяет базовый и относительный путь
        /// </summary>
        public static string Combine(string basePath, string relativePath)
        {
            string result;
            var processedBasePath = Converter.ToString(basePath).Trim();
            var processedRelativePath = Converter.ToString(relativePath).Trim();

            if (processedBasePath.Length > 0 && processedRelativePath.Length > 0)
            {
                result = RemoveLastSlash(basePath) + @"/" + RemoveFirstSlash(relativePath);
            }
            else
            {
                result = processedBasePath.Length > 0 ? processedBasePath : processedRelativePath;
            }

            return result;
        }

        /// <summary>
        /// Соеденияет пути
        /// </summary>
        public static string Combine(params string[] paths)
        {
            return paths.Aggregate(string.Empty, Combine);
        }
    }
}
