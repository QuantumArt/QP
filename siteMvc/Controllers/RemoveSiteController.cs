using Microsoft.AspNetCore.Mvc;
using Quantumart.QP8.BLL.Services.MultistepActions;
using Quantumart.QP8.BLL.Services.MultistepActions.Removing;
using Quantumart.QP8.Constants;
using Quantumart.QP8.WebMvc.Extensions.Controllers;
using Quantumart.QP8.WebMvc.Infrastructure.ActionFilters;
using Quantumart.QP8.WebMvc.Infrastructure.Enums;
using Quantumart.QP8.WebMvc.ViewModels;

namespace Quantumart.QP8.WebMvc.Controllers
{
    public class RemoveSiteController : QPController
    {
        private readonly IMultistepActionService _service;

        public RemoveSiteController(RemoveSiteService service)
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
        public ActionResult Step([FromBody] MultiStepActionViewModel model)
        {
            return Json(_service.Step(model.Stage, model.Step));
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
