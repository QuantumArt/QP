using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Quantumart.QP8.BLL.Services.DTO;
using System.Threading;
using Quantumart.QP8.WebMvc.Extensions.ActionFilters;
using Quantumart.QP8.BLL;
using Quantumart.QP8.WebMvc.Extensions.Controllers;

namespace Quantumart.QP8.WebMvc.Controllers
{
    public class MutlistepTestController : QPController
    {
		[HttpPost]
		[ExceptionResult(ExceptionResultMode.OperationAction)]
		public ActionResult PreAction(int parentId, int id)
        {
			//throw new ApplicationException("Test");
			return Json(MessageResult.Confirm("Mutlistep PreAction confirm ?"));
        }

		[HttpPost]
		[ExceptionResult(ExceptionResultMode.OperationAction)]
		public ActionResult Setup(int parentId, int id)
		{
			//throw new ApplicationException("Test");

			var result = new MultistepActionSettings
			{
				Stages = new MultistepStageSettings[] 
		        {
		            new MultistepStageSettings
		            {
		                Name = "Этап 1",
		                StepCount = 10,
		                ItemCount = 100
		            },
		            new MultistepStageSettings
		            {
		                Name = "Этап 2",
		                StepCount = 5,
		                ItemCount = 50
		            },
		            new MultistepStageSettings
		            {
		                Name = "Этап 3",
		                StepCount = 0,
		                ItemCount = 0
		            },
		        }
			};
			Thread.Sleep(500);
			return Json(result);
		}

		[HttpPost]
		[ExceptionResult(ExceptionResultMode.OperationAction)]
		public ActionResult Step(int stage, int step)
		{
			Thread.Sleep(500);
			//if(stage == 2)
			//    throw new ApplicationException("Test");

			return Json(new MultistepActionStepResult { ProcessedItemsCount = 10 });
		}

		[HttpPost]
		public ActionResult TearDown(int parentId, int id)
		{
			Thread.Sleep(500);
			return Json(new { });
		}

    }
}
