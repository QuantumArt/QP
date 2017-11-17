using System;
using System.Diagnostics.CodeAnalysis;
using System.Web.Mvc;
using Quantumart.QP8.BLL.Services.MultistepActions;
using Quantumart.QP8.WebMvc.Extensions.Controllers;
using Quantumart.QP8.WebMvc.Infrastructure.ActionFilters;
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

        protected override IActionInvoker CreateActionInvoker() => new MultistepActionInvoker(_getActionCode);

        [HttpPost]
        [ExceptionResult(ExceptionResultMode.OperationAction)]
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        public ActionResult PreAction(string command, int parentId, int[] IDs) => Json(_getService(command).PreAction(parentId, 0, IDs));

        [HttpPost]
        [ExceptionResult(ExceptionResultMode.OperationAction)]
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        public ActionResult Setup(string command, int parentId, int[] IDs, bool? boundToExternal) => Json(_getService(command).Setup(parentId, 0, IDs, boundToExternal));

        [HttpPost]
        [ExceptionResult(ExceptionResultMode.OperationAction)]
        public ActionResult Step(string command, int stage, int step) => Json(_getService(command).Step(stage, step));

        [HttpPost]
        [ExceptionResult(ExceptionResultMode.OperationAction)]
        public ActionResult TearDown(string command)
        {
            _getService(command).TearDown();
            return null;
        }
    }
}
