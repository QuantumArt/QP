using System;
using Microsoft.AspNetCore.Mvc;
using Quantumart.QP8.BLL.Services.MultistepActions;
using Quantumart.QP8.WebMvc.Extensions.Controllers;
using Quantumart.QP8.WebMvc.Infrastructure.ActionFilters;
using Quantumart.QP8.WebMvc.Infrastructure.Enums;

namespace Quantumart.QP8.WebMvc.Controllers
{
    public class MultistepController : QPController
    {
        private readonly Func<string, IMultistepActionService> _getService;

        public MultistepController(Func<string, IMultistepActionService> getService)
        {
            _getService = getService;
        }

        [HttpPost]
        [ExceptionResult(ExceptionResultMode.OperationAction)]
        [ActionAuthorize(null)]
        public ActionResult PreAction(string command, int parentId, int[] IDs)
        {
            return Json(_getService(command).PreAction(parentId, 0, IDs));
        }

        [HttpPost]
        [ExceptionResult(ExceptionResultMode.OperationAction)]
        [ActionAuthorize(null)]
        [BackendActionContext(null)]
        [BackendActionLog]
        [Record]
        public ActionResult Setup(string command, int parentId, int[] IDs, bool? boundToExternal)
        {
            return Json(_getService(command).Setup(parentId, 0, IDs, boundToExternal));
        }

        [HttpPost]
        [ExceptionResult(ExceptionResultMode.OperationAction)]
        [ActionAuthorize(null)]
        public ActionResult Step(string command, int stage, int step)
        {
            return Json(_getService(command).Step(stage, step));
        }

        [HttpPost]
        [ExceptionResult(ExceptionResultMode.OperationAction)]
        [ActionAuthorize(null)]
        public ActionResult TearDown(string command)
        {
            _getService(command).TearDown();
            return null;
        }
    }
}
