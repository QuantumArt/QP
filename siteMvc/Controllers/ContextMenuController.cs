using Microsoft.AspNetCore.Mvc;
using Quantumart.QP8.BLL.Services;
using Quantumart.QP8.WebMvc.Extensions.Controllers;

namespace Quantumart.QP8.WebMvc.Controllers
{
    public class ContextMenuController : AuthQpController
    {
        public JsonResult GetByCode(string menuCode, bool loadItems = false)
        {
            var menu = ContextMenuService.GetByCode(menuCode, loadItems);
            return Json(menu);
        }

        public JsonResult GetStatusesList(string menuCode, int? entityId, int parentEntityId, bool? boundToExternal)
        {
            var result = entityId.HasValue
                ? ContextMenuService.GetStatusesList(menuCode, entityId.Value, parentEntityId, boundToExternal)
                : null;

            return Json(result);
        }
    }
}
