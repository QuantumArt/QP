using System.Web.Mvc;
using Quantumart.QP8.BLL.Services.MultistepActions;
using Quantumart.QP8.Constants;
using Quantumart.QP8.WebMvc.Extensions.Controllers;
using Quantumart.QP8.WebMvc.Infrastructure.ActionFilters;
using Quantumart.QP8.WebMvc.Infrastructure.Enums;

namespace Quantumart.QP8.WebMvc.Controllers
{
    public class RemoveSiteController : QPController
    {
        private readonly IMultistepActionService _service;

        public RemoveSiteController(IMultistepActionService service)
        {
            _service = service;
        }

        [HttpPost]
        [ExceptionResult(ExceptionResultMode.OperationAction)]
        [ActionAuthorize(ActionCode.RemoveSite)]
        [EntityAuthorize(ActionTypeCode.Remove, EntityTypeCode.Site, "id")]
        [BackendActionContext(ActionCode.RemoveSite)]
        [BackendActionLog]
        public ActionResult Setup(int parentId, int id, bool? boundToExternal)
        {
            var settings = _service.Setup(parentId, id, boundToExternal);
            return Json(settings);
        }

        [HttpPost]
        [ConnectionScope]
        [ExceptionResult(ExceptionResultMode.OperationAction)]
        public ActionResult Step(int stage, int step)
        {
            var stepResult = _service.Step(stage, step);
            return Json(stepResult);
        }

        [HttpPost]
        [BackendActionContext(ActionCode.RemoveSite)]
        [Record(ActionCode.SimpleRemoveSite, true)]
        public ActionResult TearDown(bool isError)
        {
            _service.TearDown();
            return null;
        }
    }
}
