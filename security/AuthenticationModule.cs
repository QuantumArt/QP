using System;
using System.Threading;
using System.Web;
using Quantumart.QP8.Configuration;
using Quantumart.QP8.Constants.Mvc;

namespace Quantumart.QP8.Security
{
    public class AuthenticationModule : IHttpModule
    {
        public void Init(HttpApplication context)
        {
            context.PostAuthenticateRequest += OnPostAuthenticateRequest;
        }

        protected void OnPostAuthenticateRequest(object sender, EventArgs e)
        {
            var context = HttpContext.Current;
            if (context.Request.IsAuthenticated)
            {
                var userName = context.User.Identity.Name;
                var roles = new string[0];

                QpUser userInformation;
                if (QPConfiguration.WebConfigSection.Authentication.AllowSaveUserInformationInCookie)
                {
                    userInformation = AuthenticationHelper.GetUserInformationFromAuthenticationCookie(userName);
                    context.Items[HttpContextItems.UserDataStorageType] = "cookie";
                }
                else
                {
                    userInformation = AuthenticationHelper.GetUserInformationFromStorage(userName);
                    context.Items[HttpContextItems.UserDataStorageType] = "cache";
                }

                QPIdentity identity;
                if (userInformation != null)
                {

                    identity = new QPIdentity(
                        userInformation.Id,
                        userInformation.Name,
                        userInformation.CustomerCode,
                        "QP",
                        true,
                        userInformation.LanguageId,
                        userInformation.CultureName,
                        userInformation.IsSilverlightInstalled);

                    roles = userInformation.Roles;
                }
                else
                {
                    identity = new QPIdentity(0, userName, string.Empty, "QP", false, 0, "neutral", false);
                }

                context.User = Thread.CurrentPrincipal = new QpPrincipal(identity, roles);
            }
        }

        public void Dispose()
        {
        }
    }
}
