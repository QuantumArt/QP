using System.Linq;
using System.Web.Mvc;
using Quantumart.QP8.BLL;
using Quantumart.QP8.Configuration;
using Quantumart.QP8.Constants.Mvc;
using Quantumart.QP8.Security;
using Quantumart.QP8.WebMvc.Extensions.Controllers;
using Quantumart.QP8.WebMvc.Extensions.Helpers;
using Quantumart.QP8.WebMvc.Infrastructure.ActionFilters;
using Quantumart.QP8.WebMvc.Infrastructure.Extensions;
using Quantumart.QP8.WebMvc.ViewModels.DirectLink;

namespace Quantumart.QP8.WebMvc.Controllers
{
    [ValidateInput(false)]
    public class LogOnController : QPController
    {
        [DisableBrowserCache]
        [ResponseHeader(ResponseHeaders.QpNotAuthenticated, "True")]
        public ActionResult Index(DirectLinkOptions directLinkOptions)
        {
            if (!Request.IsAuthenticated && AuthenticationHelper.ShouldUseWindowsAuthentication(Request.UserHostAddress))
            {
                return Redirect(GetAuthorizationUrl(directLinkOptions));
            }

            FillViewBagData();
            return LogOnView();
        }

        [HttpPost]
        [DisableBrowserCache]
        public ActionResult Index(DirectLinkOptions directLinkOptions, LogOnCredentials data)
        {
            try
            {
                data.Validate();
            }
            catch (RulesException ex)
            {
                ex.Extend(ModelState);
            }

            if (ModelState.IsValid && data.User != null)
            {
                AuthenticationHelper.CompleteAuthentication(data.User);
                if (Request.IsAjaxRequest())
                {
                    return Json(new
                    {
                        success = true,
                        isAuthenticated = true,
                        userName = data.User.Name
                    });
                }

                if (directLinkOptions != null && directLinkOptions.IsDefined())
                {
                    return RedirectToAction("Index", "Home", directLinkOptions);
                }

                return RedirectToAction("Index", "Home");
            }

            FillViewBagData();
            return LogOnView();
        }

        [DisableBrowserCache]
        public ActionResult LogOut(DirectLinkOptions directLinkOptions)
        {
            var loginUrl = QPContext.LogOut();
            if (directLinkOptions != null)
            {
                loginUrl = directLinkOptions.AddToUrl(loginUrl);
            }

            return Redirect(loginUrl);
        }

        private static string GetAuthorizationUrl(DirectLinkOptions directLinkOptions)
        {
            // Если IP-адрес пользователя входит в диапазон IP-адресов
            // внутренней сети, то перенаправляем его на страницу Windows-аутентификации
            return directLinkOptions != null ? directLinkOptions.AddToUrl(AuthenticationHelper.WindowsAuthenticationUrl) : AuthenticationHelper.WindowsAuthenticationUrl;
        }

        private void FillViewBagData()
        {
            ViewBag.AllowSelectCustomerCode = QPConfiguration.AllowSelectCustomerCode;
            ViewBag.CustomerCodes = QPConfiguration.CustomerCodes.Select(cc => new QPSelectListItem { Text = cc, Value = cc }).OrderBy(cc => cc.Text);
        }

        private ActionResult LogOnView()
        {
            return Request.IsAjaxRequest() ? JsonHtml("Popup", null) : View();
        }
    }
}
