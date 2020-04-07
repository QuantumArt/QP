using Microsoft.AspNetCore.Mvc;
using Quantumart.QP8.BLL.Services;
using Quantumart.QP8.WebMvc.Extensions.Controllers;

namespace Quantumart.QP8.WebMvc.Controllers
{
    public class EntityTypeController : AuthQpController
    {
        public JsonResult GetByCode(string entityTypeCode)
        {
            var entityType = EntityTypeService.GetByCode(entityTypeCode);
            return Json(entityType);
        }

        public JsonResult GetCodeById(int entityTypeId)
        {
            var code = EntityTypeService.GetCodeById(entityTypeId);
            return Json(code);
        }

        public JsonResult GetParentCodeByCode(string entityTypeCode)
        {
            var parentEntityTypeCode = EntityTypeService.GetParentCodeByCode(entityTypeCode);
            return Json(parentEntityTypeCode);
        }
    }
}
