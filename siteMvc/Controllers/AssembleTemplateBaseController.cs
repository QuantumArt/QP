using Microsoft.AspNetCore.Mvc;
using Quantumart.QP8.BLL.Services.MultistepActions;
using Quantumart.QP8.BLL.Services.MultistepActions.Assemble;
using Quantumart.QP8.Constants;
using Quantumart.QP8.WebMvc.Extensions.Controllers;
using Quantumart.QP8.WebMvc.Infrastructure.ActionFilters;
using Quantumart.QP8.WebMvc.Infrastructure.Enums;
using Quantumart.QP8.WebMvc.ViewModels;

namespace Quantumart.QP8.WebMvc.Controllers
{
    public class AssembleTemplateBaseController : AuthQpController
    {
        protected readonly IMultistepActionService Service;

        public AssembleTemplateBaseController()
        {
        }

        public AssembleTemplateBaseController(AssembleTemplateService service)
        {
            Service = service;
        }

        [HttpPost]
        [ExceptionResult(ExceptionResultMode.OperationAction)]
        [ActionAuthorize(ActionCode.AssembleTemplate)]
        public virtual ActionResult PreAction(int parentId, int id) => Json(Service.PreAction(parentId, id));

        [HttpPost]
        [ExceptionResult(ExceptionResultMode.OperationAction)]
        [BackendActionContext(ActionCode.AssembleTemplate)]
        [ActionAuthorize(ActionCode.AssembleTemplate)]
        [BackendActionLog]
        public virtual ActionResult Setup(int parentId, int id, bool? boundToExternal)
        {
            var settings = Service.Setup(parentId, id, boundToExternal);
            return Json(settings);
        }

        [HttpPost]
        [NoTransactionConnectionScope]
        [ExceptionResult(ExceptionResultMode.OperationAction)]
        public ActionResult Step([FromBody] MultiStepActionViewModel model)
        {
            var stepResult = Service.Step(model.Stage, model.Step);
            return Json(stepResult);
        }

        [HttpPost]
        public ActionResult TearDown(bool isError)
        {
            Service.TearDown();
            return Json(null);
        }
    }
}
