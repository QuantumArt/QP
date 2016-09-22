using System;
using System.Web.Mvc;
using Quantumart.QP8.BLL.Helpers;
using Quantumart.QP8.BLL.Services.DTO;
using Quantumart.QP8.WebMvc.Extensions.ActionResults;
using Quantumart.QP8.WebMvc.Infrastructure.Enums;
using Quantumart.QP8.WebMvc.Infrastructure.Factories;

namespace Quantumart.QP8.WebMvc.Infrastructure.Helpers
{
    public static class ActionResultHelpers
    {
        public static ActionResult GererateJsonError(ExceptionResultMode responseType, Exception ex)
        {
            Ensure.NotNull(ex);

            switch (responseType)
            {
                case ExceptionResultMode.UiAction:
                    return new JsonNetResult<object>(new { success = false, message = ex.Message });
                case ExceptionResultMode.OperationAction:
                    return new JsonResult { Data = MessageResult.Error(ex.Message), JsonRequestBehavior = JsonRequestBehavior.AllowGet };
                case ExceptionResultMode.JSendResponse:
                    return JsonCamelCaseResultErrorHandlerFabric.Create(ex);

                default:
                    throw new NotImplementedException();
            }
        }
    }
}
