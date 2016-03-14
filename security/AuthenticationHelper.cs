using System;
using System.Security;
using System.Security.Principal;
using System.Configuration;
using System.Web;
using System.Web.Caching;
using System.Web.Security;
using System.Web.Configuration;
using System.Linq;
using Quantumart.QP8;
using Quantumart.QP8.Configuration;
using Quantumart.QP8.Configuration.Authentication;
using Quantumart.QP8.Configuration.Authentication.WindowsAuthentication;

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
            HttpContext context = HttpContext.Current;

            // Получаем таймаут билета из конфигурациоонного файла
            AuthenticationSection config = (AuthenticationSection)context.GetSection("system.web/authentication");
            int timeout = (int) config.Forms.Timeout.TotalMinutes;

            if (string.IsNullOrEmpty(userData))
            {
                userData = String.Empty;
            }

            // Создаем билет вручную и задаем его свойства
            FormsAuthenticationTicket ticket = new FormsAuthenticationTicket(
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
        public static FormsAuthenticationTicket CreateAuthenticationTicket(string userName, QPUser userInformation)
        {
            return CreateAuthenticationTicket(userName, SerializeUserInformation(userInformation));
        }

        /// <summary>
        /// Сохраняет аутентификационный билет в Cookie
        /// </summary>
        /// <param name="ticket">атентификационный билет</param>
        public static void SetAuthenticationCookie(FormsAuthenticationTicket ticket)
        {
            HttpContext context = HttpContext.Current;

            // Шифруем билет
            string authcookie = FormsAuthentication.Encrypt(ticket);

            // Создаем новый куки и добавляем в него данные
            HttpCookie cookie = new HttpCookie(FormsAuthentication.FormsCookieName);
            cookie.Value = authcookie;

            // Настраиваем SSL и видимость Cookie домену
            cookie.Secure = FormsAuthentication.RequireSSL;
            cookie.Domain = FormsAuthentication.CookieDomain;

            // Запрещаем чтение Cookie из клиентских скриптов
            cookie.HttpOnly = true;

            // Если требуется использование SSL, но запрос выполняется без SSL,
            // то возбуждаем исключение
            if (!context.Request.IsSecureConnection && FormsAuthentication.RequireSSL)
            {
                throw new HttpException("Аутентификационный билет требует использование SSL!");
            }

            // Записываем куки
            context.Response.Cookies.Add(cookie);
        }

        /// <summary>
        /// Сохраняет атентификационный билет в строке запроса
        /// </summary>
        /// <param name="ticket">аутентификационный билет</param>
        /// <param name="url">текущий Url</param>
        public static void SetQueryStringRedirect(FormsAuthenticationTicket ticket, string url)
        {
            HttpContext context = HttpContext.Current;

            // Если требуется использование SSL, но запрос выполняется без SSL,
            // то возбуждаем исключение
            if (!context.Request.IsSecureConnection && FormsAuthentication.RequireSSL)
            {
                throw new HttpException("Аутентификационный билет требует использование SSL!");
            }

            // Шифруем атентификационный билет
            string encTicket = FormsAuthentication.Encrypt(ticket);

            context.Response.Redirect(String.Format("{0}?{1}={2}",
                url,
                FormsAuthentication.FormsCookieName,
                encTicket));
        }

        /// <summary>
        /// Сериализует информацию о пользователе
        /// </summary>
        /// <param name="userInformation">несериализованная информация о пользователе</param>
        /// <returns>сериализованная информация о пользователе</returns>
        public static string SerializeUserInformation(QPUser userInformation)
        {
            string userData = ""; // сериализованная информация о пользователе

            if (userInformation != null)
            {
                userData = userInformation.Id + "|" +
                    "|" +
                    userInformation.CustomerCode + "|" +
					userInformation.LanguageId + "|" +
					userInformation.CultureName + "|" +
					userInformation.IsSilverlightInstalled + "|" +
                    String.Join(";", userInformation.Roles);
            }

            return userData;
        }

        /// <summary>
        /// Десериализует информацию о пользователе
        /// </summary>
        /// <param name="userName">логин пользователя</param>
        /// <param name="userData">сериализованная информация о пользователе</param>
        /// <returns>десериализованная информация о пользователе</returns>
        public static QPUser DeserializeUserInformation(string userName, string userData)
        {
            QPUser userInformation = null;

            if (userName.Length > 0 && userData.Length > 0)
            {
                string[] userDataCollection = userData.Split('|');

                if (userDataCollection.Length > 0)
                {
                    userInformation = new QPUser();
                    userInformation.Id = int.Parse(userDataCollection[0]);
                    userInformation.Name = userName;
                    userInformation.CustomerCode = userDataCollection[2];
					userInformation.LanguageId = Int32.Parse(userDataCollection[3]);
					userInformation.IsSilverlightInstalled = Boolean.Parse(userDataCollection[5]);
                    userInformation.Roles = userDataCollection[6].Split(';');
                }

                userDataCollection = null;
            }

            return userInformation;
        }

        /// <summary>
        /// Возвращает информацию о пользователе из атентификационного Cookie
        /// </summary>
        /// <returns></returns>
        public static QPUser GetUserInformationFromAuthenticationCookie(string userName)
        {
            HttpContext context = HttpContext.Current;
            QPUser userInformation = null;

            string userData = ((FormsIdentity)context.User.Identity).Ticket.UserData;
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
        public static QPUser GetUserInformationFromStorage(string userName)
        {
            HttpContext context = HttpContext.Current;
            QPUser user = context.Cache[userName] as QPUser;

            return user;
        }

        /// <summary>
        /// Добавляет информацию о пользователе в хранилище
        /// </summary>
        /// <param name="userInformartion">информация о пользователе</param>
        public static void AddUserInformationToStorage(QPUser userInformartion)
        {
            HttpContext context = HttpContext.Current;

            // Кэшируем информацию о пользователе
            context.Cache.Insert(userInformartion.Name, userInformartion, null, 
                DateTime.Now.AddMinutes(30), Cache.NoSlidingExpiration);
        }

        public static bool ShouldUseWindowsAuthentication(string currentIp)
        {
            bool result = false; // результат проверки

            WindowsAuthenticationElement qpWindowsAuthConfig = QPConfiguration.WebConfigSection.Authentication.WindowsAuthentication;

            if (qpWindowsAuthConfig != null)
            {
                foreach (IpRangeElement range in qpWindowsAuthConfig.IpRanges)
                {
                    if (AuthenticationHelper.CheckIpRange(currentIp, range.BeginIp, range.EndIp))
                    {
                        result = true;
                        break;
                    }
                }
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
            bool result = false; // результат проверки

            long currentIpNumber = Utils.Converter.IpToInt64(currentIp);
            long beginIpNumber = Utils.Converter.IpToInt64(beginIp);
            long endIpNumber = Utils.Converter.IpToInt64(endIp);

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
        public static string CompleteAuthentication(QPUser user)
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

            // Сохраняем аутентификационный билет в Cookie
            SetAuthenticationCookie(ticket);

            // Перенаправляем к изначально запрошенному ресурсу
            return FormsAuthentication.GetRedirectUrl(string.Empty, false);

        }

        public static string WindowsAuthenticationUrl
        {
            get
            {
                return QPConfiguration.WebConfigSection.Authentication.WindowsAuthentication.LoginUrl;
            }

        }

       public static string LogOut()
       {
           FormsAuthentication.SignOut();
           return FormsAuthentication.LoginUrl;	       
       }

    }
}
