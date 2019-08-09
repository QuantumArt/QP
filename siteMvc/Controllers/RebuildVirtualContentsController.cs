using Microsoft.AspNetCore.Mvc;
using Quantumart.QP8.BLL.Services.MultistepActions;
using Quantumart.QP8.Constants;
using Quantumart.QP8.WebMvc.Extensions.Controllers;
using Quantumart.QP8.WebMvc.Infrastructure.ActionFilters;
using Quantumart.QP8.WebMvc.Infrastructure.Enums;

namespace Quantumart.QP8.WebMvc.Controllers
{
    public class RebuildVirtualContentsController : QPController
    {
        private readonly IMultistepActionService _service;

        public RebuildVirtualContentsController(IMultistepActionService service)
        {
            _service = service;
        }

        [HttpPost]
        [ExceptionResult(ExceptionResultMode.OperationAction)]
        [ConnectionScope]
        [ActionAuthorize(ActionCode.RebuildVirtualContents)]
        [BackendActionContext(ActionCode.RebuildVirtualContents)]
        [BackendActionLog]
        public ActionResult Setup(int parentId, int id, bool? boundToExternal)
        {
            var settings = _service.Setup(parentId, id, boundToExternal);
            return Json(settings);
        }

        [HttpPost]
        [ExceptionResult(ExceptionResultMode.OperationAction)]
        [ConnectionScope]
        public ActionResult Step(int stage, int step)
        {
            var stepResult = _service.Step(stage, step);
            return Json(stepResult);
        }

        [HttpPost]
        public ActionResult TearDown(bool isError)
        {
            _service.TearDown();
            return null;
        }
    }
}
