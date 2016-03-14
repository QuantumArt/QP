using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Services.MultistepActions;
using Quantumart.QP8.Constants;
using Quantumart.QP8.WebMvc.Extensions.ActionFilters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Quantumart.QP8.WebMvc.Controllers
{
	public class AssembleTemplateFromFormatController : AssembleTemplateBaseController
    {

		public AssembleTemplateFromFormatController(IMultistepActionService service) : base(service) {			
		}

		[HttpPost]
		[ExceptionResult(ExceptionResultMode.OperationAction)]
		[ActionAuthorize(ActionCode.AssembleTemplateFromTemplateObjectFormat)]
		public override ActionResult PreAction(int parentId, int id)
		{
			var obj = service.ReadObjectProperties(parentId);
			return Json(service.PreAction(obj.PageTemplate.SiteId, obj.PageTemplate.Id));
		}

		[HttpPost]
		[ExceptionResult(ExceptionResultMode.OperationAction)]
		[ActionAuthorize(ActionCode.AssembleTemplateFromTemplateObjectFormat)]
		public override ActionResult Setup(int parentId, int id, bool? boundToExternal)
		{
			var obj = service.ReadObjectProperties(parentId);
			MultistepActionSettings settings = service.Setup(obj.PageTemplate.SiteId, obj.PageTemplate.Id, boundToExternal);
			return Json(settings);
		}
    }
}
