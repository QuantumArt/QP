using System;
using System.Collections.Generic;
using System.Text;
using System.Configuration;
using System.Web;
using System.Web.Configuration;
using Quantumart.QP8.Configuration.Globalization;
using Quantumart.QP8.Configuration.Authentication;

namespace Quantumart.QP8.Configuration
{
    public class QPublishingSection : ConfigurationSection
    {
		[ConfigurationProperty("globalization", IsRequired = true)]
		public GlobalizationElement Globalization
		{
			get { return (GlobalizationElement)base["globalization"]; }
		}

        [ConfigurationProperty("authentication", IsRequired = true)]
        public AuthenticationElement Authentication
        {
            get { return (AuthenticationElement)base["authentication"]; }
        }

		/// <summary>
		/// Возвращает URL, по которому расположен бэкенд
		/// </summary>
		[ConfigurationProperty("backendUrl", IsRequired = true)]
		public string BackendUrl
		{
			get { return Utils.Url.ToAbsolute(base["backendUrl"].ToString()); }
			set { base["backendUrl"] = value; }
		}

		/// <summary>
		/// Возвращает название темы по умолчанию
		/// </summary>
		[ConfigurationProperty("defaultTheme", IsRequired = true)]
		public string DefaultTheme
		{
			get { return Utils.Url.ToAbsolute(base["defaultTheme"].ToString()); }
			set { base["defaultTheme"] = value; }
		}

		/// <summary>
		/// Возвращает название темы по умолчанию
		/// </summary>
		[ConfigurationProperty("uploaderType", IsRequired = true)]
		public string UploaderTypeName
		{
			get { return Utils.Url.ToAbsolute(base["uploaderType"].ToString()); }
			set { base["uploaderType"] = value; }
		}

		[ConfigurationProperty("uploadMaxSize", DefaultValue = 100)]
		public int UploadMaxSize
		{
			get
			{
				return (int)base["uploadMaxSize"];
			}
		}

		[ConfigurationProperty("instanceName", DefaultValue = "")]
		public string InstanceName
		{
			get
			{
				return (string)base["instanceName"];
			}
		}

		[ConfigurationProperty("relationLimit", DefaultValue = 500)]
		public int RelationCountLimit
		{
			get
			{
				return (int)base["relationLimit"];
			}
		}


		[ConfigurationProperty("cmdTimeout", DefaultValue=120 )]
		public int CommandTimeout
		{
			get
			{
				return (int)base["cmdTimeout"];
			}
		}
    }
}
