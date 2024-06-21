using Microsoft.AspNetCore.Mvc;
using Quantumart.QP8.BLL.Helpers;
using Quantumart.QP8.BLL.Services;
using Quantumart.QP8.Constants;
using Quantumart.QP8.WebMvc.Extensions.Controllers;
using Quantumart.QP8.WebMvc.Infrastructure.ActionFilters;
using Quantumart.QP8.WebMvc.Infrastructure.Enums;
using Quantumart.QP8.WebMvc.ViewModels;

namespace Quantumart.QP8.WebMvc.Controllers
{
    public class RecreateDynamicImagesController : AuthQpController
    {
        private readonly IRecreateDynamicImagesService _service;
        private readonly PathHelper _pathHelper;

        public RecreateDynamicImagesController(IRecreateDynamicImagesService service, PathHelper pathHelper)
        {
            _service = service;
            _pathHelper = pathHelper;
        }

        [HttpPost]
        [ExceptionResult(ExceptionResultMode.OperationAction)]
        [ActionAuthorize(ActionCode.RecreateDynamicImages)]
        [BackendActionContext(ActionCode.RecreateDynamicImages)]
        [BackendActionLog]
        public ActionResult Setup(int parentId, int id)
        {
            var settings = _service.SetupAction(parentId, id);
            return Json(settings);
        }

        [HttpPost]
        [ExceptionResult(ExceptionResultMode.OperationAction)]
        public ActionResult Step([FromBody] MultiStepActionViewModel model)
        {
            return Json(_service.Step(model.Step, _pathHelper));
        }

        [HttpPost]
        public ActionResult TearDown(bool isError)
        {
            _service.TearDown();
            return Json(null);
        }
    }
}
