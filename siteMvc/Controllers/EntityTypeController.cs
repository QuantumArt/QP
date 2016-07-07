using Quantumart.QP8.BLL.Services;
using Quantumart.QP8.WebMvc.Extensions.Controllers;
using System.Web.Mvc;

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
            var entityType = EntityTypeService.GetByCode(entityTypeCode);
            return Json(entityType, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetCodeById(int entityTypeId)
        {
            var code = EntityTypeService.GetCodeById(entityTypeId);
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
            var parentEntityTypeCode = EntityTypeService.GetParentCodeByCode(entityTypeCode);
            return Json(parentEntityTypeCode, JsonRequestBehavior.AllowGet);
        }
    }
}
