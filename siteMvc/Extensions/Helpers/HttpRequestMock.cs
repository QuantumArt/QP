using System.Collections.Specialized;
using System.Web;
using Moq;

namespace Quantumart.QP8.WebMvc.Extensions.Helpers
{
    public class HttpRequestMock : HttpRequestBase
    {
        private NameValueCollection _form;
        private HttpCookieCollection _cookies;
        private NameValueCollection _serverVariables;
        private NameValueCollection _queryString;
        private readonly Mock<HttpFileCollectionBase> _files;
        private readonly Mock<HttpBrowserCapabilitiesBase> _browser;

        public HttpRequestMock()
        {
            _form = new NameValueCollection();
            _cookies = new HttpCookieCollection();
            _serverVariables = new NameValueCollection();
            _queryString = new NameValueCollection();
            Headers = new NameValueCollection();
            Unvalidated = new UnvalidatedRequestValuesMock(this);
            _files = new Mock<HttpFileCollectionBase>();
            _browser = new Mock<HttpBrowserCapabilitiesBase>();
            _browser.SetupAllProperties();
        }


        private string _httpMethod = "POST";
        private const string FormContentType = "application/x-www-form-urlencoded; charset=UTF-8";
        private string _path = string.Empty;

        public void SetHttpMethod(string value)
        {
            _httpMethod = value;
        }

        public void SetForm(NameValueCollection value)
        {
            _form = value;
        }

        public void SetCookies(HttpCookieCollection value)
        {
            _cookies = value;
        }

        public void SetServerVariables(NameValueCollection value)
        {
            _serverVariables = value;
        }

        public void SetQueryString(NameValueCollection value)
        {
            _queryString = value;
        }

        public void SetPath(string value)
        {
            _path = value;
        }

        public override NameValueCollection Form => _form;

        public override HttpCookieCollection Cookies => _cookies;

        public override NameValueCollection ServerVariables => _serverVariables;

        public override NameValueCollection QueryString => _queryString;

        public override NameValueCollection Headers { get; }

        public override HttpFileCollectionBase Files => _files.Object;

        public override string HttpMethod => _httpMethod;

        public override string ContentType
        {
            get
            {
                return FormContentType;
            }
            set
            {
                base.ContentType = value;
            }
        }

        public override string UserAgent { get; } = "RecordReplayHelper";

        public override HttpBrowserCapabilitiesBase Browser => _browser.Object;

        public override string Path => _path;

        public override UnvalidatedRequestValuesBase Unvalidated { get; }

        public override void ValidateInput()
        {
        }

        public override string this[string key]
        {
            get
            {
                var str = QueryString[key];
                if (str != null)
                {
                    return str;
                }

                str = Form[key];
                if (str != null)
                {
                    return str;
                }

                var cookie = Cookies[key];
                if (cookie != null)
                {
                    return cookie.Value;
                }

                str = ServerVariables[key];
                return str;
            }
        }
    }
}
