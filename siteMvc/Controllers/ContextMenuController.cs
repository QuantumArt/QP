using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Quantumart.QP8;
using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Services;
using Quantumart.QP8.Constants;
using Quantumart.QP8.WebMvc.Extensions.Controllers;

namespace Quantumart.QP8.WebMvc.Controllers
{
    public class ContextMenuController : QPController
    {
		[HttpGet]
		public JsonResult GetByCode(string menuCode, bool loadItems = false)
		{
			ContextMenu menu = ContextMenuService.GetByCode(menuCode, loadItems);

			return Json(menu, JsonRequestBehavior.AllowGet);
		}

		[HttpGet]
		public JsonResult GetStatusesList(string menuCode, int? entityId, int parentEntityId, bool? boundToExternal)
		{
			IEnumerable<BackendActionStatus> result;			
			if (entityId.HasValue)
			{
				result = ContextMenuService.GetStatusesList(menuCode, entityId.Value, parentEntityId, boundToExternal);				
			}
			else
				result = null;
			return Json(result, JsonRequestBehavior.AllowGet);
		}
    }


}