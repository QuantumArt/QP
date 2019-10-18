using System.Net;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using NLog;
using NLog.Fluent;

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
                     Logger.Error()
                         .Exception(ex.Error)
                         .Message("Unhandled exception occurs")
                         .Write();

                    var err = $"<h1>Error: {ex.Error.Message}</h1>{ex.Error.StackTrace}";
                    await context.Response.WriteAsync(err).ConfigureAwait(false);
                }
            });
        }
    }
}
