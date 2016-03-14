using System;
using System.Collections.Generic;
using System.Linq;
using System.Configuration;
using System.Security;
using System.Web;
using System.Web.Configuration;
using System.Web.Security;
using System.Web.Mvc;
using Quantumart.QP8;
using Quantumart.QP8.Configuration;
using Quantumart.QP8.Security;
using Quantumart.QP8.BLL;
using Quantumart.QP8.WebMvc.Extensions.Controllers;
using Quantumart.QP8.WebMvc.Extensions.ActionFilters;
using Quantumart.QP8.WebMvc.ViewModels.DirectLink;
using Quantumart.QP8.WebMvc.Extensions.Helpers;

namespace Quantumart.QP8.WebMvc.Controllers
{
    public class LogOnController : QPController
    {
		/// <summary>
		/// Выводит форму ввода логина и пароля
		/// </summary>
		[HttpGet]
		[DisableBrowserCache]
		[ResponseHeader("QP-Not-Authenticated", "True")]
		public ActionResult Index(DirectLinkOptions directLinkOptions)
        {
			if (!Request.IsAuthenticated && AuthenticationHelper.ShouldUseWindowsAuthentication(Request.UserHostAddress))
			{				
				// Если IP-адрес пользователя входит в диапазон IP-адресов 
				// внутренней сети, то перенаправляем его на страницу Windows-аутентификации
				if (directLinkOptions != null)
					return Redirect(directLinkOptions.AddToUrl(AuthenticationHelper.WindowsAuthenticationUrl));
				else
					return Redirect(AuthenticationHelper.WindowsAuthenticationUrl);
			}
			else
			{
				InitViewBag();
				return LogOnView();
			}
        }

		/// <summary>
		/// Аутентифицирует пользователя
		/// </summary>
        /// <param name="data">данные формы</param>
		[HttpPost]
		[DisableBrowserCache]
		public ActionResult Index(DirectLinkOptions directLinkOptions, LogOnCredentials data)
		{
			// Проверяем правильность введенных значений
            try { data.Validate(); } catch (RulesException ex) { ex.CopyTo(ModelState); }

			if (ModelState.IsValid && data.User != null)
			{
				AuthenticationHelper.CompleteAuthentication(data.User);

				if (Request.IsAjaxRequest())
				{
					return Json( new
					{
						success = true,
						isAuthenticated = true,
						userName = data.User.Name
					});
				}
				else
				{					
					if (directLinkOptions != null && directLinkOptions.IsDefined())
						return RedirectToAction("Index", "Home", directLinkOptions);
					else
						return RedirectToAction("Index", "Home");
				}
			}
			else
			{
				// Отображаем форму ввода логина и пароля с сообщениями об ошибках
				InitViewBag();
				return LogOnView();
			}
		}	

		/// <summary>
		/// Производит завершение аутентификационной сессии пользователя
		/// </summary>
		[HttpGet]
		[DisableBrowserCache]
		public ActionResult LogOut(DirectLinkOptions directLinkOptions)
		{			
			string loginUrl = QPContext.LogOut();
			if (directLinkOptions != null)
				loginUrl = directLinkOptions.AddToUrl(loginUrl);
			return Redirect(loginUrl);			
		}

		private void InitViewBag()
		{
			ViewBag.AllowSelectCustomerCode = QPConfiguration.AllowSelectCustomerCode;
			ViewBag.CustomerCodes = QPConfiguration.CustomerCodes.Select(c => new QPSelectListItem { Text = c, Value = c }).OrderBy(n => n.Text);
		}

		private ActionResult LogOnView()
		{
			if (Request.IsAjaxRequest())
			{
				return JsonHtml("Popup", null);
			}
			else
			{
				return View();
			}
		}
    }
}