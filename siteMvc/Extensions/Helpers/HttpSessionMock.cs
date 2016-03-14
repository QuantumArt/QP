using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Quantumart.QP8.WebMvc.Extensions.Helpers
{
	public class HttpSessionMock : HttpSessionStateBase
	{
		private readonly NameValueCollection keyCollection = new NameValueCollection();
		private readonly Dictionary<string, object> objects = new Dictionary<string, object>();

		public override object this[string name]
		{
			get
			{
				object result = null;

				if (objects.ContainsKey(name))
				{
					result = objects[name];
				}

				return result;

			}
			set
			{
				objects[name] = value;
				keyCollection[name] = null;
			}
		}

		public override NameObjectCollectionBase.KeysCollection Keys
		{
			get { return keyCollection.Keys; }
		}
	}
}
