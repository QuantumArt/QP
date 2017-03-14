using System;
using QP8.Infrastructure.Logging;
using Quantumart.QP8.Resources;
using Quantumart.QP8.WebMvc.Infrastructure.ActionResults;
using Quantumart.QP8.WebMvc.Infrastructure.Enums;
using Quantumart.QP8.WebMvc.Infrastructure.Exceptions;
using Quantumart.QP8.WebMvc.ViewModels;

namespace Quantumart.QP8.WebMvc.Infrastructure.Factories
{
    internal class JsonCamelCaseResultErrorHandlerFabric
    {
        internal static JsonCamelCaseResult<JSendResponse> Create(Exception ex)
        {
            if (ex is XmlDbUpdateLoggingException || ex is XmlDbUpdateReplayActionException)
            {
                Logger.Log.Warn("There was an exception in XmlDbUpdateService: ", ex);

                var message = ex.Message;
                if (ex.InnerException != null)
                {
                    message = message + " " + ex.InnerException.Message;
                    if (ex.InnerException.InnerException != null)
                    {
                        message = message + " " + ex.InnerException.InnerException.Message;
                    }

                }

                return new JsonCamelCaseResult<JSendResponse>(new JSendResponse { Status = JSendStatus.Fail, Message = message });
            }

            return new JsonCamelCaseResult<JSendResponse>(new JSendResponse { Status = JSendStatus.Error, Message = GlobalStrings._500Error });
        }
    }
}
