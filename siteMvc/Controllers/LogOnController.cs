using System.Linq;
using System.Web.Mvc;
using Quantumart.QP8.BLL;
using Quantumart.QP8.Configuration;
using Quantumart.QP8.Security;
using Quantumart.QP8.WebMvc.Extensions.ActionFilters;
using Quantumart.QP8.WebMvc.Extensions.Controllers;
using Quantumart.QP8.WebMvc.Extensions.Helpers;
using Quantumart.QP8.WebMvc.ViewModels.DirectLink;

namespace Quantumart.QP8.WebMvc.Controllers
{
    [ValidateInput(false)]
    public class LogOnController : QPController
    {
        [HttpGet]
        [DisableBrowserCache]
        [ResponseHeader("QP-Not-Authenticated", "True")]
        public ActionResult Index(DirectLinkOptions directLinkOptions)
        {
            if (!Request.IsAuthenticated && AuthenticationHelper.ShouldUseWindowsAuthentication(Request.UserHostAddress))
            {
                // Если IP-адрес пользователя входит в диапазон IP-адресов
                // внутренней сети, то перенаправляем его на страницу Windows-аутентификации
                return Redirect(directLinkOptions != null
                    ? directLinkOptions.AddToUrl(AuthenticationHelper.WindowsAuthenticationUrl)
                    : AuthenticationHelper.WindowsAuthenticationUrl);
            }

            InitViewBag();
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
                ex.CopyTo(ModelState);
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

            InitViewBag();
            return LogOnView();
        }

        [HttpGet]
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

        private void InitViewBag()
        {
            ViewBag.AllowSelectCustomerCode = QPConfiguration.AllowSelectCustomerCode;
            ViewBag.CustomerCodes = QPConfiguration.CustomerCodes.Select(c => new QPSelectListItem { Text = c, Value = c }).OrderBy(n => n.Text);
        }

        private ActionResult LogOnView()
        {
            return Request.IsAjaxRequest()
                ? JsonHtml("Popup", null)
                : View();
        }
    }
}
