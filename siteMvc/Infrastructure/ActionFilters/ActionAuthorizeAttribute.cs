using System;
using System.Security;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using QP8.Infrastructure.Extensions;
using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Services;
using Quantumart.QP8.BLL.Services.MultistepActions.Base;
using Quantumart.QP8.Resources;
using Quantumart.QP8.Security;

namespace Quantumart.QP8.WebMvc.Infrastructure.ActionFilters
{
    /// <remarks>
    /// We don't use <see cref="IAuthorizationFilter"/> because controller and
    /// exception filters are not initialized during .OnAuthorization() execution
    /// </summary>
    public class ActionAuthorizeAttribute : ActionFilterAttribute
    {
        /// <summary>
        /// Execute before any ActionFilter to simulate <see cref="IAuthorizationFilter"/> behaviour
        /// </summary>
        public static readonly int FilterOrder = ConnectionScopeAttribute.FilterOrder - 1;

        private readonly string _actionCode;

        /// <summary>
        /// If <paramref name="actionCode"/> is null then it taken from RouteData.Values["command"]
        /// </summary>
        public ActionAuthorizeAttribute(string actionCode)
        {
            Order = FilterOrder;
            _actionCode = actionCode;
        }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            IServiceProvider serviceProvider = filterContext.HttpContext.RequestServices;

            string actionCode = _actionCode;

            if (actionCode == null)
            {
                var getActionCode = serviceProvider.GetRequiredService<Func<string, IActionCode>>();
                var command = (string)filterContext.RouteData.Values["command"];
                actionCode = getActionCode(command).ActionCode;
            }

            var securityService = serviceProvider.GetRequiredService<ISecurityService>();

            if (!securityService.IsActionAccessible(actionCode, out BackendAction action))
            {
                var message = string.Format(GlobalStrings.ActionIsNotAccessible, action.Name);
                var clientMessage = CustomActionStrings.ActionNotAccessible;
                throw new SecurityException(message) { Data = { { ExceptionHelpers.ClientMessageKey, clientMessage } } };
            }

            base.OnActionExecuting(filterContext);
        }
    }
}
