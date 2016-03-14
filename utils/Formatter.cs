using System;
using System.Text;
using System.Text.RegularExpressions;

namespace Quantumart.QP8.Utils
{
	public static class Formatter
	{
		/// <summary>
		/// Применяет к строке стиль UPPERCASE
		/// </summary>
		/// <param name="value">строка</param>
		/// <returns>стилизованная строка</returns>
		public static string ToUppercaseStyle(string value)
		{
			string result = String.Empty;
			string processedValue = Converter.ToString(value).Trim();

			if (processedValue.Length > 0)
			{
				char[] symbolList = processedValue.ToCharArray();
				int symbolCount = symbolList.Length;

				for (int symbolIndex = 0; symbolIndex < symbolCount; symbolIndex++)
				{
					char symbol = symbolList[symbolIndex];
					if (char.IsUpper(symbol))
					{
						if (symbolIndex != 0 && symbolIndex != symbolCount - 1)
						{
							result += "_";
						}
					}
					result += char.ToUpper(symbol);
				}

				symbolList = null;
			}

			return result;
		}

        public static string ProtectHtml(string text)
        {
            string result = text;
            if (!String.IsNullOrEmpty(text))
            {
                RegexOptions options = RegexOptions.Compiled | RegexOptions.Multiline | RegexOptions.IgnoreCase;
                result = Regex.Replace(result, "<", "&lt;", options);
                result = Regex.Replace(result, ">", "&gt;", options);
                result = Regex.Replace(result, "&nbsp;", "&amp;nbsp;", options);
                result = Regex.Replace(result, "  ", "&nbsp;&nbsp;", options);
                result = Regex.Replace(result, "\t", "&nbsp;&nbsp;&nbsp;&nbsp;", options);
                result = Regex.Replace(result, "\n", "<br>", options);
            }
            return result;
        }
	}
}
