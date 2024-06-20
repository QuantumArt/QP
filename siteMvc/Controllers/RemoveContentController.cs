using Microsoft.AspNetCore.Mvc;
using Quantumart.QP8.BLL.Services.MultistepActions.Removing;
using Quantumart.QP8.Constants;
using Quantumart.QP8.WebMvc.Extensions.Controllers;
using Quantumart.QP8.WebMvc.Infrastructure.ActionFilters;
using Quantumart.QP8.WebMvc.Infrastructure.Enums;
using Quantumart.QP8.WebMvc.ViewModels;
using PathHelper = Quantumart.QP8.BLL.Helpers.PathHelper;

namespace Quantumart.QP8.WebMvc.Controllers
{
    public class RemoveContentController : AuthQpController
    {
        private readonly RemoveContentService _service;
        private readonly PathHelper _pathHelper;

        public RemoveContentController(RemoveContentService service, PathHelper pathHelper)
        {
            _service = service;
            _pathHelper = pathHelper;
        }

        [HttpPost]
        [ExceptionResult(ExceptionResultMode.OperationAction)]
        [ActionAuthorize(ActionCode.RemoveContent)]
        [BackendActionContext(ActionCode.RemoveContent)]
        [BackendActionLog]
        public ActionResult Setup(int parentId, int id, bool? boundToExternal)
        {
            return Json(_service.Setup(parentId, id, boundToExternal, _pathHelper.S3Options));
        }

        [HttpPost]
        [ConnectionScope]
        [ExceptionResult(ExceptionResultMode.OperationAction)]
        public ActionResult Step([FromBody] MultiStepActionViewModel model)
        {
            return Json(_service.Step(model.Stage, model.Step));
        }

        [HttpPost]
        [BackendActionContext(ActionCode.RemoveContent)]
        [Record(ActionCode.SimpleRemoveContent, true)]
        public void TearDown(bool isError)
        {
            _service.TearDown();
        }
    }
}
