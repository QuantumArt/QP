using System.Globalization;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Primitives;

namespace Quantumart.QP8.WebMvc.Infrastructure.ActionFilters;

public class CustomActionLocalizeAttribute : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        if (context.HttpContext.Request.Form.TryGetValue("current-culture", out StringValues currentCultureName))
        {
            CultureInfo.CurrentCulture = new(currentCultureName);
        }

        if (context.HttpContext.Request.Form.TryGetValue("current-ui-culture", out StringValues currentUiCultureName))
        {
            CultureInfo.CurrentUICulture = new(currentUiCultureName);
        }

        base.OnActionExecuting(context);
    }
}
