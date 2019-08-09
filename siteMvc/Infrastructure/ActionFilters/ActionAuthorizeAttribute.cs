using System;
using System.Security;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Services;
using Quantumart.QP8.BLL.Services.MultistepActions.Base;
using Quantumart.QP8.Resources;
using Quantumart.QP8.Security;

namespace Quantumart.QP8.WebMvc.Infrastructure.ActionFilters
{
    public class ActionAuthorizeAttribute : Attribute, IAuthorizationFilter
    {
        private readonly string _actionCode;

        /// <summary>
        /// If <paramref name="actionCode"/> is null then it taken from RouteData.Values["command"]
        /// </summary>
        public ActionAuthorizeAttribute(string actionCode)
        {
            _actionCode = actionCode;
        }

        public void OnAuthorization(AuthorizationFilterContext filterContext)
        {
            var serviceProvider = filterContext.HttpContext.RequestServices;

            if (!(filterContext.HttpContext.User.Identity is QpIdentity identity) || !identity.IsAuthenticated)
            {
                throw new SecurityException(GlobalStrings.YouAreNotAuthenticated);
            }

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
                throw new SecurityException(string.Format(GlobalStrings.ActionIsNotAccessible, action.Name));
            }
        }
    }
}
