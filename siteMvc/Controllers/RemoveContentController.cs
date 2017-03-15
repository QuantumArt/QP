using System.Web.Mvc;
using Quantumart.QP8.BLL.Services.MultistepActions;
using Quantumart.QP8.Constants;
using Quantumart.QP8.WebMvc.Extensions.Controllers;
using Quantumart.QP8.WebMvc.Infrastructure.ActionFilters;
using Quantumart.QP8.WebMvc.Infrastructure.Enums;

namespace Quantumart.QP8.WebMvc.Controllers
{
    public class RemoveContentController : QPController
    {
        private readonly IMultistepActionService _service;

        public RemoveContentController(IMultistepActionService service)
        {
            _service = service;
        }

        [HttpPost]
        [ExceptionResult(ExceptionResultMode.OperationAction)]
        [ActionAuthorize(ActionCode.RemoveContent)]
        [BackendActionContext(ActionCode.RemoveContent)]
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
        [BackendActionContext(ActionCode.RemoveContent)]
        [Record(ActionCode.SimpleRemoveContent, true)]
        public void TearDown(bool isError)
        {
            _service.TearDown();
        }
    }
}
