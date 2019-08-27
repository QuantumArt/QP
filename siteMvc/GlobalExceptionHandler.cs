using System.Net;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Quantumart.QP8.WebMvc
{
    public class GlobalExceptionHandler
    {
        private readonly ILoggerFactory _factory;
        public GlobalExceptionHandler(ILoggerFactory factory)
        {
            _factory = factory;
        }

        public void Action(IApplicationBuilder options)
        {
            options.Run(async context =>
            {
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                context.Response.ContentType = "text/html";
                var ex = context.Features.Get<IExceptionHandlerFeature>();
                if (ex != null)
                {
                    var logger = _factory.CreateLogger("Global Exception Handling");
                    LoggerExtensions.LogError(logger, new EventId(1), ex.Error, "Unhandled exception occurs");
                    var err = $"<h1>Error: {ex.Error.Message}</h1>{ex.Error.StackTrace}";
                    await context.Response.WriteAsync(err).ConfigureAwait(false);
                }
            });
        }
    }
}
