using Microsoft.AspNetCore.Mvc;
using Quantumart.QP8.BLL.Services;
using Quantumart.QP8.WebMvc.Extensions.Controllers;

namespace Quantumart.QP8.WebMvc.Controllers
{
    public class BackendActionTypeController : QPController
    {
        public JsonResult GetCodeByActionCode(string actionCode)
        {
            return Json(BackendActionTypeService.GetCodeByActionCode(actionCode));
        }
    }
}
