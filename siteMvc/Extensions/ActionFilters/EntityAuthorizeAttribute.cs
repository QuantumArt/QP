using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Security;
using Quantumart.QP8.Security;
using Quantumart.QP8.BLL.Services;
using Quantumart.QP8.Resources;
using Quantumart.QP8.BLL;

namespace Quantumart.QP8.WebMvc.Extensions.ActionFilters
{
	/// <summary>
	/// Фильтр для авторизации по коду действия
	/// </summary>
	public class EntityAuthorizeAttribute : FilterAttribute, IAuthorizationFilter
	{
		string actionTypeCode;
		string entityTypeCode;
		string entityIdParamName;

		public EntityAuthorizeAttribute(string actionTypeCode, string entityTypeCode, string entityIdParamName)
		{
			this.actionTypeCode = actionTypeCode;
			this.entityTypeCode = entityTypeCode;
			this.entityIdParamName = entityIdParamName;
		}

		#region IAuthorizationFilter Members

		public void OnAuthorization(AuthorizationContext filterContext)
		{
			QPIdentity identity = filterContext.HttpContext.User.Identity as QPIdentity;
			if (identity == null || !identity.IsAuthenticated)
				throw new SecurityException(GlobalStrings.YouAreNotAuthenticated);

			ValueProviderResult entityIdResult = filterContext.Controller.ValueProvider.GetValue(entityIdParamName);
			if(entityIdResult == null || String.IsNullOrEmpty(entityIdResult.AttemptedValue))
				throw new ArgumentException(String.Format("Entity id field is not found: {0}", entityIdParamName));
			int entityId;
			if(!Int32.TryParse(entityIdResult.AttemptedValue, out entityId))
				throw new ArgumentException(String.Format("Entity id is not a number: {0}", entityIdResult.AttemptedValue));

			EntityType entityType = EntityTypeService.GetByCode(entityTypeCode);
			if (entityType == null)
				throw new ArgumentException(String.Format("Unknown entity type: {0}", entityTypeCode));

			BackendActionType actionType = BackendActionTypeService.GetByCode(actionTypeCode);
			if (actionType == null)
				throw new ArgumentException(String.Format("Unknown action type: {0}", actionTypeCode));


			if (!DependencyResolver.Current.GetService<ISecurityService>().IsEntityAccessible(entityTypeCode, entityId, actionTypeCode))
				throw new SecurityException(String.Format(GlobalStrings.EntityIsNotAccessible, actionType.Name, entityType.Name, entityId));

		}

		#endregion
	}
}