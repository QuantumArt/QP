using System;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Quantumart.QP8.WebMvc.Infrastructure.ActionFilters
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class RequestHeaderAttribute : ActionFilterAttribute
    {
        public string Name { get; set; }

        public string Value { get; set; }

        public RequestHeaderAttribute(string name, string value)
        {
            Name = name;
            Value = value;
        }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);
            if (string.IsNullOrEmpty(filterContext.HttpContext.Request.Headers[Name]))
            {
                filterContext.HttpContext.Request.Headers.Add(Name, Value);
            }
        }
    }
}
