using System;
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

        // Преобразуется относительный URL в абсолютный (с точки зрения приложения)
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

        // Преобразует относительный URL в полный URL (с доменом сайта)
        public static string ToFull(string url)
        {
            var result = string.Empty;
            if (url != null)
            {
                url = url.Trim();
                if (url.Length > 0)
                {
                    if (!CheckUrlFormatIsValid(url) && HttpContext.Current != null)
                    {
                        if (url.StartsWith("~/"))
                        {
                            url = VirtualPathUtility.ToAbsolute(url);
                        }

                        var baseUri = new Uri(HttpContext.Current.Request.Url.GetLeftPart(UriPartial.Path));
                        result = new Uri(baseUri, url).ToString();
                    }
                    else
                    {
                        result = url;
                    }
                }
            }

            return result;
        }

        public static string RemoveFirstSlash(string url)
        {
            return PathUtility.RemoveFirstSlash(url);
        }

        public static string RemoveLastSlash(string url)
        {
            return PathUtility.RemoveLastSlash(url);
        }

        public static string RemoveQueryString(string url)
        {
            var result = url;
            var queryStringPosition = url.IndexOf("?");
            if (queryStringPosition != -1)
            {
                result = url.Remove(queryStringPosition);
            }

            return result;
        }

        public static bool IsQueryEmpty(string url)
        {
            return url.IndexOf("?") < 0;
        }

        public static string Combine(string basePath, string relativePath)
        {
            return PathUtility.Combine(basePath, relativePath);
        }

        public static string Combine(params string[] paths)
        {
            return PathUtility.Combine(paths);
        }
    }
}
