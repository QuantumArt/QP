using System;
using System.Linq;
using System.Web;
using System.Web.Caching;
using System.Web.Configuration;
using System.Web.Security;
using Quantumart.QP8.Configuration;
using Quantumart.QP8.Configuration.Authentication.WindowsAuthentication;
using Quantumart.QP8.Utils;

namespace Quantumart.QP8.Security
{
    public static class AuthenticationHelper
    {
        /// <summary>
        /// Создает пользовательский билет аутентификации
        /// </summary>
        /// <param name="userName">логин пользователя</param>
        /// <returns>билет аутентификации</returns>
        public static FormsAuthenticationTicket CreateAuthenticationTicket(string userName)
        {
            return CreateAuthenticationTicket(userName, string.Empty);
        }

        /// <summary>
        /// Создает пользовательский билет аутентификации на основе
        /// сериализованной информации о пользователе
        /// </summary>
        /// <param name="userName">логин пользователя</param>
        /// <param name="userData">серилизованная информация о пользователе</param>
        /// <returns></returns>
        public static FormsAuthenticationTicket CreateAuthenticationTicket(string userName, string userData)
        {
            var context = HttpContext.Current;
            var config = (AuthenticationSection)context.GetSection("system.web/authentication");
            var timeout = (int)config.Forms.Timeout.TotalMinutes;

            if (string.IsNullOrEmpty(userData))
            {
                userData = string.Empty;
            }

            // Создаем билет вручную и задаем его свойства
            var ticket = new FormsAuthenticationTicket(
                1,                                    // версия
                userName,                             // логин пользователя
                DateTime.Now,                         // время создания
                DateTime.Now.AddMinutes(timeout),     // время истечения срока
                false,                                // признак постоянного Cookie
                userData,                             // пользовательские данны
                FormsAuthentication.FormsCookiePath); // путь действия Cookie

            return ticket;
        }

        /// <summary>
        /// Создает пользовательский билет аутентификации на основе
        /// несериализованной информации о пользователе
        /// </summary>
        /// <param name="userName">логин пользователя</param>
        /// <param name="userInformation">серилизованная информация о пользователе</param>
        /// <returns></returns>
        public static FormsAuthenticationTicket CreateAuthenticationTicket(string userName, QpUser userInformation)
        {
            return CreateAuthenticationTicket(userName, SerializeUserInformation(userInformation));
        }

        /// <summary>
        /// Сохраняет аутентификационный билет в Cookie
        /// </summary>
        /// <param name="ticket">атентификационный билет</param>
        public static void SetAuthenticationCookie(FormsAuthenticationTicket ticket)
        {
            var ctx = HttpContext.Current;
            var authCookie = FormsAuthentication.Encrypt(ticket);
            var cookie = new HttpCookie(FormsAuthentication.FormsCookieName)
            {
                Value = authCookie,
                Secure = FormsAuthentication.RequireSSL,
                Domain = FormsAuthentication.CookieDomain,
                HttpOnly = true // запрет чтения Cookie из клиентских скриптов
            };

            if (FormsAuthentication.RequireSSL && !ctx.Request.IsSecureConnection)
            {
                throw new HttpException("Аутентификационный билет требует использование SSL!");
            }

            ctx.Response.Cookies.Add(cookie);
        }

        /// <summary>
        /// Сохраняет атентификационный билет в строке запроса
        /// </summary>
        /// <param name="ticket">аутентификационный билет</param>
        /// <param name="url">текущий Url</param>
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

        /// <summary>
        /// Сериализует информацию о пользователе
        /// </summary>
        /// <param name="userInformation">несериализованная информация о пользователе</param>
        /// <returns>сериализованная информация о пользователе</returns>
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
                    string.Join(";", userInformation.Roles);
            }

            return userData;
        }

        /// <summary>
        /// Десериализует информацию о пользователе
        /// </summary>
        /// <param name="userName">логин пользователя</param>
        /// <param name="userData">сериализованная информация о пользователе</param>
        /// <returns>десериализованная информация о пользователе</returns>
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
                        Roles = userDataCollection[6].Split(';')
                    };
                }
            }

            return userInformation;
        }

        /// <summary>
        /// Возвращает информацию о пользователе из атентификационного Cookie
        /// </summary>
        /// <returns></returns>
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

        /// <summary>
        /// Возвращает информацию о пользователе из хранилища
        /// </summary>
        /// <param name="userName">логин пользователя</param>
        /// <returns>информация о пользователе</returns>
        public static QpUser GetUserInformationFromStorage(string userName)
        {
            var context = HttpContext.Current;
            return context.Cache[userName] as QpUser;
        }

        /// <summary>
        /// Добавляет информацию о пользователе в хранилище
        /// </summary>
        /// <param name="userInformartion">информация о пользователе</param>
        public static void AddUserInformationToStorage(QpUser userInformartion)
        {
            var context = HttpContext.Current;

            // Кэшируем информацию о пользователе
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

        /// <summary>
        /// Завершает процедуру аутентификации
        /// </summary>
        /// <param name="user">данные пользователя для сохранения</param>
        /// <returns>URL для редиректа</returns>
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
