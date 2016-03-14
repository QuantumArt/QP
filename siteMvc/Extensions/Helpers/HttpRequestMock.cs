using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Web;
using System.Web.Helpers;
using System.Web.Util;
using Moq;

namespace Quantumart.QP8.WebMvc.Extensions.Helpers
{
	public class HttpRequestMock : HttpRequestBase
	{
		private NameValueCollection _form;
		private HttpCookieCollection _cookies;
		private NameValueCollection _serverVariables;
		private NameValueCollection _queryString;
		private NameValueCollection _headers;
		private UnvalidatedRequestValuesBase _unvalidated;
		private Mock<HttpFileCollectionBase> _files;
		private Mock<HttpBrowserCapabilitiesBase> _browser;
		
		public HttpRequestMock()
		{
			 _form = new NameValueCollection();
			 _cookies = new HttpCookieCollection();
			 _serverVariables = new NameValueCollection();
			 _queryString = new NameValueCollection();
			 _headers = new NameValueCollection();
			 _unvalidated = new UnvalidatedRequestValuesMock(this);
			 _files = new Mock<HttpFileCollectionBase>();
			 _browser = new Mock<HttpBrowserCapabilitiesBase>();
			 _browser.SetupAllProperties();
		}


		private string _HttpMethod = "POST";
		private string _ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
		private string _UserAgent = "RecordReplayHelper";
		private string _Path = "";

		#region Set Methods

		public void SetHttpMethod(string value)
		{
			_HttpMethod = value;
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
			_Path = value;
		}

		#endregion

		public override NameValueCollection Form
		{
			get
			{
				return _form;
			}
		}

		public override HttpCookieCollection Cookies
		{
			get
			{
				return _cookies;
			}
		}

		public override NameValueCollection ServerVariables
		{
			get
			{
				return _serverVariables;
			}
		}

		public override NameValueCollection QueryString
		{
			get
			{
				return _queryString;
			}
		}

		public override NameValueCollection Headers
		{
			get
			{
				return _headers;
			}
		}

		public override HttpFileCollectionBase Files
		{
			get
			{
				return _files.Object;
			}
		}

		public override string HttpMethod
		{
			get
			{
				return _HttpMethod;
			}
		}

		public override string ContentType
		{
			get
			{
				return _ContentType;
			}
			set
			{
				base.ContentType = value;
			}
		}

		public override string UserAgent
		{
			get
			{
				return _UserAgent;
			}
		}

		public override HttpBrowserCapabilitiesBase Browser
		{
			get
			{
				return _browser.Object;
			}
		}

		public override string Path
		{
			get
			{
				return _Path;
			}
		}

		public override UnvalidatedRequestValuesBase Unvalidated
		{
			get
			{
				return _unvalidated;
			}
		}

		public override void ValidateInput()
		{
		
		}


		public override string this[string key]
		{
			get
			{
				string str = this.QueryString[key];
				if (str != null)
				{
					return str;
				}
				str = this.Form[key];
				if (str != null)
				{
					return str;
				}
				HttpCookie cookie = this.Cookies[key];
				if (cookie != null)
				{
					return cookie.Value;
				}
				str = this.ServerVariables[key];
				if (str != null)
				{
					return str;
				}
				return null;
			}
		}
	}
}
