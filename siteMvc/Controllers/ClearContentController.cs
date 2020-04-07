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
    public class ClearContentController : AuthQpController
    {
        private readonly IMultistepActionService _service;

        public ClearContentController(ClearContentService service)
        {
            _service = service;
        }

        [HttpPost]
        [ExceptionResult(ExceptionResultMode.OperationAction)]
        [ActionAuthorize(ActionCode.ClearContent)]
        public ActionResult PreAction(int parentId, int id) => Json(_service.PreAction(parentId, id));

        [HttpPost]
        [ExceptionResult(ExceptionResultMode.OperationAction)]
        [ActionAuthorize(ActionCode.ClearContent)]
        [BackendActionContext(ActionCode.ClearContent)]
        [BackendActionLog]
        public ActionResult Setup(int parentId, int id, bool? boundToExternal) => Json(_service.Setup(parentId, id, boundToExternal));

        [HttpPost]
        [ConnectionScope]
        [ExceptionResult(ExceptionResultMode.OperationAction)]
        public ActionResult Step([FromBody] MultiStepActionViewModel model) => Json(_service.Step(model.Stage, model.Step));

        [HttpPost]
        public void TearDown(bool isError)
        {
            _service.TearDown();
        }
    }
}
