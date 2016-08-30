using System;
using System.Web.Mvc;
using Quantumart.QP8.BLL.Services.MultistepActions;
using Quantumart.QP8.WebMvc.Extensions.ActionFilters;
using Quantumart.QP8.WebMvc.Extensions.Controllers;

namespace Quantumart.QP8.WebMvc.Controllers
{
    public class MultistepController : QPController
    {

        private readonly Func<string, IMultistepActionService> _getService;
        private readonly Func<string, string> _getActionCode;

        public MultistepController(Func<string, IMultistepActionService> getService, Func<string, string> getActionCode)
        {
            _getService = getService;
            _getActionCode = getActionCode;
        }

        protected override IActionInvoker CreateActionInvoker()
        {
            return new MultistepActionInvoker(_getActionCode);
        }

        [HttpPost]
        [ExceptionResult(ExceptionResultMode.OperationAction)]
        public ActionResult PreAction(string command, int parentId, int[] IDs)
        {
            return Json(_getService(command).PreAction(parentId, 0, IDs));
        }

        [HttpPost]
        [ExceptionResult(ExceptionResultMode.OperationAction)]
        public ActionResult Setup(string command, int parentId, int[] IDs, bool? boundToExternal)
        {
            return Json(_getService(command).Setup(parentId, 0, IDs, boundToExternal));
        }

        [HttpPost]
        [ExceptionResult(ExceptionResultMode.OperationAction)]
        public ActionResult Step(string command, int stage, int step)
        {
            return Json(_getService(command).Step(stage, step));
        }

        [HttpPost]
        [ExceptionResult(ExceptionResultMode.OperationAction)]
        public ActionResult TearDown(string command)
        {
            _getService(command).TearDown();
            return null;
        }
    }

    public class MultistepActionInvoker : ControllerActionInvoker
    {
        private readonly Func<string, string> _getActionCode;

        public MultistepActionInvoker(Func<string, string> getActionCode)
        {
            _getActionCode = getActionCode;
        }

        protected override FilterInfo GetFilters(ControllerContext controllerContext, ActionDescriptor actionDescriptor)
        {
            var command = (string)controllerContext.RouteData.Values["command"];
            var actionCode = _getActionCode(command);

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
