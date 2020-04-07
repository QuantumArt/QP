using Microsoft.AspNetCore.Http;
using Quantumart.QP8.Constants.Mvc;

namespace Quantumart.QP8.WebMvc.Infrastructure.Extensions
{
    public static class HttpRequestExtensions
    {
        public static bool IsAjaxRequest(this HttpRequest request)
        {
            IQueryCollection query = request.Query;
            if (query != null)
            {
                if (query[RequestHeaders.XRequestedWith] == "XMLHttpRequest")
                {
                    return true;
                }
            }

            IHeaderDictionary headers = request.Headers;
            if (headers != null)
            {
                if (headers[RequestHeaders.XRequestedWith] == "XMLHttpRequest")
                {
                    return true;
                }
            }
            return false;
        }
    }
}
