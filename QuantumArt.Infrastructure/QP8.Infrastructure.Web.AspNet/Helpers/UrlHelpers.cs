using System;
using System.Web;

namespace QP8.Infrastructure.Web.AspNet.Helpers
{
    public class UrlHelpers
    {

        public static string ConvertToAbsoluteUrl(string url)
        {
            if (Web.Helpers.UrlHelpers.IsRelativeUrl(url))
            {
                var urlToProcess = url.Trim();
                if (urlToProcess.StartsWith("~/"))
                {
                    urlToProcess = VirtualPathUtility.ToAbsolute(url);
                }

                var baseUri = new Uri(GetBaseUrl());
                return new Uri(baseUri, urlToProcess).ToString();
            }

            return url;
        }

        public static string GetBaseUrl()
        {
            var httpContext = HttpContext.Current;
            Ensure.NotNull(httpContext, "HttpContext not exists here");
            return GetBaseUrl(httpContext.Request);
        }

        public static string GetBaseUrl(HttpContext context)
        {
            Ensure.Argument.NotNull(context, "HttpContext not exists here");
            return GetBaseUrl(context.Request);
        }

        public static string GetBaseUrl(HttpRequest request)
        {
            Ensure.Argument.NotNull(request, "HttpRequest not exists here");
            return request.Url.GetLeftPart(UriPartial.Path);
        }
    }
}
