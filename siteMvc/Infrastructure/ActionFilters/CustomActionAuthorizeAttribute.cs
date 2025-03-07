using System;
using System.Globalization;
using System.Security;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;
using QP.ConfigurationService.Models;
using QP8.Infrastructure.Extensions;
using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Services;
using Quantumart.QP8.Resources;
using Quantumart.QP8.Security;
using Quantumart.QPublishing.Database;
using Quantumart.QPublishing.OnScreen;

namespace Quantumart.QP8.WebMvc.Infrastructure.ActionFilters;

public class CustomActionAuthorizeAttribute : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        if (!context.HttpContext.Request.Form.TryGetValue("backend_sid", out StringValues backendSid))
        {
            throw new SecurityException { Data = { { ExceptionHelpers.ClientMessageKey, CustomActionStrings.ActionNotAccessible } } };
        }

        DBConnector connector = new(QPContext.CurrentDbConnectionInfo.ConnectionString, (DatabaseType)QPContext.CurrentDbConnectionInfo.DbType);
        QScreen screen = new(connector);
        int userId = screen.AuthenticateForCustomTab(connector, backendSid);

        if (userId <= 0)
        {
            throw new SecurityException { Data = { { ExceptionHelpers.ClientMessageKey, CustomActionStrings.ActionNotAccessible } } };
        }

        User user;

        try
        {
            IUserService userService = context.HttpContext.RequestServices.GetRequiredService<IUserService>();
            user = userService.ReadProfile(userId);
        }
        catch (Exception)
        {
            throw new SecurityException { Data = { { ExceptionHelpers.ClientMessageKey, CustomActionStrings.ActionNotAccessible } } };
        }

        QPContext.CurrentUserId = userId;

        string langName = QpUser.GetCultureNameByLanguageId(user.LanguageId);

        CultureInfo.CurrentCulture = new(langName);
        CultureInfo.CurrentUICulture = new(langName);

        base.OnActionExecuting(context);
    }
}
