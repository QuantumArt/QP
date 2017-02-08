using System.Web.Mvc;
using Quantumart.QP8.BLL.Services.MultistepActions;
using Quantumart.QP8.Constants;
using Quantumart.QP8.WebMvc.Extensions.Controllers;
using Quantumart.QP8.WebMvc.Infrastructure.ActionFilters;
using Quantumart.QP8.WebMvc.Infrastructure.Enums;

namespace Quantumart.QP8.WebMvc.Controllers
{
    public class ClearContentController : QPController
    {
        private readonly IMultistepActionService _service;

        public ClearContentController(IMultistepActionService service)
        {
            _service = service;
        }

        [HttpPost]
        [ExceptionResult(ExceptionResultMode.OperationAction)]
        [ActionAuthorize(ActionCode.ClearContent)]
        public ActionResult PreAction(int parentId, int id)
        {
            return Json(_service.PreAction(parentId, id));
        }

        [HttpPost]
        [ExceptionResult(ExceptionResultMode.OperationAction)]
        [ActionAuthorize(ActionCode.ClearContent)]
        [BackendActionContext(ActionCode.ClearContent)]
        [BackendActionLog]
        public ActionResult Setup(int parentId, int id, bool? boundToExternal)
        {
            return Json(_service.Setup(parentId, id, boundToExternal));
        }

        [HttpPost]
        [ConnectionScope]
        [ExceptionResult(ExceptionResultMode.OperationAction)]
        public ActionResult Step(int stage, int step)
        {
            return Json(_service.Step(stage, step));
        }

        [HttpPost]
        public void TearDown(bool isError)
        {
            _service.TearDown();
        }
    }
}
