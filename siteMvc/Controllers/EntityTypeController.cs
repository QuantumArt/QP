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
    public class EntityTypeController : QPController
    {
		/// <summary>
		/// Возвращает тип сущности по ее коду
		/// </summary>
		/// <param name="entityTypeCode">код типа сущности</param>
		/// <returns>информация о типе сущности</returns>
		[HttpGet]
		public JsonResult GetByCode(string entityTypeCode)
		{
			EntityType entityType = EntityTypeService.GetByCode(entityTypeCode);
			return Json(entityType, JsonRequestBehavior.AllowGet);
		}

		[HttpGet]
		public JsonResult GetCodeById(int entityTypeId)
		{
			string code = EntityTypeService.GetCodeById(entityTypeId);
			return Json(code, JsonRequestBehavior.AllowGet);
		}

		[HttpGet]
		/// <summary>
		/// Возвращает код типа родительской сущности
		/// </summary>
		/// <param name="entityTypeCode">код типа сущности</param>
		/// <returns>код типа родительской сущности</returns>
		public JsonResult GetParentCodeByCode(string entityTypeCode)
		{
			string parentEntityTypeCode = EntityTypeService.GetParentCodeByCode(entityTypeCode);

			return Json(parentEntityTypeCode, JsonRequestBehavior.AllowGet);
		}
    }
}