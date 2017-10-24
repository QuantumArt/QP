using System.Collections.Specialized;
using System.Web;
using Moq;

namespace Quantumart.QP8.WebMvc.Extensions.Helpers
{
    public class HttpRequestMock : HttpRequestBase
    {
        private NameValueCollection _form;
        private readonly Mock<HttpFileCollectionBase> _files;
        private readonly Mock<HttpBrowserCapabilitiesBase> _browser;

        public HttpRequestMock()
        {
            _form = new NameValueCollection();
            Cookies = new HttpCookieCollection();
            ServerVariables = new NameValueCollection();
            QueryString = new NameValueCollection();
            Headers = new NameValueCollection();
            Unvalidated = new UnvalidatedRequestValuesMock(this);
            _files = new Mock<HttpFileCollectionBase>();
            _browser = new Mock<HttpBrowserCapabilitiesBase>();
            _browser.SetupAllProperties();
        }

        private const string FormContentType = "application/x-www-form-urlencoded; charset=UTF-8";
        private string _path = string.Empty;

        public void SetForm(NameValueCollection value)
        {
            _form = value;
        }

        public void SetPath(string value)
        {
            _path = value;
        }

        public override NameValueCollection Form => _form;

        public override HttpCookieCollection Cookies { get; }

        public override NameValueCollection ServerVariables { get; }

        public override NameValueCollection QueryString { get; }

        public override NameValueCollection Headers { get; }

        public override HttpFileCollectionBase Files => _files.Object;

        public override string HttpMethod { get; } = "POST";

        public override string ContentType
        {
            get => FormContentType;
            set => base.ContentType = value;
        }

        public override string UserAgent { get; } = "RecordReplayHelper";

        public override HttpBrowserCapabilitiesBase Browser => _browser.Object;

        public override string Path => _path;

        public override UnvalidatedRequestValuesBase Unvalidated { get; }

        public override void ValidateInput()
        {
            // Should not be deleted
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

                return ServerVariables[key];
            }
        }
    }
}
