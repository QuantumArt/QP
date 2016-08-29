using System;
using System.Diagnostics.CodeAnalysis;
using System.Web.Mvc;
using Quantumart.QP8.BLL.Services.MultistepActions;
using Quantumart.QP8.WebMvc.Extensions.ActionFilters;
using Quantumart.QP8.WebMvc.Extensions.Controllers;
using Quantumart.QP8.WebMvc.Infrastructure.Enums;

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
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        public ActionResult PreAction(string command, int parentId, int id, int[] IDs)
        {
            return Json(_getService(command).PreAction(parentId, id, IDs));
        }

        [HttpPost]
        [ExceptionResult(ExceptionResultMode.OperationAction)]
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        public ActionResult Setup(string command, int parentId, int id, int[] IDs, bool? boundToExternal)
        {
            return Json(_getService(command).Setup(parentId, id, IDs, boundToExternal));
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
            filters.ActionFilters.Add(new BackendActionContextAttribute(actionCode));
            return filters;
        }
    }
}
