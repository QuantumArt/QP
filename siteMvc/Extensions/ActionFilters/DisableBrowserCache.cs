using System;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace Quantumart.QP8.WebMvc.Extensions.ActionFilters
{
	/// <summary>
	/// Запрещает броузеру кэшировать результат действия 
	/// </summary>
	public class DisableBrowserCache : ActionFilterAttribute, IActionFilter
	{
		public override void OnResultExecuting(ResultExecutingContext filterContext)
		{
			HttpResponseBase response = filterContext.HttpContext.Response;

			response.Cache.SetExpires(DateTime.UtcNow.AddDays(-1));
			response.Cache.SetValidUntilExpires(false);
			response.Cache.SetRevalidation(HttpCacheRevalidation.AllCaches);
			response.Cache.SetCacheability(HttpCacheability.NoCache);
			response.Cache.SetNoStore();

			base.OnResultExecuting(filterContext);
		}

		public override void OnResultExecuted(ResultExecutedContext filterContext)
		{
			base.OnResultExecuted(filterContext);
		}
	}
}
