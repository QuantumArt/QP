using System;
using Quantumart.QP8.BLL.Services;
using Quantumart.QP8.Resources;
using Quantumart.QP8.WebMvc.Extensions.ActionResults;
using Quantumart.QP8.WebMvc.Infrastructure.Enums;
using Quantumart.QP8.WebMvc.Infrastructure.Exceptions;
using Quantumart.QP8.WebMvc.ViewModels;

namespace Quantumart.QP8.WebMvc.Infrastructure.Factories
{
    internal class JsonCamelCaseResultFabric
    {
        internal static JsonCamelCaseResult<JSendResponse> Create(Exception ex)
        {
            if (ex is XmlDbUpdateLoggingException || ex is XmlDbUpdateReplayActionException)
            {
                Logger.Log.Warn("There was an exception in XmlDbUpdateService: ", ex);
                return new JsonCamelCaseResult<JSendResponse>(new JSendResponse { Status = JSendStatus.Fail, Message = ex.Message });
            }

            return new JsonCamelCaseResult<JSendResponse>(new JSendResponse { Status = JSendStatus.Error, Message = GlobalStrings._500Error });
        }
    }
}
