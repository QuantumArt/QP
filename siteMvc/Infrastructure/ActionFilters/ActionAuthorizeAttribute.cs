using System.Security;
using System.Web.Mvc;
using Quantumart.QP8.BLL.Services;
using Quantumart.QP8.Resources;
using Quantumart.QP8.Security;

namespace Quantumart.QP8.WebMvc.Infrastructure.ActionFilters
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
            if (!(filterContext.HttpContext.User.Identity is QpIdentity identity) || !identity.IsAuthenticated)
            {
                throw new SecurityException(GlobalStrings.YouAreNotAuthenticated);
            }

            if (!DependencyResolver.Current.GetService<ISecurityService>().IsActionAccessible(_actionCode, out var action))
            {
                throw new SecurityException(string.Format(GlobalStrings.ActionIsNotAccessible, action.Name));
            }
        }
    }
}
