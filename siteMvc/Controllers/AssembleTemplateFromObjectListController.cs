using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Services.MultistepActions;
using Quantumart.QP8.Constants;
using Quantumart.QP8.WebMvc.Extensions.ActionFilters;
using System.Web.Mvc;

namespace Quantumart.QP8.WebMvc.Controllers
{
	public class AssembleTemplateFromObjectListController : AssembleTemplateBaseController
    {
		public AssembleTemplateFromObjectListController(IMultistepActionService service)
			: base(service)
		{			
		}

		[HttpPost]
		[ExceptionResult(ExceptionResultMode.OperationAction)]
		[ActionAuthorize(ActionCode.AssembleTemplateFromTemplateObjectList)]
		public override ActionResult PreAction(int parentId, int id)
		{
			var template = service.ReadTemplateProperties(parentId);
			return Json(service.PreAction(template.SiteId, template.Id));
		}

		[HttpPost]
		[ExceptionResult(ExceptionResultMode.OperationAction)]
		[ActionAuthorize(ActionCode.AssembleTemplateFromTemplateObjectList)]
		public override ActionResult Setup(int parentId, int id, bool? boundToExternal)
		{
			var template = service.ReadTemplateProperties(parentId);
			MultistepActionSettings settings = service.Setup(template.SiteId, template.Id, boundToExternal);
			return Json(settings);
		}
    }
}
