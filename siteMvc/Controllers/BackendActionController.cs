using System;
using System.Collections.Generic;
using System.Web.Mvc;
using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Services;
using Quantumart.QP8.BLL.Services.DTO;
using Quantumart.QP8.WebMvc.Extensions.ActionFilters;
using Quantumart.QP8.WebMvc.Extensions.Controllers;

namespace Quantumart.QP8.WebMvc.Controllers
{
    public class BackendActionController : QPController
    {
        [HttpGet]
        [ExceptionResult(ExceptionResultMode.OperationAction)]
        public JsonResult GetByCode(string actionCode)
        {
            BackendAction action = BackendActionService.GetByCode(actionCode);
            return new JsonResult
            {
                Data = new
                {
                    success = true,
                    action = action
                },
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };            
        }

        [HttpGet]
        [ExceptionResult(ExceptionResultMode.OperationAction)]
        public JsonResult GetCodeById(int actionId)
        {
            string actionCode = BackendActionService.GetCodeById(actionId);
            return new JsonResult
            {
                Data = new
                {
                    success = true,
                    actionCode = actionCode
                },
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            }; 
        }

        [HttpGet]
        [ExceptionResult(ExceptionResultMode.OperationAction)]
        public JsonResult GetEntityTypeIdToActionListItemsDictionary()
        {
            IEnumerable<EntityTypeIdToActionListItemPair> dictionary = BackendActionService.GetEntityTypeIdToActionListItemsDictionary();
            return new JsonResult
            {
                Data = new
                {
                    success = true,
                    dictionary = dictionary
                },
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        [HttpGet]
        [ExceptionResult(ExceptionResultMode.OperationAction)]
        public JsonResult GetStatusesList(string actionCode, string entityId, int parentEntityId, bool? boundToExternal)
        {
            IEnumerable<BackendActionStatus> result;
            int idResult = 0;
            if (Int32.TryParse(entityId, out idResult))
                result = BackendActionService.GetStatusesList(actionCode, idResult, parentEntityId);
            else
                result = null;
            
            return new JsonResult
            {
                Data = new
                {
                    success = true,
                    actionStatuses = result
                },
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }
    }
}