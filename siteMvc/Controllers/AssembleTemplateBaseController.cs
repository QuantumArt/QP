using System.Web.Mvc;
using Quantumart.QP8.BLL.Services.MultistepActions;
using Quantumart.QP8.Constants;
using Quantumart.QP8.WebMvc.Extensions.ActionFilters;
using Quantumart.QP8.WebMvc.Extensions.Controllers;
using Quantumart.QP8.WebMvc.Infrastructure.Enums;

namespace Quantumart.QP8.WebMvc.Controllers
{
    public class AssembleTemplateBaseController : QPController
    {
        protected readonly IMultistepActionService Service;

        public AssembleTemplateBaseController()
        {
        }

        public AssembleTemplateBaseController(IMultistepActionService service)
        {
            Service = service;
        }

        [HttpPost]
        [ExceptionResult(ExceptionResultMode.OperationAction)]
        [ActionAuthorize(ActionCode.AssembleTemplate)]
        public virtual ActionResult PreAction(int parentId, int id)
        {
            return Json(Service.PreAction(parentId, id));
        }

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
        public ActionResult Step(int stage, int step)
        {
            var stepResult = Service.Step(stage, step);
            return Json(stepResult);
        }

        [HttpPost]
        public ActionResult TearDown(bool isError)
        {
            Service.TearDown();
            return null;
        }
    }
}
