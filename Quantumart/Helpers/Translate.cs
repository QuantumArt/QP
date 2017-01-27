using System.Web;
using Quantumart.QPublishing.Database;

namespace Quantumart.QPublishing.Helpers
{
    public class TranslateManager
    {
        public static string Translate(string phrase)
        {
            return Translate(phrase, false);
        }

        public static string Translate(string phrase, bool forJavaScript)
        {
            return HttpContext.Current.Session["CurrentLanguageID"].ToString() == "1"
                ? phrase
                : GetTranslation(int.Parse(GetPhraseId(phrase)), (int)HttpContext.Current.Session["CurrentLanguageID"], phrase);
        }

        public static string ReplaceForJavaScript(string input, bool forJavaScript)
        {
            return forJavaScript ? input.Replace("\"", "\\\"").Replace("'", "\\'") : input;
        }

        public static string GetPhraseId(string phrase)
        {

            var conn = new DBConnector();
            var dt = conn.GetCachedData($"select * from phrases where phrase_text = '{phrase.Replace("'", "''")}'");
            return dt.Rows.Count == 0 ? "0" : dt.Rows[0]["phraseId"].ToString();
        }

        public static string GetTranslation(int phraseId, int languageId, string phraseText)
        {
            var conn = new DBConnector();
            if (phraseId == 0)
            {
                return phraseText;
            }

            var dt = conn.GetCachedData($"select * from translations where phrase_id = {phraseId} and language_id = {languageId}");
            return dt.Rows.Count == 0 ? phraseText : dt.Rows[0]["phrase_translation"].ToString();
        }
    }
}
