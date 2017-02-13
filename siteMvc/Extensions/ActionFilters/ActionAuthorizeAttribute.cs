using System.Security;
using System.Web.Mvc;
using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Services;
using Quantumart.QP8.Resources;
using Quantumart.QP8.Security;

namespace Quantumart.QP8.WebMvc.Extensions.ActionFilters
{
    public class ActionAuthorizeAttribute : FilterAttribute, IAuthorizationFilter
    {
        private readonly string _actionCode;

        public ActionAuthorizeAttribute(string actionCode)
        {
            _actionCode = actionCode;
        }

        public void OnAuthorization(AuthorizationContext filterContext)
        {
            var identity = filterContext.HttpContext.User.Identity as QpIdentity;
            if (identity == null || !identity.IsAuthenticated)
            {
                throw new SecurityException(GlobalStrings.YouAreNotAuthenticated);
            }

            BackendAction action;
            if (!DependencyResolver.Current.GetService<ISecurityService>().IsActionAccessible(_actionCode, out action))
            {
                throw new SecurityException(string.Format(GlobalStrings.ActionIsNotAccessible, action.Name));
            }
        }
    }
}
