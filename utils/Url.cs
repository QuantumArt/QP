using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using System.Text.RegularExpressions;

namespace Quantumart.QP8.Utils
{
    public static class Url
    {
        private static readonly Regex UrlFormatRegExp = new Regex(@"^[a-zA-Z0-9\+\.-]+:");

        private static HttpContext HttpContext => new HttpContextAccessor().HttpContext;

        public static bool CheckUrlFormatIsValid(string url)
        {
            return UrlFormatRegExp.IsMatch(url);
        }

        /// <summary>
        /// Преобразуется относительный URL в абсолютный (с точки зрения приложения) [без домена]
        /// </summary>
        public static string ToAbsolute(string url)
        {
            var result = string.Empty;
            if (url != null)
            {
                url = url.Trim();
                if (url.Length > 0)
                {
                    if (!CheckUrlFormatIsValid(url) && HttpContext != null && url.StartsWith("~/"))
                    {
                        var urlHelper = HttpContext.RequestServices.GetRequiredService<IUrlHelper>();

                        result = urlHelper.Content(url);
                    }
                    else
                    {
                        result = url;
                    }
                }
            }

            return result;
        }

        public static bool IsQueryEmpty(string url)
        {
            return url.IndexOf("?") < 0;
        }
    }
}
