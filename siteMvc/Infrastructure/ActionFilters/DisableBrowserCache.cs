using Microsoft.AspNetCore.Mvc.Filters;

namespace Quantumart.QP8.WebMvc.Infrastructure.ActionFilters
{
    public class DisableBrowserCache : ActionFilterAttribute
    {
        public override void OnResultExecuting(ResultExecutingContext filterContext)
        {
            var headers = filterContext.HttpContext.Response.Headers;

            headers["Cache-Control"] = "no-cache, no-store, must-revalidate";
            headers["Expires"] = "-1";
            headers["Pragma"] = "no-cache";

            base.OnResultExecuting(filterContext);
        }
    }
}
