using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.DependencyInjection;
using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Services.MultistepActions.Base;

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

        /// <summary>
        /// If <paramref name="actionCode"/> is null then it taken from RouteData.Values["command"]
        /// </summary>
        public BackendActionContextAttribute(string actionCode, string entitySingleIdParamName = "id", string entityMultiIdParamName = "IDs", string parentEntityIdParamName = "parentId")
        {
            Order = FilterOrder;
            _entitySingleIdParamName = entitySingleIdParamName;
            _entityMultiIdParamName = entityMultiIdParamName;
            _parentEntityIdParamName = parentEntityIdParamName;
            _actionCode = actionCode;
        }

        public override async Task OnActionExecutionAsync(ActionExecutingContext filterContext, ActionExecutionDelegate next)
        {
            IServiceProvider serviceProvider = filterContext.HttpContext.RequestServices;
            ControllerContext controllerContext = ((Controller)filterContext.Controller).ControllerContext;
            IValueProvider valueProvider = await CompositeValueProvider.CreateAsync(controllerContext);

            var stringEntityIDs = new List<string>();

            if (int.TryParse(valueProvider.GetValue(_entitySingleIdParamName).FirstValue, out int entityId))
            {
                stringEntityIDs.Add(entityId.ToString());
            }
            else
            {
                if (filterContext.ActionArguments.ContainsKey(_entityMultiIdParamName))
                {
                    var ids = filterContext.ActionArguments[_entityMultiIdParamName] as IEnumerable<int>;
                    var strIds = filterContext.ActionArguments[_entityMultiIdParamName] as IEnumerable<string>;
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

            if (int.TryParse(valueProvider.GetValue(_parentEntityIdParamName).FirstValue, out int parentId))
            {
                parentEntityId = parentId;
            }

            string actionCode = _actionCode;

            if (actionCode == null)
            {
                var getActionCode = serviceProvider.GetRequiredService<Func<string, IActionCode>>();
                var command = (string)filterContext.RouteData.Values["command"];
                actionCode = getActionCode(command).ActionCode;
            }

            BackendActionContext.SetCurrent(actionCode, stringEntityIDs, parentEntityId);

            try
            {
                await next.Invoke();
            }
            finally
            {
                BackendActionContext.ResetCurrent();
            }
        }
    }
}
