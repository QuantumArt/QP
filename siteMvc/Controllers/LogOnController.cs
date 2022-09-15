using System;
using System.Linq;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Quantumart.QP8.BLL;
using Quantumart.QP8.Configuration;
using Quantumart.QP8.Constants.Mvc;
using Quantumart.QP8.Security;
using Quantumart.QP8.Security.Ldap;
using Quantumart.QP8.WebMvc.Extensions.Controllers;
using Quantumart.QP8.WebMvc.Extensions.Helpers;
using Quantumart.QP8.WebMvc.Infrastructure.ActionFilters;
using Quantumart.QP8.WebMvc.Infrastructure.Extensions;
using Quantumart.QP8.WebMvc.ViewModels.DirectLink;

namespace Quantumart.QP8.WebMvc.Controllers
{
    public class LogOnController : QPController
    {
        private AuthenticationHelper _helper;
        private ModelExpressionProvider _provider;
        private readonly ILdapIdentityManager _ldapIdentityManager;

        public LogOnController(AuthenticationHelper helper, ModelExpressionProvider provider, ILdapIdentityManager ldapIdentityManager)
        {
            _helper = helper;
            _provider = provider;
            _ldapIdentityManager = ldapIdentityManager;
        }

        [DisableBrowserCache]
        [ResponseHeader(ResponseHeaders.QpNotAuthenticated, "True")]
        public async Task<ActionResult> Index(bool? useAutoLogin, DirectLinkOptions directLinkOptions)
        {
            var data = new LogOnCredentials
            {
                UseAutoLogin = useAutoLogin ?? IsWindowsAuthentication(),
                NtUserName = GetCurrentUser()
            };

            FillViewBagData(directLinkOptions);
            return await LogOnView(data);
        }

        [HttpPost]
        [DisableBrowserCache]
        public async Task<ActionResult> Index(bool? useAutoLogin, LogOnCredentials data, string returnUrl)
        {
            return await PostIndex(useAutoLogin, data, returnUrl);
        }

        [HttpPost]
        [DisableBrowserCache]
        public async Task<ActionResult> JsonIndex(bool? useAutoLogin, [FromBody] LogOnCredentials data, string returnUrl)
        {
            return await PostIndex(useAutoLogin, data, returnUrl);
        }

        private async Task<ActionResult> PostIndex(bool? useAutoLogin, LogOnCredentials data, string returnUrl)
        {
            data.UseAutoLogin = useAutoLogin ?? IsWindowsAuthentication();
            data.NtUserName = GetCurrentUser();

            try
            {
                data.Validate(_ldapIdentityManager);
            }
            catch (RulesException ex)
            {
                ex.Extend(ModelState, _provider);
            }

            if (ModelState.IsValid && data.User != null)
            {
                _helper.SignIn(data.User);
                if (Request.IsAjaxRequest())
                {
                    return Json(new
                    {
                        success = true,
                        isAuthenticated = true,
                        userName = data.User.Name
                    });
                }

                if (!string.IsNullOrEmpty(returnUrl))
                {
                    return LocalRedirect(returnUrl);
                }

                return RedirectToAction("Index", "Home");
            }

            FillViewBagData();
            return await LogOnView(data);
        }

        [DisableBrowserCache]
        public ActionResult LogOut(DirectLinkOptions directLinkOptions)
        {
            QPContext.LogOut();
            return RedirectToAction("Index");
        }

        private void FillViewBagData(DirectLinkOptions directLinkOptions = null)
        {
            ViewBag.AllowSelectCustomerCode = QPConfiguration.AllowSelectCustomerCode;
            ViewBag.CustomerCodes = QPConfiguration.GetCustomerCodes().Select(cc => new QPSelectListItem { Text = cc, Value = cc }).OrderBy(cc => cc.Text);

            ViewBag.AutoLoginLinkQuery = "?useAutoLogin=false";
            if (directLinkOptions != null && directLinkOptions.IsDefined())
            {
                ViewBag.AutoLoginLinkQuery += "&" + directLinkOptions.ToUrlParams();
            }
        }

        private string GetCurrentUser() => HttpContext.User.Identity is WindowsIdentity wi ? wi.Name : null;

        private bool IsWindowsAuthentication() => HttpContext.User.Identity is WindowsIdentity;

        private async Task<ActionResult> LogOnView(LogOnCredentials data)
        {
            if (Request.IsAjaxRequest())
            {
                return await JsonHtml("Popup", data);
            }
            return View("Index", data);
        }
    }
}
