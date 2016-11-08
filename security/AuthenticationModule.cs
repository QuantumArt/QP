using System;
using System.Collections.Generic;
using System.Security;
using System.Security.Principal;
using System.Threading;
using System.Web;
using System.Web.Configuration;
using System.Web.Security;
using Quantumart.QP8;
using Quantumart.QP8.Configuration;

namespace Quantumart.QP8.Security
{
    public class AuthenticationModule : IHttpModule
    {
        public void Init(HttpApplication context)
        {
            // Регистрация события PostAuthenticateRequest
            context.PostAuthenticateRequest += new EventHandler(OnPostAuthenticateRequest);
        }

        protected void OnPostAuthenticateRequest(object sender, EventArgs e)
        {
            HttpContext context = HttpContext.Current;

            // Запускается толко в том случае, если пользователь аутентифицирован
            // и используется аутентификация на основе форм
			if (context.Request.IsAuthenticated)
			{
				string userName = context.User.Identity.Name; // логин пользователя
				string[] roles = new string[0]; // роли, доступные пользоватею

				QpUser userInformation; // информация о пользователе
				if (QPConfiguration.WebConfigSection.Authentication.AllowSaveUserInformationInCookie)
				{
					userInformation = AuthenticationHelper.GetUserInformationFromAuthenticationCookie(userName);
					context.Items["userDataStorageType"] = "cookie";
				}
				else
				{
					userInformation = AuthenticationHelper.GetUserInformationFromStorage(userName);
					context.Items["userDataStorageType"] = "cache";
				}

				// Создание нового QP8Identity
				QPIdentity identity;
				if (userInformation != null)
				{

					identity = new QPIdentity(userInformation.Id, userInformation.Name,
						userInformation.CustomerCode, "QP", true,
						userInformation.LanguageId, userInformation.CultureName, userInformation.IsSilverlightInstalled);
					roles = userInformation.Roles;
				}
				else
				{
					identity = new QPIdentity(0, userName, "", "QP", false, 0, "neutral", false);
				}

				// Создание нового QP8Principal
				QPPrincipal principal = new QPPrincipal(identity, roles);

				// Новый принципал становится доступен для остальной части запроса
				context.User = Thread.CurrentPrincipal = principal;
			}			
        }

        public void Dispose()
        {
        }
    }
}
