using System;
using System.Web.Mvc;

namespace Quantumart.QP8.WebMvc.Infrastructure.ActionResults
{
    public class JsonpResult : JsonResult
    {
        public override void ExecuteResult(ControllerContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var request = context.HttpContext.Request;
            var response = context.HttpContext.Response;
            var jsoncallback = context.RouteData.Values["callback"] as string ?? request["callback"];
            if (string.IsNullOrEmpty(jsoncallback))
            {
                jsoncallback = context.RouteData.Values["jsoncallback"] as string ?? request["jsoncallback"];
            }

            if (!string.IsNullOrEmpty(jsoncallback))
            {
                if (string.IsNullOrEmpty(ContentType))
                {
                    ContentType = "application/x-javascript";
                }

                response.Write($"{jsoncallback}(");
            }

            base.ExecuteResult(context);
            if (!string.IsNullOrEmpty(jsoncallback))
            {
                response.Write(")");
            }
        }
    }
}
