using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Quantumart.QP8.Utils.FullTextSearch
{
	/// <summary>
	/// Список стоп-слов
	/// </summary>
	public interface IStopWordList
	{
		/// <summary>
		/// Содержит ли список слово
		/// </summary>
		/// <param name="word">слово</param>
		/// <returns></returns>
		bool ContainsWord(string word);
	}
}
