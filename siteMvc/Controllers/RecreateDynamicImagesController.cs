using System.Web.Mvc;
using Quantumart.QP8.WebMvc.Extensions.ActionFilters;
using Quantumart.QP8.Constants;
using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Services;
using Quantumart.QP8.WebMvc.Extensions.Controllers;

namespace Quantumart.QP8.WebMvc.Controllers
{
    public class RecreateDynamicImagesController : QPController
    {
		private readonly IRecreateDynamicImagesService service;

		public RecreateDynamicImagesController(IRecreateDynamicImagesService service)
		{
			this.service = service;
		}

		[HttpPost]
		[ExceptionResult(ExceptionResultMode.OperationAction)]
		[ActionAuthorize(ActionCode.RecreateDynamicImages)]
		[BackendActionContext(ActionCode.RecreateDynamicImages)]
		[BackendActionLog]
		public ActionResult Setup(int parentId, int id)
		{
			MultistepActionSettings settings = service.SetupAction(parentId, id);
			return Json(settings);
		}

		[HttpPost]
		[ExceptionResult(ExceptionResultMode.OperationAction)]
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
