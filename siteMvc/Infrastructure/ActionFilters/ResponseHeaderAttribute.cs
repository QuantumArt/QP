using System;
using System.Web.Mvc;

namespace Quantumart.QP8.WebMvc.Infrastructure.ActionFilters
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class ResponseHeaderAttribute : ActionFilterAttribute
    {
        public string Name { get; set; }

        public string Value { get; set; }

        public ResponseHeaderAttribute(string name, string value)
        {
            Name = name;
            Value = value;
        }

        public override void OnResultExecuted(ResultExecutedContext filterContext)
        {
            base.OnResultExecuted(filterContext);
            if (string.IsNullOrEmpty(filterContext.HttpContext.Response.Headers[Name]))
            {
                filterContext.HttpContext.Response.Headers.Add(Name, Value);
            }
        }
    }
}
