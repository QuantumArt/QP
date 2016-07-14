using System;
using System.Collections.Generic;
using System.Web.Mvc;
using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Services;
using Quantumart.QP8.WebMvc.Extensions.Controllers;

namespace Quantumart.QP8.WebMvc.Controllers
{
    public class ToolbarController : QPController
    {
        [HttpGet]
        public JsonResult GetToolbarButtonListByActionCode(string actionCode, string entityId, string parentEntityId, bool? boundToExternal)
        {
			int parsedId = 0;
			int parsedParentId = 0;
			Int32.TryParse(entityId, out parsedId);
			Int32.TryParse(parentEntityId, out parsedParentId);
			

			IEnumerable<ToolbarButton> buttonsList = ToolbarService.GetButtonListByActionCode(actionCode, parsedId, parsedParentId, boundToExternal);
			return Json(buttonsList, JsonRequestBehavior.AllowGet);
        }
    }
}