using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Quantumart.QP8.BLL;
using Quantumart.QP8.Configuration;
using Quantumart.QP8.Constants.Mvc;
using Quantumart.QP8.Security;
using Quantumart.QP8.WebMvc.Extensions.Controllers;
using Quantumart.QP8.WebMvc.Extensions.Helpers;
using Quantumart.QP8.WebMvc.Infrastructure.ActionFilters;
using Quantumart.QP8.WebMvc.Infrastructure.Extensions;
using Quantumart.QP8.WebMvc.ViewModels.DirectLink;
using Refit;

namespace Quantumart.QP8.WebMvc.Controllers
{
    public class LogOnController : QPController
    {
        private AuthenticationHelper _helper;

        public LogOnController(AuthenticationHelper helper)
        {
            _helper = helper;
        }

        [DisableBrowserCache]
        [ResponseHeader(ResponseHeaders.QpNotAuthenticated, "True")]
        public async Task<ActionResult> Index(DirectLinkOptions directLinkOptions)
        {
            string userIpAddress = HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();

            if (!User.Identity.IsAuthenticated && _helper.ShouldUseWindowsAuthentication(userIpAddress))
            {
                return Redirect(GetAuthorizationUrl(directLinkOptions));
            }

            FillViewBagData();
            return await LogOnView();
        }

        [HttpPost]
        [DisableBrowserCache]
        public async Task<ActionResult> Index(DirectLinkOptions directLinkOptions, LogOnCredentials data)
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

                if (directLinkOptions != null && directLinkOptions.IsDefined())
                {
                    return RedirectToAction("Index", "Home", directLinkOptions);
                }

                return RedirectToAction("Index", "Home");
            }

            FillViewBagData();
            return await LogOnView();
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

        private string GetAuthorizationUrl(DirectLinkOptions directLinkOptions)
        {
            var url = Options.Authentication.WinLogonUrl;
            return directLinkOptions != null ? directLinkOptions.AddToUrl(url) : url;
        }

        private void FillViewBagData()
        {
            ViewBag.AllowSelectCustomerCode = QPConfiguration.AllowSelectCustomerCode;
            ViewBag.CustomerCodes = QPConfiguration.GetCustomerCodes().Select(cc => new QPSelectListItem { Text = cc, Value = cc }).OrderBy(cc => cc.Text);
        }

        private async Task<ActionResult> LogOnView()
        {
            if (Request.IsAjaxRequest())
            {
                return await JsonHtml("Popup", null);
            }
            return View();
        }
    }
}
