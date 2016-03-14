using System;
using System.Collections.Generic;
using System.Text;
using System.Configuration;
using System.Web;
using System.Web.Configuration;

namespace Quantumart.QP8.Configuration.Globalization
{
	public class GlobalizationElement : ConfigurationElement
	{
		[ConfigurationProperty("defaultLanguageId", IsRequired = true)]
		public int DefaultLanguageId
		{
			get { return (int)base["defaultLanguageId"]; }
			set { base["defaultLanguageId"] = value; }
		}

		[ConfigurationProperty("defaultCulture", IsRequired = true)]
		public string DefaultCulture
		{
			get { return (string)base["defaultCulture"]; }
			set { base["defaultCulture"] = value; }
		}
	}
}
