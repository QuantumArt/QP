using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Quantumart.QP8.WebMvc.Extensions.ActionFilters
{
	/// <summary>
	/// Добавляет X-Requested-With="XMLHttpRequest" в заголовок запроса
	/// </summary>
	[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
	public class RequestHeaderAttribute : ActionFilterAttribute, IActionFilter
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

			if (String.IsNullOrEmpty(filterContext.HttpContext.Request.Headers[Name]))
				filterContext.HttpContext.Request.Headers.Add(Name, Value);
		}
	}
}