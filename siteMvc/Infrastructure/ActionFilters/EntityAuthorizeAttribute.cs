using System;
using System.Security;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.DependencyInjection;
using Quantumart.QP8.BLL.Services;
using Quantumart.QP8.Resources;
using Quantumart.QP8.Security;

namespace Quantumart.QP8.WebMvc.Infrastructure.ActionFilters
{
    /// <remarks>
    /// We don't use <see cref="IAuthorizationFilter"/> because controller and
    /// exception filters are not initialized during .OnAuthorization() execution
    /// </summary>
    public class EntityAuthorizeAttribute : ActionFilterAttribute
    {
        /// <summary>
        /// Execute before any ActionFilter to simulate <see cref="IAuthorizationFilter"/> behaviour
        /// </summary>
        public static readonly int FilterOrder = ConnectionScopeAttribute.FilterOrder - 1;

        private readonly string _actionTypeCode;
        private readonly string _entityTypeCode;
        private readonly string _entityIdParamName;

        public EntityAuthorizeAttribute(string actionTypeCode, string entityTypeCode, string entityIdParamName)
        {
            Order = FilterOrder;
            _actionTypeCode = actionTypeCode;
            _entityTypeCode = entityTypeCode;
            _entityIdParamName = entityIdParamName;
        }

        public override async Task OnActionExecutionAsync(ActionExecutingContext filterContext, ActionExecutionDelegate next)
        {
            if (!(filterContext.HttpContext.User.Identity is QpIdentity identity) || !identity.IsAuthenticated)
            {
                throw new SecurityException(GlobalStrings.YouAreNotAuthenticated);
            }

            IServiceProvider serviceProvider = filterContext.HttpContext.RequestServices;
            ControllerContext controllerContext = ((Controller)filterContext.Controller).ControllerContext;
            IValueProvider valueProvider = await CompositeValueProvider.CreateAsync(controllerContext);

            var entityIdResult = valueProvider.GetValue(_entityIdParamName);
            if (string.IsNullOrEmpty(entityIdResult.FirstValue))
            {
                throw new ArgumentException($"Entity id field is not found: {_entityIdParamName}");
            }

            if (!int.TryParse(entityIdResult.FirstValue, out var entityId))
            {
                throw new ArgumentException($"Entity id is not a number: {entityIdResult.FirstValue}");
            }

            var entityType = EntityTypeService.GetByCode(_entityTypeCode);
            if (entityType == null)
            {
                throw new ArgumentException($"Unknown entity type: {_entityTypeCode}");
            }

            var actionType = BackendActionTypeService.GetByCode(_actionTypeCode);
            if (actionType == null)
            {
                throw new ArgumentException($"Unknown action type: {_actionTypeCode}");
            }

            var securityService = serviceProvider.GetRequiredService<ISecurityService>();

            if (!securityService.IsEntityAccessible(_entityTypeCode, entityId, _actionTypeCode))
            {
                throw new SecurityException(string.Format(
                    GlobalStrings.EntityIsNotAccessible, actionType.Name, entityType.Name, entityId));
            }

            await next.Invoke();
        }
    }
}
