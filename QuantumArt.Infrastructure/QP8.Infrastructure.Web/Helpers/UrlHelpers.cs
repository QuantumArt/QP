using System;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace QP8.Infrastructure.Web.Helpers
{
    public class UrlHelpers
    {
        public const string AbsoluteWebFolderUrl = @"^http(s)?://(?:[a-zA-Zа-яА-Я0-9_\-]{1,63}|(?:(?!\d+\.|-)[a-zA-Zа-яА-Я0-9_\-]{1,63}(?<!-)\.)+(?:[a-zA-Zа-яА-Я]{2,}))(:[0-9]{1,5})?(/[a-zA-Z0-9а-яА-Я-_/\.]*)?$";
        public const string RelativeWebFolderUrl = @"^/([a-zA-Z0-9а-яА-Я-_\./]*$)";
        private const string UrlInvalidFormat = "Url has an invalid format";

        public static bool IsValidUrl(string url) => IsValidUrl(url, out Uri _);

        public static bool IsValidUrl(string url, out Uri uriResult)
        {
            var isUrl = Uri.TryCreate(url, UriKind.RelativeOrAbsolute, out uriResult);
            return isUrl && (!uriResult.IsAbsoluteUri || uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
        }

        public static bool IsRelativeUrl(string url) => !IsAbsoluteUrl(url);

        public static bool IsRelativeUrl(string url, out Uri uriResult) => !IsAbsoluteUrl(url, out uriResult);

        public static bool IsAbsoluteUrl(string url) => IsAbsoluteUrl(url, out Uri _);

        public static bool IsAbsoluteUrl(string url, out Uri uriResult)
        {
            Ensure.Argument.NotNull(url);
            Ensure.Argument.Is(IsValidUrl(url, out uriResult), UrlInvalidFormat);
            return uriResult.IsAbsoluteUri;
        }

        public static bool IsValidWebFolderUrl(string url) => IsValidWebFolderUrl(url, out Uri _);

        public static bool IsValidWebFolderUrl(string url, out Uri uriResult) => IsValidUrl(url, out uriResult) && (Regex.IsMatch(url, AbsoluteWebFolderUrl) || Regex.IsMatch(url, RelativeWebFolderUrl));

        public static bool IsRelativeWebFolderUrl(string url) => !IsAbsoluteWebFolderUrl(url);

        public static bool IsRelativeWebFolderUrl(string url, out Uri uriResult) => !IsAbsoluteWebFolderUrl(url, out uriResult);

        public static bool IsAbsoluteWebFolderUrl(string url) => IsAbsoluteWebFolderUrl(url, out Uri _);

        public static bool IsAbsoluteWebFolderUrl(string url, out Uri uriResult)
        {
            Ensure.Argument.NotNull(url);
            Ensure.Argument.Is(IsValidWebFolderUrl(url, out uriResult), UrlInvalidFormat);
            return Regex.IsMatch(url, AbsoluteWebFolderUrl);
        }

        private static HttpContext HttpContext => new HttpContextAccessor().HttpContext;

        public static string ConvertToAbsoluteUrl(string url)
        {
            if (Web.Helpers.UrlHelpers.IsRelativeUrl(url))
            {
                url = url.Trim();
                if (url.StartsWith("~/"))
                {
                    var urlHelper = HttpContext.RequestServices.GetRequiredService<IUrlHelper>();

                    url = urlHelper.Content(url);
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
            return new Uri(request.GetEncodedUrl()).GetLeftPart(UriPartial.Path);
        }

    }
}
