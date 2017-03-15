using System;
using System.Web.Mvc;
using QP8.Infrastucture.Extensions;
using Quantumart.QP8.BLL.Helpers;
using Quantumart.QP8.BLL.Services.DTO;
using Quantumart.QP8.WebMvc.Infrastructure.ActionResults;
using Quantumart.QP8.WebMvc.Infrastructure.Enums;
using Quantumart.QP8.WebMvc.Infrastructure.Factories;

namespace Quantumart.QP8.WebMvc.Infrastructure.Helpers
{
    internal static class ActionResultHelpers
    {
        internal static ActionResult GererateJsonResultFromException(ExceptionResultMode responseType, Exception ex)
        {
            Ensure.NotNull(ex);
            switch (responseType)
            {
                case ExceptionResultMode.UiAction:
                    return new JsonNetResult<object>(new { success = false, message = ex.Dump() });
                case ExceptionResultMode.OperationAction:
                    return new JsonResult { Data = MessageResult.Error(ex.Dump()), JsonRequestBehavior = JsonRequestBehavior.AllowGet };
                case ExceptionResultMode.JSendResponse:
                    return JsonCamelCaseResultErrorHandlerFabric.Create(ex);
                default:
                    throw new NotImplementedException();
            }
        }
    }
}
