using System;
using System.Text;

namespace Quantumart.QP8.Utils
{
	public class Typographer
	{
		/// <summary>
		/// Обрезает строку до указанного количества символов
		/// </summary>
		/// <param name="value">значение</param>
		/// <param name="maxLength">максимальная длина строки</param>
		/// <returns>обрезанная строка</returns>
		public static string CutShort(string value, int maxLength)
		{
			return CutShort(value, maxLength, "…");
		}

		/// <summary>
		/// Обрезает строку до указанного количества символов
		/// </summary>
		/// <param name="value">значение</param>
		/// <param name="maxLength">максимальная длина строки</param>
		/// <param name="endSymbol">символ, который ставится в конце обрезанной строки</param>
		/// <returns>обрезанная строка</returns>
		public static string CutShort(string value, int maxLength, string endSymbol)
		{
			string result = value.Trim();

			if (result.Length > maxLength)
			{
				result = result.Substring(0, maxLength - 1).Trim() + endSymbol;
			}

			return result;
		}

		/// <summary>
		/// Обрезает строку до указанного количества символов, сохраняя целостность слов
		/// </summary>
		/// <param name="value">значение</param>
		/// <param name="maxLength">максимальная длина строки</param>
		/// <returns>обрезанная строка</returns>
		public static string CutShortByWords(string value, int maxLength)
		{
			return CutShortByWords(value, maxLength, "…");
		}

		/// <summary>
		/// Обрезает строку до указанного количества символов, сохраняя целостность слов
		/// </summary>
		/// <param name="value">значение</param>
		/// <param name="maxLength">максимальная длина строки</param>
		/// <param name="endSymbol">символ, который ставится в конце обрезанной строки</param>
		/// <returns>обрезанная строка</returns>
		public static string CutShortByWords(string value, int maxLength, string endSymbol)
		{
			return CutShortByWords(value, maxLength, endSymbol, false);
		}

		/// <summary>
		/// Обрезает строку до указанного количества символов, сохраняя целостность слов
		/// </summary>
		/// <param name="value">значение</param>
		/// <param name="maxLength">максимальная длина строки</param>
		/// <param name="endSymbol">символ, который ставится в конце обрезанной строки</param>
		/// <param name="isAfterCut">признак, разрешающий обрезать строку после 
		/// слова, превысившего максимальную длину</param>
		/// <returns>обрезанная строка</returns>
		public static string CutShortByWords(string value, int maxLength, string endSymbol, bool isAfterCut)
		{
			string result = "";
			string processedValue = value.Trim();

			if (processedValue.Length > maxLength)
			{
				if (processedValue.IndexOf(" ") != -1)
				{
					// Заменяем все переводы строк на пробелы
					// и удаляем дублирующиеся пробелы
					processedValue = Cleaner.RemoveNewLines(processedValue);

					// Получаем список слов
					string[] wordList = Converter.ToStringCollection(processedValue, ' ');
					string word = "";

					result = "";

					for (int i = 0; i < wordList.Length; i++)
					{
						word = wordList[i];

						if (!isAfterCut)
						{
							if ((result + " " + word).Length > maxLength - 1)
							{
								break;
							}
						}

						if (result.Length > 0)
						{
							result += " " + word;
						}
						else
						{
							result += word;
						}

						if (isAfterCut)
						{
							if (result.Length > maxLength - 1)
							{
								break;
							}
						}
					}

					wordList = null;

					result += endSymbol;
				}
				else
				{
					result = CutShort(processedValue, maxLength);
				}
			}
			else
			{
				result = processedValue;
			}

			return result;
		}
	}
}
