using System.Net;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using NLog;
using NLog.Fluent;
using QP8.Infrastructure.Extensions;
using Quantumart.QP8.Resources;

namespace Quantumart.QP8.WebMvc
{
    public class GlobalExceptionHandler
    {
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

        public void Action(IApplicationBuilder options)
        {
            options.Run(async context =>
            {
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                context.Response.ContentType = "text/html";
                var ex = context.Features.Get<IExceptionHandlerFeature>();
                if (ex != null)
                {
                    Logger.ForErrorEvent()
                         .Exception(ex.Error)
                         .Message("Unhandled exception occurs")
                         .Log();
                    var message = ex.Error.Data[ExceptionHelpers.ClientMessageKey] ?? GlobalStrings._500Error;
                    var err = $"<h1>Error: {message}</h1>";
                    await context.Response.WriteAsync(err).ConfigureAwait(false);
                }
            });
        }
    }
}
