using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.Web.Mvc;

namespace Quantumart.QP8.WebMvc.Extensions.ActionResults
{
    public class JsonNetResult<T> : JsonResult
    {
        private readonly JsonSerializerSettings _settings;
        private Formatting _jsonFormatting = Formatting.None;

        public JsonNetResult(T data)
            : this(data, new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore })
        {
        }

        public JsonNetResult(T data, JsonSerializerSettings settings)
        {
            Data = data;
            _settings = settings;
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
            response.Write(SerializeToJson(_settings));
        }

        public static implicit operator JsonNetResult<T>(T data)
        {
            return new JsonNetResult<T>(data);
        }

        protected virtual string SerializeToJson(JsonSerializerSettings settings)
        {
            SetIndentedFormattingIfDebug();
            return JsonConvert.SerializeObject(Data, _jsonFormatting, settings);
        }

        [Conditional("DEBUG")]
        private void SetIndentedFormattingIfDebug()
        {
            _jsonFormatting = Formatting.Indented;
        }
    }
}
