using System;
using System.Dynamic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using NLog;
using QP8.Infrastructure.Web.Extensions;
using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Services.KeyCloak;
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
        private readonly ISsoAuthService _ssoAuthService;

        public LogOnController(AuthenticationHelper helper, ModelExpressionProvider provider, ILdapIdentityManager ldapIdentityManager, ISsoAuthService ssoAuthService)
        {
            _helper = helper;
            _provider = provider;
            _ldapIdentityManager = ldapIdentityManager;
            _ssoAuthService = ssoAuthService;
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

        [HttpGet]
        [DisableBrowserCache]
        public async Task<IActionResult> KeyCloakSsoPopup(bool? useAutoLogin, string customerCode, string returnUrl)
        {
            LogOnCredentials data = new()
            {
                CustomerCode = customerCode,
                IsPopup = true
            };

            return await KeyCloakSsoInternal(useAutoLogin, data, returnUrl);
        }

        [HttpPost]
        [DisableBrowserCache]
        public async Task<IActionResult> KeyCloakSso(bool? useAutoLogin, LogOnCredentials data, string returnUrl) => await KeyCloakSsoInternal(useAutoLogin, data, returnUrl);

        private async Task<IActionResult> KeyCloakSsoInternal(bool? useAutoLogin, LogOnCredentials data, string returnUrl)
        {
            data.IsSso = true;

            if (!await data.CheckSsoEnabled(_ssoAuthService))
            {
                return await PostIndex(useAutoLogin, data, returnUrl);
            }

            Guid state = Guid.NewGuid();
            string verifier = _ssoAuthService.GenerateCodeVerifier();
            string challenge = _ssoAuthService.GenerateCodeChallenge(verifier);

            HttpContext.Session.SetValue("KeyCloakState", state.ToString());
            HttpContext.Session.SetValue("KeyCloakChallenge", verifier);
            HttpContext.Session.SetValue("KeyCloakCustomerCode", data.CustomerCode);
            HttpContext.Session.SetValue("KeyCloakReturnUrl", returnUrl);
            HttpContext.Session.SetValue("KeyCloakPopup", data.IsPopup);

            return Redirect(_ssoAuthService.GetAuthenticateUrl(state.ToString(), challenge));
        }

        [HttpGet]
        public async Task<IActionResult> SsoCallback([FromQuery]string state, [FromQuery]string code, [FromQuery] string error)
        {
            LogOnCredentials data = new()
            {
                CustomerCode = HttpContext.Session.GetValue<string>("KeyCloakCustomerCode"),
                IsSso = true
            };

            string returnUrl = HttpContext.Session.GetValue<string>("KeyCloakReturnUrl");
            string storedState = HttpContext.Session.GetValue<string>("KeyCloakState");
            string verifier = HttpContext.Session.GetValue<string>("KeyCloakChallenge");
            bool isPopup = HttpContext.Session.GetValue<bool>("KeyCloakPopup");
            data.IsPopup = isPopup;
            await data.ValidateSso(_ssoAuthService, LogManager.GetCurrentClassLogger(), state, storedState, code, error, verifier);

            return await PostIndex(false, data, returnUrl);
        }

        private async Task<ActionResult> PostIndex(bool? useAutoLogin, LogOnCredentials data, string returnUrl)
        {
            data.UseAutoLogin = useAutoLogin ?? IsWindowsAuthentication();
            data.NtUserName = string.IsNullOrWhiteSpace(data.NtUserName) ? GetCurrentUser() : data.NtUserName;

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

                if (data.IsPopup && data.IsSso)
                {
                    dynamic result = new ExpandoObject();
                    result.success = true;
                    result.userName = data.User.Name;
                    result.isAuthenticated = true;
                    return Content($"<script>localStorage.setItem('keyCloakResult', '{JsonSerializer.Serialize(result)}'); window.close();</script>", "text/html");
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

        private string GetCurrentUser() => HttpContext.User.Identity is WindowsIdentity wi && RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? wi.Name : null;

        private bool IsWindowsAuthentication() => HttpContext.User.Identity is WindowsIdentity;

        private async Task<ActionResult> LogOnView(LogOnCredentials data)
        {
            if (data.IsPopup && data.IsSso)
            {
                data.UseAutoLogin = false;
                object result = (await JsonHtmlEscaped("Popup", data)).Value;
                string script = $"<script>localStorage.setItem('keyCloakResult', '{JsonSerializer.Serialize(result)}'); window.close();</script>";
                return Content(script, "text/html");
            }

            if (Request.IsAjaxRequest())
            {
                return await JsonHtml("Popup", data);
            }

            return View("Index", data);
        }
    }
}
