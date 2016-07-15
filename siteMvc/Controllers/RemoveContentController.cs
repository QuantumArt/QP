using System.Web.Mvc;
using Quantumart.QP8.WebMvc.Extensions.ActionFilters;
using Quantumart.QP8.BLL;
using Quantumart.QP8.Constants;
using Quantumart.QP8.BLL.Services.MultistepActions;
using Quantumart.QP8.WebMvc.Extensions.Controllers;

namespace Quantumart.QP8.WebMvc.Controllers
{
    public class RemoveContentController : QPController
    {
		private readonly IMultistepActionService service;

		public RemoveContentController(IMultistepActionService service)
		{
			this.service = service;
		}

		[HttpPost]
		[ExceptionResult(ExceptionResultMode.OperationAction)]
		[ActionAuthorize(ActionCode.RemoveContent)]
		[BackendActionContext(ActionCode.RemoveContent)]
		[BackendActionLog]
		public ActionResult Setup(int parentId, int id, bool? boundToExternal)
		{
			MultistepActionSettings settings = service.Setup(parentId, id, boundToExternal);
			return Json(settings);
		}

		[HttpPost]
		[ConnectionScope()]
		[ExceptionResult(ExceptionResultMode.OperationAction)]

		public ActionResult Step(int stage, int step)
		{
			MultistepActionStepResult stepResult = service.Step(stage, step);
			return Json(stepResult);
		}

		[HttpPost]
		[BackendActionContext(ActionCode.RemoveContent)]
		[Record(ActionCode.SimpleRemoveContent, true)]
		public ActionResult TearDown(bool isError)
		{
			service.TearDown();
			return null;
		}
    }
}
