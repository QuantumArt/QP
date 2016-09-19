using System.Web.Mvc;
using Quantumart.QP8.BLL.Services;
using Quantumart.QP8.WebMvc.Extensions.Controllers;

namespace Quantumart.QP8.WebMvc.Controllers
{
    public class EntityTypeController : QPController
    {
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
        public JsonResult GetParentCodeByCode(string entityTypeCode)
        {
            var parentEntityTypeCode = EntityTypeService.GetParentCodeByCode(entityTypeCode);
            return Json(parentEntityTypeCode, JsonRequestBehavior.AllowGet);
        }
    }
}
