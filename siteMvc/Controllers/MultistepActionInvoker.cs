using System;
using System.Web.Mvc;
using Quantumart.QP8.BLL.Services.MultistepActions.Base;
using Quantumart.QP8.WebMvc.Infrastructure.ActionFilters;

namespace Quantumart.QP8.WebMvc.Controllers
{
    public class MultistepActionInvoker : ControllerActionInvoker
    {
        private readonly Func<string, IActionCode> _getActionCode;

        public MultistepActionInvoker(Func<string, IActionCode> getActionCode)
        {
            _getActionCode = getActionCode;
        }

        protected override FilterInfo GetFilters(ControllerContext controllerContext, ActionDescriptor actionDescriptor)
        {
            var command = (string)controllerContext.RouteData.Values["command"];
            var actionCode = _getActionCode(command).ActionCode;

            var filters = base.GetFilters(controllerContext, actionDescriptor);
            filters.AuthorizationFilters.Add(new ActionAuthorizeAttribute(actionCode));

            if (actionDescriptor.ActionName == nameof(MultistepController.Setup))
            {
                filters.ActionFilters.Add(new BackendActionContextAttribute(actionCode));
                filters.ActionFilters.Add(new BackendActionLogAttribute());
                filters.ActionFilters.Add(new RecordAttribute());
            }

            return filters;
        }
    }
}
