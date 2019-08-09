using Microsoft.AspNetCore.Mvc;
using Quantumart.QP8.BLL.Services;
using Quantumart.QP8.WebMvc.Extensions.Controllers;

namespace Quantumart.QP8.WebMvc.Controllers
{
    public class ToolbarController : QPController
    {
        public JsonResult GetToolbarButtonListByActionCode(string actionCode, string entityId, string parentEntityId, bool? boundToExternal)
        {
            int.TryParse(entityId, out var parsedId);
            int.TryParse(parentEntityId, out var parsedParentId);

            var buttonsList = ToolbarService.GetButtonListByActionCode(actionCode, parsedId, parsedParentId, boundToExternal);
            return Json(buttonsList);
        }
    }
}
