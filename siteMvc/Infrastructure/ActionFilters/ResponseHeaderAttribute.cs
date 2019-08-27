using System;
using Microsoft.AspNetCore.Mvc.Filters;

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

        public override void OnResultExecuting(ResultExecutingContext context)
        {

            if (string.IsNullOrEmpty(context.HttpContext.Response.Headers[Name]))
            {
                context.HttpContext.Response.Headers.Add(Name, Value);
            }
            base.OnResultExecuting(context);
        }
    }
}
