using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace QP8.Infrastructure.Web.AspNet.Helpers
{
    public class UrlHelpers
    {
        private static HttpContext HttpContext => new HttpContextAccessor().HttpContext;

        public static string ConvertToAbsoluteUrl(string url)
        {
            if (Web.Helpers.UrlHelpers.IsRelativeUrl(url))
            {
                url = url.Trim();
                if (url.StartsWith("~/"))
                {
                    var hostingEnvironment = HttpContext.RequestServices.GetRequiredService<IHostingEnvironment>();

                    url = hostingEnvironment.ContentRootPath + url.Substring(1);
                }

                var baseUri = new Uri(GetBaseUrl());
                return new Uri(baseUri, url).ToString();
            }

            return url;
        }

        public static string GetBaseUrl()
        {
            Ensure.NotNull(HttpContext, "HttpContext not exists here");
            return GetBaseUrl(HttpContext.Request);
        }

        public static string GetBaseUrl(HttpContext context)
        {
            Ensure.Argument.NotNull(context, "HttpContext not exists here");
            return GetBaseUrl(context.Request);
        }

        public static string GetBaseUrl(HttpRequest request)
        {
            Ensure.Argument.NotNull(request, "HttpRequest not exists here");
            return new Uri(UriHelper.GetEncodedUrl(request)).GetLeftPart(UriPartial.Path);
        }
    }
}
