using System;
using QP8.Infrastructure.Extensions;
using QP8.Infrastructure.Logging;
using QP8.Infrastructure.Web.ActionResults;
using QP8.Infrastructure.Web.Enums;
using QP8.Infrastructure.Web.Responses;
using Quantumart.QP8.WebMvc.Infrastructure.Exceptions;

namespace Quantumart.QP8.WebMvc.Infrastructure.Factories
{
    internal class JsonCamelCaseResultErrorHandlerFabric
    {
        internal static JsonCamelCaseResult<JSendResponse> Create(Exception ex)
        {
            if (ex is XmlDbUpdateLoggingException || ex is XmlDbUpdateReplayActionException)
            {
                Logger.Log.Warn("There was an exception at XmlDbUpdateService: ", ex);
                return new JsonCamelCaseResult<JSendResponse>(new JSendResponse { Status = JSendStatus.Fail, Message = ex.Dump() });
            }

            Logger.Log.Error("There was an exception: ", ex);
            return new JsonCamelCaseResult<JSendResponse>(new JSendResponse { Status = JSendStatus.Error, Message = ex.Dump() });
        }
    }
}
