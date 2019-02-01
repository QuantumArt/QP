using System.Text.RegularExpressions;
using System.Web;

namespace Quantumart.QP8.Utils
{
    public static class Url
    {
        private static readonly Regex UrlFormatRegExp = new Regex(@"^[a-zA-Z0-9\+\.-]+:");

        public static bool CheckUrlFormatIsValid(string url)
        {
            return UrlFormatRegExp.IsMatch(url);
        }

        #if !NET_STANDARD
        // Преобразуется относительный URL в абсолютный (с точки зрения приложения) [без домена]
        public static string ToAbsolute(string url)
        {
            var result = string.Empty;
            if (url != null)
            {
                url = url.Trim();
                if (url.Length > 0)
                {
                    if (!CheckUrlFormatIsValid(url) && HttpContext.Current != null && url.StartsWith("~/"))
                    {
                        result = VirtualPathUtility.ToAbsolute(url);
                    }
                    else
                    {
                        result = url;
                    }
                }
            }

            return result;
        }
        #endif

        public static bool IsQueryEmpty(string url)
        {
            return url.IndexOf("?") < 0;
        }
    }
}
