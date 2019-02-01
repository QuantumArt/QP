using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Quantumart.QP8.BLL.Repository;
using Quantumart.QP8.Utils;

namespace Quantumart.QP8.BLL
{
    public class Translator
    {
        public static string Translate(string phrase)
        {
#if !NET_STANDARD
            if (!string.IsNullOrWhiteSpace(phrase) && QPContext.CurrentUserIdentity != null)
            {
                var dictionaries = Dictionaries.Value;
                var keyPhrase = phrase.ToLowerInvariant();
                if (dictionaries.ContainsKey(keyPhrase))
                {
                    return dictionaries[keyPhrase].ContainsKey(QPContext.CurrentUserIdentity.LanguageId)
                        ? dictionaries[keyPhrase][QPContext.CurrentUserIdentity.LanguageId]
                        : phrase;
                }

                return phrase;
            }
#endif
            return phrase;
        }

        private static readonly Lazy<Dictionary<string, Dictionary<int, string>>> Dictionaries = new Lazy<Dictionary<string, Dictionary<int, string>>>(LoadDictionaries, true);

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
                .ToDictionary(g => g.Key, g => g.ToDictionary(r => r.LanguageId, r => r.Translation));
        }
    }
}
