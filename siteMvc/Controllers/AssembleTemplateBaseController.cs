using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Services.MultistepActions;
using Quantumart.QP8.Constants;
using Quantumart.QP8.WebMvc.Extensions.ActionFilters;
using System.Web.Mvc;
using Quantumart.QP8.WebMvc.Extensions.Controllers;

namespace Quantumart.QP8.WebMvc.Controllers
{
    public class AssembleTemplateBaseController : QPController
    {
		protected readonly IMultistepActionService service;

		public AssembleTemplateBaseController() { }

		public AssembleTemplateBaseController(IMultistepActionService service)
		{
			this.service = service;
		}

		[HttpPost]
		[ExceptionResult(ExceptionResultMode.OperationAction)]
		[ActionAuthorize(ActionCode.AssembleTemplate)]
		public virtual ActionResult PreAction(int parentId, int id)
		{
			return Json(service.PreAction(parentId, id));
		}

		[HttpPost]
		[ExceptionResult(ExceptionResultMode.OperationAction)]
		[BackendActionContext(ActionCode.AssembleTemplate)]
		[ActionAuthorize(ActionCode.AssembleTemplate)]
		[BackendActionLog]
		public virtual ActionResult Setup(int parentId, int id, bool? boundToExternal)
		{
			MultistepActionSettings settings = service.Setup(parentId, id, boundToExternal);
			return Json(settings);
		}

		[HttpPost]
		[NoTransactionConnectionScopeAttribute]
		[ExceptionResult(ExceptionResultMode.OperationAction)]
		public ActionResult Step(int stage, int step)
		{
			MultistepActionStepResult stepResult = service.Step(stage, step);
			return Json(stepResult);
		}

		[HttpPost]
		public ActionResult TearDown(bool isError)
		{
			service.TearDown();
			return null;
		}
    }
}
