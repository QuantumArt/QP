using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Quantumart.QP8.WebMvc.Extensions.ActionFilters;
using Quantumart.QP8.BLL;
using Quantumart.QP8.Constants;
using Microsoft.Practices.Unity;
using Quantumart.QP8.BLL.Services.MultistepActions;
using Quantumart.QP8.WebMvc.Extensions.Controllers;

namespace Quantumart.QP8.WebMvc.Controllers
{
    public class RemoveSiteController : QPController
    {
		private readonly IMultistepActionService service;

		public RemoveSiteController(IMultistepActionService service)
		{
			this.service = service;
		}

		[HttpPost]
		[ExceptionResult(ExceptionResultMode.OperationAction)]
		[ActionAuthorize(ActionCode.RemoveSite)]
		[EntityAuthorize(ActionTypeCode.Remove, EntityTypeCode.Site, "id")]
		[BackendActionContext(ActionCode.RemoveSite)]
		[BackendActionLog]
		public ActionResult Setup(int parentId, int id, bool? boundToExternal)
		{
			MultistepActionSettings settings = service.Setup(parentId, id, boundToExternal);
			return Json(settings);
		}

		[HttpPost]
		[ConnectionScope(ConnectionScopeMode.TransactionOn)]
		[ExceptionResult(ExceptionResultMode.OperationAction)]
		public ActionResult Step(int stage, int step)
		{
			MultistepActionStepResult stepResult = service.Step(stage, step);
			return Json(stepResult);
		}

		[HttpPost]
		[BackendActionContext(ActionCode.RemoveSite)]
		[Record(ActionCode.SimpleRemoveSite, true)]
		public ActionResult TearDown(bool isError)
		{
			service.TearDown();
			return null;
		}
    }
}
