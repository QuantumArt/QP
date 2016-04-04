using Newtonsoft.Json;
using System;
using System.Web.Mvc;

namespace Quantumart.QP8.WebMvc.Extensions.ActionResults
{
    public class JsonNetResult<T> : JsonResult
    {
        public JsonNetResult(T data)
        {
            Data = data;
            JsonRequestBehavior = JsonRequestBehavior.AllowGet;
        }

        public override void ExecuteResult(ControllerContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var response = context.HttpContext.Response;
            response.ContentType = !string.IsNullOrEmpty(ContentType) ? ContentType : "application/json";
            response.ContentEncoding = ContentEncoding ?? response.ContentEncoding;
            response.Write(JsonConvert.SerializeObject(Data, Formatting.None));
        }

        public static implicit operator JsonNetResult<T>(T data)
        {
            return new JsonNetResult<T>(data);
        }
    }
}
