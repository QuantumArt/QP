using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Quantumart.QP8.BLL;
using Quantumart.QP8.WebMvc.Infrastructure.Constants;

namespace Quantumart.QP8.WebMvc.Infrastructure.Middleware
{
    public class LoggingMiddleware
    {
        private readonly RequestDelegate _next;

        public LoggingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            using IDisposable logScope = NLog.ScopeContext.PushProperties(new Dictionary<string, object>()
            {
                { LoggingAttributeConstants.UserIdAttribute, QPContext.CurrentUserId },
                { LoggingAttributeConstants.UserNameAttribute, QPContext.CurrentUserName },
                { LoggingAttributeConstants.UserIpAttribute, QPContext.GetUserIpAddress() },
                { LoggingAttributeConstants.UserGroupIdsAttribute, string.Join(",", QPContext.CurrentGroupIds) }
            });

            await _next.Invoke(context);
        }
    }
}
