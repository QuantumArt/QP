using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Quantumart.QP8.Configuration;
using Quantumart.QP8.Utils;

namespace Quantumart.QP8.Security
{
    public class AuthenticationHelper
    {
        private HttpContext _httpContext;
        private QPublishingOptions _options;
        public AuthenticationHelper (IHttpContextAccessor httpContextAccessor, QPublishingOptions options)
        {
            _httpContext = httpContextAccessor.HttpContext;
            _options = options;
        }

        public async void SignIn(QpUser user)
        {
            List<Claim> claims = new List<Claim>
            {
                new Claim("Id", user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Name),
                new Claim(ClaimTypes.Role, string.Join(";", user.Roles)),
                new Claim("CustomerCode", user.CustomerCode),
                new Claim("LanguageId", user.CustomerCode),
                new Claim("CultureName", user.CultureName),
                new Claim("MustChangePassword", user.MustChangePassword.ToString()),
                new Claim(ClaimTypes.Sid, user.SessionId.ToString())
            };

            var claimsIdentity = new ClaimsIdentity(
                claims, CookieAuthenticationDefaults.AuthenticationScheme);

            var authProperties = new AuthenticationProperties();
            await _httpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);
        }

        public async void SignOut()
        {
            await _httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        }

        public bool ShouldUseWindowsAuthentication(string currentIp)
        {
            var result = false;

            var winLogonUrl = _options.Authentication.WinLogonUrl;
            if (winLogonUrl != null)
            {
                result = _options.Authentication.WinLogonIpRanges.Any(range => CheckIpRange(currentIp, range.BeginIp, range.EndIp));
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




        /*
        #if !NET_STANDARD
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




                public static string CompleteAuthentication(QpUser user)
                {
                    FormsAuthenticationTicket ticket;
                    if (QPConfiguration.AppConfigSection.Authentication.AllowSaveUserInformationInCookie)
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

                public static string WindowsAuthenticationUrl => QPConfiguration.AppConfigSection.Authentication.WindowsAuthentication.LoginUrl;

                public static string LogOut()
                {
                    FormsAuthentication.SignOut();
                    return FormsAuthentication.LoginUrl;
                }
        #else
                public static string LogOut() => throw new NotImplementedException();
        #endif

         */
    }
}
