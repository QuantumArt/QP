using System;
using System.Security;
using System.Web.Mvc;
using Quantumart.QP8.BLL.Services;
using Quantumart.QP8.Resources;
using Quantumart.QP8.Security;

namespace Quantumart.QP8.WebMvc.Infrastructure.ActionFilters
{
    public class EntityAuthorizeAttribute : FilterAttribute, IAuthorizationFilter
    {
        private readonly string _actionTypeCode;
        private readonly string _entityTypeCode;
        private readonly string _entityIdParamName;

        public EntityAuthorizeAttribute(string actionTypeCode, string entityTypeCode, string entityIdParamName)
        {
            _actionTypeCode = actionTypeCode;
            _entityTypeCode = entityTypeCode;
            _entityIdParamName = entityIdParamName;
        }

        public void OnAuthorization(AuthorizationContext filterContext)
        {
            var identity = filterContext.HttpContext.User.Identity as QpIdentity;
            if (identity == null || !identity.IsAuthenticated)
            {
                throw new SecurityException(GlobalStrings.YouAreNotAuthenticated);
            }

            var entityIdResult = filterContext.Controller.ValueProvider.GetValue(_entityIdParamName);
            if (string.IsNullOrEmpty(entityIdResult?.AttemptedValue))
            {
                throw new ArgumentException($"Entity id field is not found: {_entityIdParamName}");
            }

            if (!int.TryParse(entityIdResult.AttemptedValue, out int entityId))
            {
                throw new ArgumentException($"Entity id is not a number: {entityIdResult.AttemptedValue}");
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

            if (!DependencyResolver.Current.GetService<ISecurityService>().IsEntityAccessible(_entityTypeCode, entityId, _actionTypeCode))
            {
                throw new SecurityException(string.Format(GlobalStrings.EntityIsNotAccessible, actionType.Name, entityType.Name, entityId));
            }
        }
    }
}
