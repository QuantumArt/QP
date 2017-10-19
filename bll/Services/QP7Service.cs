using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using Quantumart.QP8.BLL.Repository;
using Quantumart.QP8.Constants.Mvc;
using Quantumart.QP8.Resources;

// ReSharper disable InconsistentNaming
namespace Quantumart.QP8.BLL.Services
{
    public interface IQP7Service
    {
        string GetQP7Path();

        QP7Token Authenticate();

        QP7Token Authenticate(string applicationPath);

        QP7Token Authenticate(string userName, string password, string customerCode, string applicationPath);

        string AssemblePage(int pageId, QP7Token token);
    }

    public class QP7Service : IQP7Service
    {
        #region Constants and fields
        private const string Qp7PathSettringKey = "QP7Path";
        private const string AssemblePageTemplate = "{0}assemble.asp?pageId={1}&skipTemplate=1";
        #endregion

        public string GetQP7Path()
        {
            var path = DbRepository.GetAppSettings()
                .Where(s => s.Key == Qp7PathSettringKey)
                .Select(s => s.Value)
                .FirstOrDefault();

            if (string.IsNullOrWhiteSpace(path))
            {
                throw new Exception(SiteStrings.SettingQP7PathIsMissed);
            }

            path = path.Trim();

            if (!path.EndsWith("/"))
            {
                path += "/";
            }

            return path;
        }

        public QP7Token Authenticate()
        {
            return Authenticate(GetQP7Path());
        }
        public QP7Token Authenticate(string applicationPath)
        {
            var customerCode = QPContext.CurrentCustomerCode;
            var userName = QPContext.CurrentUserIdentity.Name;
            var password = HttpContext.Current.Session[HttpContextSession.Qp7Password] as string;

            return Authenticate(userName, password, customerCode, applicationPath);
        }

        public QP7Token Authenticate(string userName, string password, string customerCode, string applicationPath)
        {
            if (userName == null)
            {
                throw new ArgumentNullException(nameof(userName), @"Login is missing for QP7 authentication.");
            }

            if (password == null)
            {
                throw new ArgumentNullException(nameof(password), SiteStrings.QP7MissingPassword);
            }

            if (customerCode == null)
            {
                throw new ArgumentNullException(nameof(customerCode), @"CustomerCode is missing for QP7 authentication");
            }

            if (applicationPath == null)
            {
                throw new ArgumentNullException(nameof(applicationPath), @"Application path is missing for QP7 authentication");
            }

            var loginRequest = CreateHttpWebRequest(applicationPath + "login.asp");
            var loginCookies = new CookieContainer();
            using (var loginResponse = CallPage(loginRequest))
            {
                loginCookies.SetCookies(loginRequest.RequestUri, loginResponse.Headers[ResponseHeaders.SetCookie]);
            }

            var defaultRequest = CreateHttpWebRequest(applicationPath + "default.asp");
            defaultRequest.CookieContainer = loginCookies;

            defaultRequest.Method = "POST";
            var formParameters = Encoding.UTF8.GetBytes($"login={userName}&password={password}&customer={customerCode}");

            defaultRequest.ContentLength = formParameters.Length;
            using (var formParametersStream = defaultRequest.GetRequestStream())
            {
                formParametersStream.Write(formParameters, 0, formParameters.Length);
            }

            var defaultCookies = defaultRequest.CookieContainer;
            using (var defaultResponse = CallPage(defaultRequest))
            {
                defaultCookies.SetCookies(defaultRequest.RequestUri, defaultResponse.Headers[ResponseHeaders.SetCookie]);
            }

            return new QP7Token
            {
                UserName = userName,
                CustomerCode = customerCode,
                Cookie = defaultCookies,
                ApplicationPath = applicationPath
            };
        }

        public string AssemblePage(int pageId, QP7Token token)
        {
            var path = string.Format(AssemblePageTemplate, token.ApplicationPath, pageId);
            var assemblyRequest = CreateHttpWebRequest(path);
            assemblyRequest.CookieContainer = token.Cookie;
            CallPage(assemblyRequest, out string response);

            return response;
        }

        private static HttpWebRequest CreateHttpWebRequest(string requestPath)
        {
            var request = (HttpWebRequest)WebRequest.Create(requestPath);
            request.Credentials = CredentialCache.DefaultCredentials;
            request.Method = "GET";
            request.ContentType = "application/x-www-form-urlencoded";
            request.UserAgent = "Mozilla/4.0+(compatible;+MSIE+6.0;+Windows+NT+5.0)";
            request.Proxy = null;

            return request;
        }

        private static HttpWebResponse CallPage(WebRequest request)
        {
            return CallPage(request, out string response);
        }

        private static HttpWebResponse CallPage(WebRequest request, out string responseValue)
        {
            var encode = Encoding.GetEncoding("utf-8");
            var response = (HttpWebResponse)request.GetResponse();

            using (var responseStream = response.GetResponseStream())
            using (var sr = new StreamReader(responseStream, encode))
            {
                responseValue = sr.ReadToEnd();
            }

            return response;
        }

        public static void SetPassword(string password)
        {
            HttpContext.Current.Session[HttpContextSession.Qp7Password] = password;
        }
    }

    public class QP7Token
    {
        public string UserName { get; set; }

        public string CustomerCode { get; set; }

        public string ApplicationPath { get; set; }

        public CookieContainer Cookie { get; set; }
    }
}
