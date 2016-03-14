using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Quantumart.QP8.BLL.Repository;
using System.Data;
using Quantumart.QP8.Utils;

namespace Quantumart.QP8.BLL
{
	/// <summary>
	/// Переводчик
	/// </summary>
	public class Translator
	{
		public static string Translate(string phrase)
		{
			if (!String.IsNullOrWhiteSpace(phrase) && QPContext.CurrentUserIdentity != null)
			{
				var dictionaries = _dictionaries.Value;
				var keyPhrase = phrase.ToLowerInvariant();
				if (dictionaries.ContainsKey(keyPhrase))
				{
					if (dictionaries[keyPhrase].ContainsKey(QPContext.CurrentUserIdentity.LanguageId))
					{
						return dictionaries[keyPhrase][QPContext.CurrentUserIdentity.LanguageId];
					}
					else
						return phrase;
				}
				else
					return phrase;
			}
			return phrase;
		}

		private static Lazy<Dictionary<string, Dictionary<int, string>>> _dictionaries = new Lazy<Dictionary<string,Dictionary<int,string>>>(LoadDictionaries, true);

		private static Dictionary<string, Dictionary<int, string>> LoadDictionaries()
		{			
			return EntityObjectRepository.GetTranslations()
				.Select(r => new 
				{
					PhraseId = Converter.ToInt32(r.Field<decimal>("PHRASE_ID")),
					Phrase = r.Field<string>("PHRASE_TEXT"),
					LanguageId = Converter.ToInt32(r.Field<decimal>("LANGUAGE_ID")),
					Translation = r.Field<string>("PHRASE_TRANSLATION")
				})			
				.GroupBy(d => d.Phrase.ToLowerInvariant())
				.ToDictionary(g => g.Key, g => 
					g.ToDictionary(r => 
						r.LanguageId, r => r.Translation
					)
				);			
		}
	}
}
