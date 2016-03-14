using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Quantumart.QP8.BLL.Services.MultistepActions;
using Microsoft.Practices.Unity;
using Quantumart.QP8.WebMvc.Extensions.ActionFilters;
using Quantumart.QP8.Constants;
using Quantumart.QP8.BLL;
using Quantumart.QP8.WebMvc.Extensions.Controllers;

namespace Quantumart.QP8.WebMvc.Controllers
{
    public class AssembleSiteController : QPController
    {
        private readonly IMultistepActionService service;

		public AssembleSiteController(IMultistepActionService service)
		{
			this.service = service;
		}

		[HttpPost]
		[ExceptionResult(ExceptionResultMode.OperationAction)]
		[ActionAuthorize(ActionCode.AssembleSite)]
		public ActionResult PreAction(int parentId, int id)
		{
			return Json(service.PreAction(parentId, id));
		}

		[HttpPost]
		[ExceptionResult(ExceptionResultMode.OperationAction)]
		[ActionAuthorize(ActionCode.AssembleSite)]
		[BackendActionContext(ActionCode.AssembleSite)]
		[BackendActionLog]
		public ActionResult Setup(int parentId, int id, bool? boundToExternal)
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
