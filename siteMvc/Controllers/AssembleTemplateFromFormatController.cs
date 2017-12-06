using System.Web.Mvc;
using Quantumart.QP8.BLL.Services.MultistepActions;
using Quantumart.QP8.Constants;
using Quantumart.QP8.WebMvc.Infrastructure.ActionFilters;
using Quantumart.QP8.WebMvc.Infrastructure.Enums;

namespace Quantumart.QP8.WebMvc.Controllers
{
    public class AssembleTemplateFromFormatController : AssembleTemplateBaseController
    {
        public AssembleTemplateFromFormatController(IMultistepActionService service)
            : base(service)
        {
        }

        [HttpPost]
        [ExceptionResult(ExceptionResultMode.OperationAction)]
        [ActionAuthorize(ActionCode.AssembleTemplateFromTemplateObjectFormat)]
        public override ActionResult PreAction(int parentId, int id)
        {
            var obj = Service.ReadObjectProperties(parentId);
            return Json(Service.PreAction(obj.PageTemplate.SiteId, obj.PageTemplate.Id));
        }

        [HttpPost]
        [ExceptionResult(ExceptionResultMode.OperationAction)]
        [ActionAuthorize(ActionCode.AssembleTemplateFromTemplateObjectFormat)]
        public override ActionResult Setup(int parentId, int id, bool? boundToExternal)
        {
            var obj = Service.ReadObjectProperties(parentId);
            var settings = Service.Setup(obj.PageTemplate.SiteId, obj.PageTemplate.Id, boundToExternal);
            return Json(settings);
        }
    }
}
