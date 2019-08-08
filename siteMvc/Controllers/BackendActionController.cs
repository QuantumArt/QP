using Microsoft.AspNetCore.Mvc;
using Quantumart.QP8.BLL.Services;
using Quantumart.QP8.WebMvc.Extensions.Controllers;
using Quantumart.QP8.WebMvc.Infrastructure.ActionFilters;
using Quantumart.QP8.WebMvc.Infrastructure.Enums;

namespace Quantumart.QP8.WebMvc.Controllers
{
    public class BackendActionController : QPController
    {
        [ExceptionResult(ExceptionResultMode.OperationAction)]
        public JsonResult GetByCode(string actionCode)
        {
            var action = BackendActionService.GetByCode(actionCode);
            return Json(new
            {
                success = true,
                action
            });
        }

        [ExceptionResult(ExceptionResultMode.OperationAction)]
        public JsonResult GetCodeById(int actionId)
        {
            var actionCode = BackendActionService.GetCodeById(actionId);
            return Json(new
            {
                success = true,
                actionCode
            });
        }

        [ExceptionResult(ExceptionResultMode.OperationAction)]
        public JsonResult GetEntityTypeIdToActionListItemsDictionary()
        {
            var dictionary = BackendActionService.GetEntityTypeIdToActionListItemsDictionary();
            return Json(new
            {
                success = true,
                dictionary
            });
        }

        [ExceptionResult(ExceptionResultMode.OperationAction)]
        public JsonResult GetStatusesList(string actionCode, string entityId, int parentEntityId, bool? boundToExternal)
        {
            var result = int.TryParse(entityId, out var idResult) ? BackendActionService.GetStatusesList(actionCode, idResult, parentEntityId) : null;
            return Json(new
            {
                success = true,
                actionStatuses = result
            });
        }
    }
}
