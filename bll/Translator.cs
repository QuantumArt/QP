using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Xml;
using Quantumart.QP8.BLL.Repository;
using Quantumart.QP8.Resources;
using Quantumart.QP8.Utils;

namespace Quantumart.QP8.BLL
{
    public class Translator
    {
        public static string Translate(string phrase)
        {

            if (!string.IsNullOrWhiteSpace(phrase))
            {
                var dictionaries = Dictionaries.Value;
                var keyPhrase = phrase.ToLowerInvariant();
                if (dictionaries.ContainsKey(keyPhrase))
                {
                    return dictionaries[keyPhrase].ContainsKey(QPContext.CurrentLanguageId)
                        ? dictionaries[keyPhrase][QPContext.CurrentLanguageId]
                        : phrase;
                }

                return phrase;
            }
            return phrase;
        }

        private static readonly Lazy<Dictionary<string, Dictionary<int, string>>> Dictionaries = new Lazy<Dictionary<string, Dictionary<int, string>>>(LoadDictionaries, true);

        private static Dictionary<string, Dictionary<int, string>> LoadDictionaries()
        {
            var resourcesAssembly = typeof(TranslationsResourcesMarker).GetTypeInfo().Assembly;
            var xmlDoc = new XmlDocument();
            using (var stream = resourcesAssembly.GetManifestResourceStream("Quantumart.QP8.Resources.translations.xml"))
            {
                xmlDoc.Load(stream);
            }

            var phrases = xmlDoc.SelectNodes("//phrase");

            var result = new Dictionary<string, Dictionary<int, string>>();

            foreach (XmlNode phraseNode in phrases)
            {
                var keyAttr = (XmlAttribute)phraseNode.Attributes.GetNamedItem("key");
                if (keyAttr != null)
                {
                    var keyPhrase = keyAttr.Value.ToLowerInvariant();
                    var trNodes = phraseNode.SelectNodes("descendant::translation");
                    var translationsDict = new Dictionary<int, string>();
                    foreach (XmlNode trNode in trNodes)
                    {

                        var languageIdAttr = trNode.Attributes.GetNamedItem("languageId");
                        if (languageIdAttr != null)
                        {
                            if (int.TryParse(languageIdAttr.Value, out var langId))
                            {
                                translationsDict[langId] = trNode.InnerText;
                            }
                        }
                    }

                    result[keyPhrase] = translationsDict;

                }
            }

            return result;

            // return EntityObjectRepository.GetTranslations()
            //     .Select(r => new
            //     {
            //         PhraseId = Converter.ToInt32(r.Field<decimal>("PHRASE_ID")),
            //         Phrase = r.Field<string>("PHRASE_TEXT"),
            //         LanguageId = Converter.ToInt32(r.Field<decimal>("LANGUAGE_ID")),
            //         Translation = r.Field<string>("PHRASE_TRANSLATION")
            //     })
            //     .GroupBy(d => d.Phrase.ToLowerInvariant())
            //     .ToDictionary(g => g.Key, g => g.ToDictionary(r => r.LanguageId, r => r.Translation));
        }
    }
}
