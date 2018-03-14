using System;
using System.Linq;
using System.Web;
using System.Web.Caching;
using System.Web.Configuration;
using System.Web.Security;
using QP8.Infrastructure.Extensions;
using QP8.Infrastructure.Logging;
using Quantumart.QP8.Configuration;
using Quantumart.QP8.Configuration.Authentication.WindowsAuthentication;
using Quantumart.QP8.Constants.Mvc;
using Quantumart.QP8.Utils;

namespace Quantumart.QP8.Security
{
    public static class AuthenticationHelper
    {
        public static FormsAuthenticationTicket CreateAuthenticationTicket(string userName, string userData = null)
        {
            var config = (AuthenticationSection)HttpContext.Current.GetSection(WebConfigSections.SystemWebAuthentication);
            var expireAt = (int)config.Forms.Timeout.TotalMinutes;
            return new FormsAuthenticationTicket(
                1, // версия
                userName, // логин пользователя
                DateTime.Now, // время создания
                DateTime.Now.AddMinutes(expireAt), // время истечения срока
                false, // признак постоянного Cookie
                userData ?? string.Empty, // пользовательские данные
                FormsAuthentication.FormsCookiePath); // путь действия Cookie
        }

        public static FormsAuthenticationTicket CreateAuthenticationTicket(string userName, QpUser userInformation) => CreateAuthenticationTicket(userName, SerializeUserInformation(userInformation));

        public static void SetAuthenticationCookie(FormsAuthenticationTicket ticket)
        {
            var ctx = HttpContext.Current;
            if (FormsAuthentication.RequireSSL && !ctx.Request.IsSecureConnection)
            {
                throw new HttpException("Аутентификационный билет требует использование SSL!");
            }

            ctx.Response.Cache.SetCacheability(HttpCacheability.NoCache);
            ctx.Response.SetCookie(new HttpCookie(FormsAuthentication.FormsCookieName)
            {
                Value = FormsAuthentication.Encrypt(ticket),
                Secure = FormsAuthentication.RequireSSL,
                Domain = FormsAuthentication.CookieDomain,
                HttpOnly = true // запрет чтения Cookie из клиентских скриптов
            });
        }

        public static void SetQueryStringRedirect(FormsAuthenticationTicket ticket, string url)
        {
            var context = HttpContext.Current;
            if (FormsAuthentication.RequireSSL && !context.Request.IsSecureConnection)
            {
                throw new HttpException("Аутентификационный билет требует использование SSL!");
            }

            var authCookie = FormsAuthentication.Encrypt(ticket);
            context.Response.Redirect($"{url}?{FormsAuthentication.FormsCookieName}={authCookie}");
        }

        public static string SerializeUserInformation(QpUser userInformation)
        {
            var userData = string.Empty; // сериализованная информация о пользователе
            if (userInformation != null)
            {
                userData = userInformation.Id + "|" +
                    "|" +
                    userInformation.CustomerCode + "|" +
                    userInformation.LanguageId + "|" +
                    userInformation.CultureName + "|" +
                    userInformation.IsSilverlightInstalled + "|" +
                    string.Join(";", userInformation.Roles) + "|" +
                    userInformation.MustChangePassword + "|" +
                    userInformation.SessionId;
            }

            return userData;
        }

        public static QpUser DeserializeUserInformation(string userName, string userData)
        {
            QpUser userInformation = null;
            if (userName.Length > 0 && userData.Length > 0)
            {
                var userDataCollection = userData.Split('|');
                if (userDataCollection.Length > 0)
                {
                    userInformation = new QpUser
                    {
                        Id = int.Parse(userDataCollection[0]),
                        Name = userName,
                        CustomerCode = userDataCollection[2],
                        LanguageId = int.Parse(userDataCollection[3]),
                        IsSilverlightInstalled = bool.Parse(userDataCollection[5]),
                        Roles = userDataCollection[6].Split(';'),
                        MustChangePassword = userDataCollection.Length >= 8 && bool.TryParse(userDataCollection[7], out var result) ? result : false,
                        SessionId = userDataCollection.Length >= 9 && int.TryParse(userDataCollection[8], out var sessionId) ? sessionId : 0
                    };
                }
            }

            return userInformation;
        }

        public static QpUser GetUserInformationFromAuthenticationCookie(string userName)
        {
            var context = HttpContext.Current;
            QpUser userInformation = null;

            var userData = ((FormsIdentity)context.User.Identity).Ticket.UserData;
            if (userData.Length > 0)
            {
                userInformation = DeserializeUserInformation(userName, userData);
            }

            return userInformation;
        }

        public static QpUser GetUserInformationFromStorage(string userName) => HttpContext.Current.Cache[userName] as QpUser;

        public static void AddUserInformationToStorage(QpUser userInformartion)
        {
            var context = HttpContext.Current;
            context.Cache.Insert(userInformartion.Name, userInformartion, null, DateTime.Now.AddMinutes(30), Cache.NoSlidingExpiration);
        }

        public static bool ShouldUseWindowsAuthentication(string currentIp)
        {
            var result = false;
            var qpWindowsAuthConfig = QPConfiguration.WebConfigSection.Authentication.WindowsAuthentication;
            if (qpWindowsAuthConfig != null)
            {
                result = qpWindowsAuthConfig.IpRanges.Cast<IpRangeElement>().Any(range => CheckIpRange(currentIp, range.BeginIp, range.EndIp));
            }

            return result;
        }

        /// <summary>
        /// Проверяет попадание IP-адреса в указанный диапазон IP-адресов
        /// </summary>
        /// <param name="currentIp">текущий IP-адрес</param>
        /// <param name="beginIp">начальный IP-адрес</param>
        /// <param name="endIp">конечный IP-адрес</param>
        /// <returns>результат проверки (true - попадает; false - не попадает)</returns>
        private static bool CheckIpRange(string currentIp, string beginIp, string endIp)
        {
            var result = false;
            var currentIpNumber = Converter.IpToInt64(currentIp);
            var beginIpNumber = Converter.IpToInt64(beginIp);
            var endIpNumber = Converter.IpToInt64(endIp);
            if (currentIpNumber >= beginIpNumber && currentIpNumber <= endIpNumber)
            {
                result = true;
            }

            return result;
        }

        public static string CompleteAuthentication(QpUser user)
        {
            FormsAuthenticationTicket ticket;
            if (QPConfiguration.WebConfigSection.Authentication.AllowSaveUserInformationInCookie)
            {
                ticket = CreateAuthenticationTicket(user.Name, user);
            }
            else
            {
                ticket = CreateAuthenticationTicket(user.Name);
                AddUserInformationToStorage(user);
            }

            SetAuthenticationCookie(ticket);

            Logger.Log.Debug($"User successfully authenticated: {user.ToJsonLog()}");
            return FormsAuthentication.GetRedirectUrl(string.Empty, false);
        }

        public static string WindowsAuthenticationUrl => QPConfiguration.WebConfigSection.Authentication.WindowsAuthentication.LoginUrl;

        public static string LogOut()
        {
            FormsAuthentication.SignOut();
            return FormsAuthentication.LoginUrl;
        }
    }
}
