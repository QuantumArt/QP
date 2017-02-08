using System.Web.Mvc;
using Quantumart.QP8.BLL.Services.MultistepActions;
using Quantumart.QP8.Constants;
using Quantumart.QP8.WebMvc.Infrastructure.ActionFilters;
using Quantumart.QP8.WebMvc.Infrastructure.Enums;

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
            var template = Service.ReadTemplateProperties(parentId);
            return Json(Service.PreAction(template.SiteId, template.Id));
        }

        [HttpPost]
        [ExceptionResult(ExceptionResultMode.OperationAction)]
        [ActionAuthorize(ActionCode.AssembleTemplateFromTemplateObjectList)]
        public override ActionResult Setup(int parentId, int id, bool? boundToExternal)
        {
            var template = Service.ReadTemplateProperties(parentId);
            var settings = Service.Setup(template.SiteId, template.Id, boundToExternal);
            return Json(settings);
        }
    }
}
