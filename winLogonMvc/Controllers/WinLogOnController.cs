using System.Linq;
using System.Web.Mvc;
using Quantumart.QP8.Configuration;
using Quantumart.QP8.Security;
using Quantumart.QP8.BLL;
using Quantumart.QP8.WebMvc.Extensions.ActionFilters;
using Quantumart.QP8.WebMvc.Extensions.Controllers;
using Quantumart.QP8.WebMvc.ViewModels.DirectLink;
using Quantumart.QP8.WebMvc.Extensions.Helpers;


namespace Quantumart.QP8.WebMvc.WinLogOn.Controllers
{
    [ValidateInput(false)]
    public class WinLogOnController : QPController
    {
        /// <summary>
        /// Выводит форму ввода логина и пароля
        /// </summary>
        [DisableBrowserCache]
        [ResponseHeader("QP-Not-Authenticated", "True")]
        public ActionResult Index(bool? useAutoLogin, DirectLinkOptions directLinkOptions)
        {
            LogOnCredentials data = new LogOnCredentials
            {
                UseAutoLogin = useAutoLogin ?? true,
                NtUserName = GetCurrentUser()
            };
            InitViewBag(directLinkOptions);			
            return LogOnView(data);
        }

        /// <summary>
        /// Аутентифицирует пользователя
        /// </summary>
        /// <param name="directLinkOptions"></param>
        /// <param name="data">данные формы</param>
        /// <param name="useAutoLogin"></param>
        [HttpPost]
        [DisableBrowserCache]
        public ActionResult Index(bool? useAutoLogin, DirectLinkOptions directLinkOptions, LogOnCredentials data)
        {
            data.UseAutoLogin = useAutoLogin ?? true;
            data.NtUserName = GetCurrentUser();
            try { data.Validate(); } catch (RulesException ex) { ex.CopyTo(ModelState); } 

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
                else
                {
                    string redirectUrl = QPConfiguration.WebConfigSection.BackendUrl;
                    if (directLinkOptions != null)
                        redirectUrl = directLinkOptions.AddToUrl(redirectUrl);
                    return Redirect(redirectUrl);
                }
            }
            else
            {
                // Отображаем форму ввода логина и пароля с сообщениями об ошибках
                InitViewBag(directLinkOptions);
                return LogOnView(data);
            }
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
                ViewBag.AutoLoginLinkQuery += "&" + directLinkOptions.ToUrlParams();
        }

        [NonAction]
        private ActionResult LogOnView(LogOnCredentials model)
        {
            if (Request.IsAjaxRequest())
            {
                return JsonHtml("Popup", model);
            }
            else
            {
                return View(model);
            }
        }
    }
}
