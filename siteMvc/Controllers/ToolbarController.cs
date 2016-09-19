using System.Web.Mvc;
using Quantumart.QP8.BLL.Services;
using Quantumart.QP8.WebMvc.Extensions.Controllers;

namespace Quantumart.QP8.WebMvc.Controllers
{
    public class ToolbarController : QPController
    {
        [HttpGet]
        public JsonResult GetToolbarButtonListByActionCode(string actionCode, string entityId, string parentEntityId, bool? boundToExternal)
        {
            int parsedId;
            int parsedParentId;
            int.TryParse(entityId, out parsedId);
            int.TryParse(parentEntityId, out parsedParentId);

            var buttonsList = ToolbarService.GetButtonListByActionCode(actionCode, parsedId, parsedParentId, boundToExternal);
            return Json(buttonsList, JsonRequestBehavior.AllowGet);
        }
    }
}
