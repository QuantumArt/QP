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
	public class ResponseHeaderAttribute : ActionFilterAttribute, IActionFilter
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
			if (String.IsNullOrEmpty(filterContext.HttpContext.Response.Headers[Name]))
				filterContext.HttpContext.Response.Headers.Add(Name, Value);
		}		
	}
}