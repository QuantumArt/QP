using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace Quantumart.QP8.Utils.FullTextSearch
{
	/// <summary>
	/// Находит и выделяет в найденном тексте наиболее релевантные участки соответствующие условиям поиска
	/// </summary>
	public class FoundTextMarker
	{
		public static string GetRelevantMarkedText(string text, IEnumerable<string> wordForms, int size, string startTag, string endTag)
		{
			if (String.IsNullOrEmpty(text))
				return text;

			// Получить позиции словоформ
			var positionsDict = FoundTextMarker.FindWordFormPositionDictionary(text, wordForms);

			// вычислить saturation
			int saturation = FoundTextMarker.CalcSaturation(positionsDict.Keys.ToArray());
			
			// вычислить границы наиболее релевантного диапазона
			var range = FoundTextMarker.GetRange(text, positionsDict, saturation, size);
			int startPos = range.Item1;
			int endPos = range.Item2;
			// получить релевантный диапазон
			string relevantText = text.Substring(startPos, endPos - startPos);
			
			// определить подмножество позиций словоформ внутри диапазона
			var positionsInInterval = positionsDict.Keys.Where(n => (n >= startPos && n <= endPos)).OrderBy(n=>n).ToArray();
			// обернуть словоформы тегами
			int extraLength = startTag.Length + endTag.Length;
			StringBuilder sb = new StringBuilder(relevantText);
			int[] relativePositions = positionsInInterval.Select(n => n - startPos).ToArray();
			for (int i = 0; i < relativePositions.Length; i++)
			{
				int position = relativePositions[i] + i * extraLength;
				string wordForm = positionsDict[positionsInInterval[i]];
				sb.Insert(position, startTag);
				int endPosition = position + wordForm.Length + startTag.Length;
				if (endPosition >= sb.Length)
					sb.Append(endTag);
				else
					sb.Insert(endPosition, endTag);
			}
			return sb.ToString();			
		}

		/// <summary>
		/// Находит и выделяет в найденном тексте наиболее релевантные участки соответствующие условиям поиска - более простой случай без словоформ
		/// </summary>
		public static string GetSimpleRelevantMarkedText(string text, string filter, int size, string startTag, string endTag)
		{
			// Получить позиции словоформ
			var positionsDict = FoundTextMarker.FindMatchPositionDictionary(text, filter);

			// вычислить saturation
			int saturation = FoundTextMarker.CalcSaturation(positionsDict.Keys.ToArray());

			// вычислить границы наиболее релевантного диапазона
			var range = FoundTextMarker.GetRange(text, positionsDict, saturation, size);
			int startPos = range.Item1;
			int endPos = range.Item2;
			// получить релевантный диапазон
			string txt = text.Substring(startPos, endPos - startPos);			
			var positionsInInterval = positionsDict.Keys.Where(n => (n >= startPos && n <= endPos)).OrderBy(n => n).ToArray();
			// обернуть словоформы тегами
			int extraLength = startTag.Length + endTag.Length;
			StringBuilder sb = new StringBuilder(txt);
			int[] relativePositions = positionsInInterval.Select(n => n - startPos).ToArray();
			for (int i = 0; i < relativePositions.Length; i++)
			{
				int position = relativePositions[i] + i * extraLength;
				string wordForm = positionsDict[positionsInInterval[i]];
				sb.Insert(position, startTag);
				int endPosition = position + wordForm.Length + startTag.Length;
				if (endPosition >= sb.Length)
					sb.Append(endTag);
				else
					sb.Insert(endPosition, endTag);
			}
			return sb.ToString();
		}

		/// <summary>
		/// Получить позиции словоформ в тексте
		/// </summary>
		/// <param name="text"></param>
		/// <param name="wordForms"></param>
		/// <returns></returns>
		private static IDictionary<int, string> FindWordFormPositionDictionary(string text, IEnumerable<string> wordForms)
		{			
			Dictionary<int, string> positions = new Dictionary<int, string>();
			foreach (var wordForm in wordForms)
			{
				int start = 0;
				int position = 0;
				while (position != -1)
				{
					position = text.ToLower().IndexOf(wordForm.ToLower(), start);
					if (position >= 0)
					{
						if ((position == 0 || IsBreak(text[position - 1])) && (position + wordForm.Length == text.Length || IsBreak(text[position + wordForm.Length])))
						{
							/* условие добавлено в связи с возникающими ошибками о добавлении уже имеющихся ключей */
							if (!positions.ContainsKey(position))
								positions.Add(position, wordForm);
						}
						start = position + 1;
					}
				}
			}
			var result = new Dictionary<int, string>();
			foreach (var k in positions.Keys.OrderBy(k => k))
			{
				result.Add(k, positions[k]);
			}
			return result;
		}

		/// <summary>
		/// Получить позиции строки в тексте
		/// </summary>
		/// <param name="text"></param>
		/// <param name="wordForms"></param>
		/// <returns></returns>
		private static IDictionary<int, string> FindMatchPositionDictionary(string text, string matchString)
		{
			Dictionary<int, string> positions = new Dictionary<int, string>();

			int start = 0;
			int position = 0;
			while (position != -1)
			{
				position = text.ToLower().IndexOf(matchString.ToLower(), start);
				if (position >= 0)
				{		//(position == 0 || IsBreak(text[position - 1])) && (position + wordForm.Length == text.Length || IsBreak(text[position + wordForm.Length]))										
					/* условие добавлено в связи с возникающими ошибками о добавлении уже имеющихся ключей */
					if (!positions.ContainsKey(position))
						positions.Add(position, matchString);					
					start = position + 1;
				}
			}
			
			var result = new Dictionary<int, string>();
			foreach (var k in positions.Keys.OrderBy(k => k))
			{
				result.Add(k, positions[k]);
			}
			return result;
		}

		/// <summary>
		/// Посчитать Saturation
		/// </summary>
		/// <param name="position"></param>
		/// <returns></returns>
		private static int CalcSaturation(int[] position)
		{
			if (position.Length == 0)
				return 0;
			var orderedPosition = position.OrderBy(p => p).ToArray();
			int[] saturations = new int[orderedPosition.Length];
			for (var i = 0; i < saturations.Length; i++)
			{
				saturations[i] = orderedPosition.Select(n => Math.Abs(n - orderedPosition[i])).Sum();
			}			
			int minSaturation = saturations.Min();
			int index = saturations.Select((n, i) => new { Index = i, Saturation = n }).Where(n => n.Saturation == minSaturation).Select(n => n.Index).First();
			return orderedPosition[index];
		}

		/// <summary>
		/// Определить какой диапазон будем показывать
		/// </summary>
		/// <param name="size"></param>
		/// <returns></returns>
		private static Tuple<int, int> GetRange(string text, IDictionary<int, string> wfPos, int saturation, int size)
		{
			int start, end;
			if (size == 0)
			{
				start = 0;
				end = text.Length;
			}
			else
			{
				if (wfPos.Count == 0)
				{
					start = 0;
					end = start + size;
				}
				else
				{
					start = saturation - size;
					end = saturation + wfPos[saturation].Length + size;
				}

				if (start < 0)
					start = 0;

				if (end > text.Length)
					end = text.Length;

				while (start > 0)
				{
					if (IsBreak(text[start - 1]))
						break;
					start--;
				}
				while (end < text.Length - 1)
				{
					if (IsBreak(text[end]))
						break;
					end++;
				}
			}

			return Tuple.Create(start, end);
		}

		private static bool IsBreak(char c)
		{
			int code = Convert.ToInt16(c); 			
			return (code < 48 || code > 058 && code < 64 || code > 90 && code < 97 || c > 122 && c < 192);
		}

		public static IEnumerable<string> SplitIntoWords(string searchString)
		{
			if (String.IsNullOrWhiteSpace(searchString))
				return Enumerable.Empty<string>();
			var delimiterString = @" ,.:;~!@#$%^&*(){}\/[]<>|'?؟-_+،""=";
			var words = searchString
				.Split(delimiterString.ToCharArray(), StringSplitOptions.RemoveEmptyEntries)
				.Distinct();
			return words;
		}

	}
}
