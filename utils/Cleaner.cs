using System.Text.RegularExpressions;

namespace Quantumart.QP8.Utils
{
    public static class Cleaner
    {
        private static readonly Regex EndBlockTagsRegExp = new Regex(@"((<br\s*\/?>)|<\/h(1-7)>)|(<\/p>)|(<\/li>)|(<\/td>)", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex AllTagsRegExp = new Regex(@"<[^>]*>", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex BreakTagRegExp = new Regex(@"<br\s*\/?>", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        /// <summary>
        /// Удаляет тэги из HTML-строки
        /// </summary>
        public static string RemoveAllHtmlTags(string value, bool replaceToXmlEntities = true)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                var result = value.Trim();
                if (result.Length > 0)
                {
                    // Заменяем тэги BR, P, LI и TD на переводы строк
                    result = EndBlockTagsRegExp.Replace(result, "\n");
                    if (replaceToXmlEntities)
                    {
                        result = result.Replace("<", "&lt;");
                        result = result.Replace(">", "&gt;");
                    }
                    else
                    {
                        result = AllTagsRegExp.Replace(result, "");
                    }
                }

                return result.Trim();
            }
            else
                return string.Empty;
        }

        /// <summary>
        /// Удаляет тэги и пробелы из HTML-строки
        /// </summary>
        public static string RemoveAllHtmlTagsAndSpaces(string value)
        {
            var result = RemoveAllHtmlTags(value);
            if (result.Length > 0)
            {
                result = result.Replace("&nbsp;", "");
                result = RemoveNewLines(result);
                result = result.Trim();
            }

            return result;
        }

        /// <summary>
        /// Удаляет тэги BR и заменяет их на переводы строк
        /// </summary>
        public static string RemoveBreaks(string value)
        {
            var result = value.Trim();
            if (result.Length > 0)
            {
                result = BreakTagRegExp.Replace(result, "\n");
            }

            return result;
        }

        /// <summary>
        /// Удаляет переводы строк и заменяет их на пробелы
        /// </summary>
        public static string RemoveNewLines(string value)
        {
            var result = value.Trim();
            if (result.Length > 0)
            {
                result = result.Replace("\n", " ");
                result = result.Replace("  ", " ");
            }

            return result;
        }

        /// <summary>
        /// Эскейпит спец.символы в строке-параметре условия LIKE
        /// </summary>
        public static string ToSafeSqlLikeCondition(string condition)
        {
            return string.IsNullOrEmpty(condition) ? condition : condition.Replace("'", "''").Replace("[", "[[]").Replace("%", "[%]").Replace("_", "[_]");
        }

        /// <summary>
        /// Эскейпит спец.символы в строке
        /// </summary>
        public static string ToSafeSqlString(string str)
        {
            return string.IsNullOrEmpty(str) ? str : str.Replace("'", "''");
        }
    }
}
