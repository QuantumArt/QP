using System;
using System.Web.Mvc;
using Quantumart.QP8.BLL.Services.DTO;
using Quantumart.QP8.Resources;
using Quantumart.QP8.WebMvc.Extensions.ActionFilters;
using Quantumart.QP8.WebMvc.Extensions.ActionResults;
using Quantumart.QP8.WebMvc.Infrastructure.Enums;
using Quantumart.QP8.WebMvc.ViewModels;

namespace Quantumart.QP8.WebMvc.Infrastructure.Helpers
{
    public static class ActionResultHelpers
    {
        public static ActionResult GererateJsonError(ExceptionResultMode responseType, Exception ex)
        {
            switch (responseType)
            {
                case ExceptionResultMode.UiAction:
                    return new JsonNetResult<object>(new { success = false, message = ex.Message });
                case ExceptionResultMode.OperationAction:
                    return new JsonResult { Data = MessageResult.Error(ex.Message), JsonRequestBehavior = JsonRequestBehavior.AllowGet };
                case ExceptionResultMode.JSendResponse:
                    return new JsonCamelCaseResult<JSendResponse>(new JSendResponse { Status = JSendStatus.Error, Message = GlobalStrings._500Error });
                default:
                    throw new NotImplementedException();
            }
        }
    }
}
