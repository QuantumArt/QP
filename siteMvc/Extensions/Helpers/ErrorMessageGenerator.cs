using Quantumart.QP8.BLL.Services.DTO;
using Quantumart.QP8.Resources;
using Quantumart.QP8.WebMvc.Extensions.ActionFilters;
using Quantumart.QP8.WebMvc.Extensions.ActionResults;
using Quantumart.QP8.WebMvc.ViewModels;
using System;
using System.Web.Mvc;

namespace Quantumart.QP8.WebMvc.Extensions.Helpers
{
    public class ErrorMessageGenerator
    {
        public static ActionResult GererateJsonError(ExceptionResultMode responseType, Exception ex)
        {
            ActionResult result = null;

            switch (responseType)
            {
                case ExceptionResultMode.UiAction:
                    result = new JsonNetResult<object>(new { success = false, message = ex.Message });
                    break;
                case ExceptionResultMode.OperationAction:
                    result = new JsonResult { Data = MessageResult.Error(ex.Message), JsonRequestBehavior = JsonRequestBehavior.AllowGet };
                    break;
                case ExceptionResultMode.JSendResponse:
                    result = new JsonCamelCaseResult<JSendResponse>(new JSendResponse { Status = JSendStatus.Error, Message = GlobalStrings._500Error });
                    break;
                default:
                    break;
            }


            return result;
        }
    }
}
