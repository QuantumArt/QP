using System;
using Microsoft.AspNetCore.Mvc;
using Quantumart.QP8.BLL.Helpers;
using Quantumart.QP8.BLL.Services.MultistepActions;
using Quantumart.QP8.WebMvc.Extensions.Controllers;
using Quantumart.QP8.WebMvc.Infrastructure.ActionFilters;
using Quantumart.QP8.WebMvc.Infrastructure.Enums;
using Quantumart.QP8.WebMvc.ViewModels;

namespace Quantumart.QP8.WebMvc.Controllers
{
    public class MultistepController : AuthQpController
    {
        private readonly Func<string, IMultistepActionService> _getService;
        private readonly PathHelper _pathHelper;

        public MultistepController(Func<string, IMultistepActionService> getService, PathHelper pathHelper)
        {
            _getService = getService;
            _pathHelper = pathHelper;
        }

        [HttpPost]
        [ExceptionResult(ExceptionResultMode.OperationAction)]
        [ActionAuthorize(null)]
        public ActionResult PreAction(string command, int parentId, [FromBody] SelectedItemsViewModel model)
        {
            return Json(_getService(command).PreAction(parentId, 0, model.Ids));
        }

        [HttpPost]
        [ExceptionResult(ExceptionResultMode.OperationAction)]
        [ActionAuthorize(null)]
        [BackendActionContext(null)]
        [BackendActionLog]
        [Record]
        public ActionResult Setup(string command, int parentId, [FromBody] SelectedItemsViewModel model, bool? boundToExternal)
        {
            return Json(_getService(command).Setup(parentId, 0, model.Ids, boundToExternal, _pathHelper.S3Options));
        }

        [HttpPost]
        [ExceptionResult(ExceptionResultMode.OperationAction)]
        [ActionAuthorize(null)]
        public ActionResult Step(string command, [FromBody] MultiStepActionViewModel model)
        {
            return Json(_getService(command).Step(model.Stage, model.Step));
        }

        [HttpPost]
        [ExceptionResult(ExceptionResultMode.OperationAction)]
        [ActionAuthorize(null)]
        public ActionResult TearDown(string command)
        {
            _getService(command).TearDown();
            return Json(null);
        }
    }
}
