using System.Web.Mvc;
using Quantumart.QP8.WebMvc.Extensions.ActionFilters;
using Quantumart.QP8.Constants;
using Quantumart.QP8.BLL.Services;
using Quantumart.QP8.BLL;
using Quantumart.QP8.WebMvc.Extensions.Controllers;

namespace Quantumart.QP8.WebMvc.Controllers
{
    public class FieldDefaultValueController : QPController
    {
		private readonly IFieldDefaultValueService service;

		public FieldDefaultValueController(IFieldDefaultValueService service)
		{
			this.service = service;
		}

		[HttpPost]
		[ExceptionResult(ExceptionResultMode.OperationAction)]
		[ActionAuthorize(ActionCode.ApplyFieldDefaultValue)]
		[BackendActionContext(ActionCode.ApplyFieldDefaultValue)]
		[BackendActionLog]
		public ActionResult PreAction(int parentId, int id)
		{
			return Json(service.PreAction(id));
		}

		[HttpPost]
		[ExceptionResult(ExceptionResultMode.OperationAction)]
		[ActionAuthorize(ActionCode.ApplyFieldDefaultValue)]
		public ActionResult Setup(int parentId, int id)
		{
			MultistepActionSettings settings = service.SetupAction(parentId, id);
			return Json(settings);
		}

		[HttpPost]
		[ExceptionResult(ExceptionResultMode.OperationAction)]
		[ActionAuthorize(ActionCode.ApplyFieldDefaultValue)]
		[ConnectionScope()]
		public ActionResult Step(int step)
		{
			MultistepActionStepResult stepResult = service.Step(step);
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
