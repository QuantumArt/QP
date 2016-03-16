using System.Collections;
using System.Text;
using System.Text.RegularExpressions;

namespace Quantumart.QPublishing.Helpers
{
    internal class DefaultLinqNameGenerator
    {
        private static Hashtable GetRusEngTranslator()
        {
            var dict = new Hashtable
            {
                {"а", "a"},
                {"б", "b"},
                {"в", "v"},
                {"г", "g"},
                {"д", "d"},
                {"е", "e"},
                {"ё", "e"},
                {"ж", "zh"},
                {"з", "z"},
                {"и", "i"},
                {"й", "y"},
                {"к", "k"},
                {"л", "l"},
                {"м", "m"},
                {"н", "n"},
                {"о", "o"},
                {"п", "p"},
                {"р", "r"},
                {"с", "s"},
                {"т", "t"},
                {"у", "u"},
                {"ф", "f"},
                {"х", "kh"},
                {"ц", "ts"},
                {"ч", "ch"},
                {"ш", "sh"},
                {"щ", "shch"},
                {"ъ", ""},
                {"ы", "y"},
                {"ь", ""},
                {"э", "e"},
                {"ю", "yu"},
                {"я", "ya"},
                {"А", "A"},
                {"Б", "B"},
                {"В", "V"},
                {"Г", "G"},
                {"Д", "D"},
                {"Е", "E"},
                {"Ё", "E"},
                {"Ж", "Zh"},
                {"З", "Z"},
                {"И", "I"},
                {"Й", "Y"},
                {"К", "K"},
                {"Л", "L"},
                {"М", "M"},
                {"Н", "N"},
                {"О", "O"},
                {"П", "P"},
                {"Р", "R"},
                {"С", "S"},
                {"Т", "T"},
                {"У", "U"},
                {"Ф", "F"},
                {"Х", "Kh"},
                {"Ц", "Ts"},
                {"Ч", "Ch"},
                {"Ш", "Sh"},
                {"Щ", "Shch"},
                {"Ъ", ""},
                {"Ы", "Y"},
                {"Ь", ""},
                {"Э", "E"},
                {"Ю", "Yu"},
                {"Я", "Ya"}
            };
            return dict;

        }
        private static string TranslateRusEng(string mappedName)
        {
            var sb = new StringBuilder();
            var dict = GetRusEngTranslator();
            foreach (var c in mappedName.ToCharArray())
            {
                var s = c.ToString();
                var s2 = dict.Contains(s) ? dict[s].ToString() : s;
                sb.Append(s2);
            }
            return sb.ToString();
        }

        private static bool HasRussianChars(string text)
        {
            return Regex.IsMatch(text, @"[а-яА-Я]");
        }

        private static bool IsValidIdentifier(string text)
        {
            return Regex.IsMatch(text, @"^[a-zA-Z][0-9a-zA-Z_]+$");
        }

        private static string GetDefaultName(int id, bool isContent)
        {
            return isContent ? "Content" + id : "Field" + id;
        }

        public static string GetMappedName(string name, int id, bool isContent)
        {
            var mappedName = Regex.Replace(name, @"[\s\._]+", string.Empty);
            if (!IsValidIdentifier(mappedName))
            {
                if (HasRussianChars(mappedName))
                {
                    mappedName = TranslateRusEng(mappedName);
                    if (!IsValidIdentifier(mappedName))
                        mappedName = GetDefaultName(id, isContent);
                }
                else
                {
                    mappedName = GetDefaultName(id, isContent);
                }
            }

            return mappedName;
        }
    }
}
