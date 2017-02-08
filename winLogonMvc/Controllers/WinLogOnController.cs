using System.Linq;
using System.Web.Mvc;
using Quantumart.QP8.BLL;
using Quantumart.QP8.Configuration;
using Quantumart.QP8.Security;
using Quantumart.QP8.WebMvc.Extensions.Controllers;
using Quantumart.QP8.WebMvc.Extensions.Exceptions;
using Quantumart.QP8.WebMvc.Extensions.Helpers;
using Quantumart.QP8.WebMvc.Infrastructure.ActionFilters;
using Quantumart.QP8.WebMvc.ViewModels.DirectLink;

namespace Quantumart.QP8.WebMvc.WinLogOn.Controllers
{
    [ValidateInput(false)]
    public class WinLogOnController : QPController
    {
        [DisableBrowserCache]
        [ResponseHeader("QP-Not-Authenticated", "True")]
        public ActionResult Index(bool? useAutoLogin, DirectLinkOptions directLinkOptions)
        {
            var data = new LogOnCredentials
            {
                UseAutoLogin = useAutoLogin ?? true,
                NtUserName = GetCurrentUser()
            };

            InitViewBag(directLinkOptions);
            return LogOnView(data);
        }

        [HttpPost]
        [DisableBrowserCache]
        public ActionResult Index(bool? useAutoLogin, DirectLinkOptions directLinkOptions, LogOnCredentials data)
        {
            data.UseAutoLogin = useAutoLogin ?? true;
            data.NtUserName = GetCurrentUser();

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

                var redirectUrl = QPConfiguration.WebConfigSection.BackendUrl;
                if (directLinkOptions != null)
                {
                    redirectUrl = directLinkOptions.AddToUrl(redirectUrl);
                }

                return Redirect(redirectUrl);
            }

            InitViewBag(directLinkOptions);
            return LogOnView(data);
        }

        [NonAction]
        private string GetCurrentUser()
        {
            return Request.ServerVariables["LOGON_USER"];
        }

        [NonAction]
        private void InitViewBag(DirectLinkOptions directLinkOptions)
        {
            ViewBag.AllowSelectCustomerCode = QPConfiguration.AllowSelectCustomerCode;
            ViewBag.CustomerCodes = QPConfiguration.CustomerCodes.Select(c => new QPSelectListItem { Text = c, Value = c }).OrderBy(n => n.Text);
            ViewBag.AutoLoginLinkQuery = "?UseAutoLogin=false";
            if (directLinkOptions != null && directLinkOptions.IsDefined())
            {
                ViewBag.AutoLoginLinkQuery += "&" + directLinkOptions.ToUrlParams();
            }
        }

        [NonAction]
        private ActionResult LogOnView(LogOnCredentials model)
        {
            return Request.IsAjaxRequest() ? JsonHtml("Popup", model) : View(model);
        }
    }
}
