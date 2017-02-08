using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Quantumart.QP8.BLL;

namespace Quantumart.QP8.WebMvc.Infrastructure.ActionFilters
{
    [AttributeUsage(AttributeTargets.Method)]
    public class BackendActionContextAttribute : ActionFilterAttribute
    {
        public static readonly int FilterOrder = ConnectionScopeAttribute.FilterOrder + 1;

        private readonly string _entitySingleIdParamName;
        private readonly string _parentEntityIdParamName;
        private readonly string _entityMultiIdParamName;
        private readonly string _actionCode;

        public BackendActionContextAttribute(string actionCode, string entitySingleIdParamName = "id", string entityMultiIdParamName = "IDs", string parentEntityIdParamName = "parentId")
        {
            Order = FilterOrder;
            _entitySingleIdParamName = entitySingleIdParamName;
            _entityMultiIdParamName = entityMultiIdParamName;
            _parentEntityIdParamName = parentEntityIdParamName;
            _actionCode = actionCode;
        }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);
            var stringEntityIDs = new List<string>();
            var vpr = filterContext.Controller.ValueProvider.GetValue(_entitySingleIdParamName);
            if (vpr != null)
            {
                stringEntityIDs.Add(vpr.AttemptedValue);
            }
            else
            {
                if (filterContext.ActionParameters.ContainsKey(_entityMultiIdParamName))
                {
                    var ids = filterContext.ActionParameters[_entityMultiIdParamName] as IEnumerable<int>;
                    var strIds = filterContext.ActionParameters[_entityMultiIdParamName] as IEnumerable<string>;
                    if (ids != null)
                    {
                        stringEntityIDs.AddRange(ids.Select(id => id.ToString()));
                    }
                    else if (strIds != null)
                    {
                        stringEntityIDs.AddRange(strIds);
                    }
                }
            }

            int? parentEntityId = null;
            vpr = filterContext.Controller.ValueProvider.GetValue(_parentEntityIdParamName);
            if (!string.IsNullOrEmpty(vpr?.AttemptedValue))
            {
                int peid;
                if (int.TryParse(vpr.AttemptedValue, out peid))
                {
                    parentEntityId = peid;
                }
            }

            BackendActionContext.SetCurrent(_actionCode, stringEntityIDs, parentEntityId);
        }

        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            BackendActionContext.ResetCurrent();
            base.OnActionExecuted(filterContext);
        }
    }
}
