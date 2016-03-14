using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Quantumart.QP8.BLL;
using Quantumart.QP8.Utils;
using Quantumart.QP8.BLL.Repository;

namespace Quantumart.QP8.WebMvc.Extensions.ActionFilters
{
	[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
	public class BackendActionContextAttribute : ActionFilterAttribute, IActionFilter
	{
		public static readonly int FilterOrder = ConnectionScopeAttribute.FilterOrder + 1;

		string entitySingleIdParamName;
		string parentEntityIdParamName;
		string entityMultiIdParamName;
		string actionCode;

		public BackendActionContextAttribute(string actionCode, string entitySingleIdParamName = "id", string entityMultiIdParamName = "IDs", string parentEntityIdParamName = "parentId")
		{
			Order = FilterOrder;
			this.entitySingleIdParamName = entitySingleIdParamName;
			this.entityMultiIdParamName = entityMultiIdParamName;
			this.parentEntityIdParamName = parentEntityIdParamName;
			this.actionCode = actionCode;
		}

		public override void OnActionExecuting(ActionExecutingContext filterContext)
		{
			base.OnActionExecuting(filterContext);

			List<int> entityIDs = new List<int>();
			List<string> stringEntityIDs = new List<string>();
			ValueProviderResult vpr = filterContext.Controller.ValueProvider.GetValue(entitySingleIdParamName);
			if (vpr != null)
			{
				stringEntityIDs.Add(vpr.AttemptedValue);				
			}
			else
			{
				// для multiply
				if(filterContext.ActionParameters.ContainsKey(entityMultiIdParamName))
				{
					IEnumerable<int> ids = filterContext.ActionParameters[entityMultiIdParamName] as IEnumerable<int>;
					IEnumerable<string> strIds = filterContext.ActionParameters[entityMultiIdParamName] as IEnumerable<string>;
					if (ids != null)
						stringEntityIDs.AddRange(Converter.ToStringCollection(ids.ToArray()));
					else if (strIds != null)
						stringEntityIDs.AddRange(strIds);
				}
			}

			int? parentEntityId = null;
			vpr = filterContext.Controller.ValueProvider.GetValue(parentEntityIdParamName);
			if (vpr != null && !String.IsNullOrEmpty(vpr.AttemptedValue))
			{
				int peid;
				if (Int32.TryParse(vpr.AttemptedValue, out peid))
					parentEntityId = peid;
			}

			
			BackendActionContext.SetCurrent(actionCode, stringEntityIDs, parentEntityId);
		}

		public override void OnActionExecuted(ActionExecutedContext filterContext)
		{
			BackendActionContext.ResetCurrent();
			base.OnActionExecuted(filterContext);
		}
	}
}