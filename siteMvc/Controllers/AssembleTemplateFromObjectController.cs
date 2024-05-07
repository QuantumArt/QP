using Microsoft.AspNetCore.Mvc;
using Quantumart.QP8.BLL.Services.MultistepActions.Assemble;
using Quantumart.QP8.Configuration;
using Quantumart.QP8.Constants;
using Quantumart.QP8.WebMvc.Infrastructure.ActionFilters;
using Quantumart.QP8.WebMvc.Infrastructure.Enums;

namespace Quantumart.QP8.WebMvc.Controllers
{
    public class AssembleTemplateFromObjectController : AssembleTemplateBaseController
    {
        public AssembleTemplateFromObjectController(AssembleTemplateService service)
            : base(service)
        {
        }

        [HttpPost]
        [ExceptionResult(ExceptionResultMode.OperationAction)]
        [ActionAuthorize(ActionCode.AssembleTemplateFromTemplateObject)]
        public override ActionResult PreAction(int parentId, int id)
        {
            var template = Service.ReadTemplateProperties(parentId);
            return Json(Service.PreAction(template.SiteId, template.Id));
        }

        [HttpPost]
        [ExceptionResult(ExceptionResultMode.OperationAction)]
        [ActionAuthorize(ActionCode.AssembleTemplateFromTemplateObject)]
        public override ActionResult Setup(int parentId, int id, bool? boundToExternal)
        {
            var template = Service.ReadTemplateProperties(parentId);
            var settings = Service.Setup(template.SiteId, template.Id, boundToExternal, new S3Options());
            return Json(settings);
        }
    }
}
