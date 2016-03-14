using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using Quantumart.QP8.BLL.Repository;
using Quantumart.QP8.Resources;

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
        private const string QP7PathSettringKey = "QP7Path";
        private const string PasswordKey = "QP7Service.Password";
        private const string AssemblePageTemplate = "{0}assemble.asp?pageId={1}&skipTemplate=1";
        #endregion      

        public QP7Service()
        {
        }

        #region IQP7Service implementation
        public string GetQP7Path()
        {
            string path = DbRepository.GetAppSettings()
                .Where(s => s.Key == QP7PathSettringKey)
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
            string customerCode = QPContext.CurrentCustomerCode;
            string userName = QPContext.CurrentUserIdentity.Name;
            string password = HttpContext.Current.Session[PasswordKey] as string;          

            return Authenticate(userName, password, customerCode, applicationPath);
        }

        public QP7Token Authenticate(string userName, string password, string customerCode, string applicationPath)
        {
            if (userName == null)
            {
                throw new ArgumentNullException("userName", "Login is missing for QP7 authentication.");
            }

            if (password == null)
            {
                throw new ArgumentNullException("password", SiteStrings.QP7MissingPassword);
            }

            if (customerCode == null)
            {
                throw new ArgumentNullException("customerCode", "CustomerCode is missing for QP7 authentication");
            }

            if (applicationPath == null)
            {
                throw new ArgumentNullException("applicationPath", "Application path is missing for QP7 authentication");
            }

            HttpWebRequest loginRequest = CreateHttpWebRequest(applicationPath + "login.asp");

            CookieContainer loginCookies = new CookieContainer();
            using (HttpWebResponse loginResponse = CallPage(loginRequest))
                loginCookies.SetCookies(loginRequest.RequestUri, loginResponse.Headers["Set-Cookie"]);

            HttpWebRequest defaultRequest = CreateHttpWebRequest(applicationPath + "default.asp");
            defaultRequest.CookieContainer = loginCookies;

            defaultRequest.Method = "POST";
            byte[] formParameters = Encoding.UTF8.GetBytes(
                string.Format(
                    "login={0}&password={1}&customer={2}",
                    userName, password, customerCode));

            defaultRequest.ContentLength = formParameters.Length;
            using (Stream formParametersStream = defaultRequest.GetRequestStream())
                formParametersStream.Write(formParameters, 0, formParameters.Length);

            CookieContainer defaultCookies = defaultRequest.CookieContainer;
            using (HttpWebResponse defaultResponse = CallPage(defaultRequest))
                defaultCookies.SetCookies(defaultRequest.RequestUri, defaultResponse.Headers["Set-Cookie"]);

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
            string response;
            string path = string.Format(AssemblePageTemplate, token.ApplicationPath, pageId);
            HttpWebRequest assemblyRequest = CreateHttpWebRequest(path);
            assemblyRequest.CookieContainer = token.Cookie;
            CallPage(assemblyRequest, out response);
            return response;
        }
        #endregion

        #region Private methods
        private HttpWebRequest CreateHttpWebRequest(string requestPath)
        {
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(requestPath);

            request.Credentials = CredentialCache.DefaultCredentials;
            request.Method = "GET";
            request.ContentType = "application/x-www-form-urlencoded";
            request.UserAgent = "Mozilla/4.0+(compatible;+MSIE+6.0;+Windows+NT+5.0)";
      
            request.Proxy = null;

            return request;
        }

        private HttpWebResponse CallPage(HttpWebRequest request)
        {
            string response;
            return CallPage(request, out response);
        }

        private HttpWebResponse CallPage(HttpWebRequest request, out string responseValue)
        {
            Encoding encode = Encoding.GetEncoding("utf-8");

            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            using (Stream responseStream = response.GetResponseStream())
            using (StreamReader sr = new StreamReader(responseStream, encode))
                responseValue = sr.ReadToEnd();

            return response;
        }
        #endregion

        public static void SetPassword(string password)
        {
            HttpContext.Current.Session[PasswordKey] = password;
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
