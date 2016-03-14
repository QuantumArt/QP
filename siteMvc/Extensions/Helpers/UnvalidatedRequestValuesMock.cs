using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Collections.Specialized;

namespace Quantumart.QP8.WebMvc.Extensions.Helpers
{
	public class UnvalidatedRequestValuesMock : UnvalidatedRequestValuesBase
	{
		private HttpRequestBase _request;
		
		public UnvalidatedRequestValuesMock(HttpRequestBase request)
		{
			_request = request;
		}
		
		public override HttpCookieCollection Cookies
		{
			get
			{
				return _request.Cookies;
			}
		}

		public override HttpFileCollectionBase Files
		{
			get
			{
				return _request.Files;
			}
		}

		public override NameValueCollection Headers
		{
			get
			{
				return _request.Headers;
			}
		}

		public override NameValueCollection QueryString
		{
			get 
			{
				return _request.QueryString;
			}	
		}

		public override NameValueCollection Form
		{
			get
			{
				return _request.Form;
			}
		}

		public override string this[string field]
		{
			get
			{
				return _request[field];
			}
		}

	}
}