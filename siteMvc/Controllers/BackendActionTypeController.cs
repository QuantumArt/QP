using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Quantumart.QP8;
using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Services;
using Quantumart.QP8.WebMvc.Extensions.Controllers;

namespace Quantumart.QP8.WebMvc.Controllers
{
    public class BackendActionTypeController : QPController
    {
		/// <summary>
		/// Возвращает код типа действия по коду действия
		/// </summary>
		/// <param name="actionCode">код действия</param>
		/// <returns>код типа действия</returns>
		[HttpGet]
		public JsonResult GetCodeByActionCode(string actionCode)
		{
			string actionTypeCode = BackendActionTypeService.GetCodeByActionCode(actionCode);

			return Json(actionTypeCode, JsonRequestBehavior.AllowGet);
		}
    }
}
