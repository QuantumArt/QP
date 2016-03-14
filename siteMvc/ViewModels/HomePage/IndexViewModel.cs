using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Web;
using Quantumart.QP8.BLL;
using Quantumart.QP8.Utils;
using System.Web.Mvc;
using Quantumart.QP8.WebMvc.ViewModels.DirectLink;
using Quantumart.QP8.Configuration;
using Quantumart.QP8.Constants;

namespace Quantumart.QP8.WebMvc.ViewModels.HomePage
{
	public class IndexViewModel
	{
		private DirectLinkOptions directLinkOptions;
		private Db data;
		private string dbHash;
		private string title = "QP8 Backend";

		public IndexViewModel(DirectLinkOptions directLinkOptions, Db data, string dbHash)
		{
			this.directLinkOptions = directLinkOptions;
			this.data = data;
			this.dbHash = dbHash;
		}

		public Db Data
		{
			get { return data; }
		}

		public string DbHash
		{
			get { return dbHash; }
		}

		public string Title
		{
			get 
			{
				string configTitle = QPConfiguration.ApplicationTitle
					.Replace("{release}", Default.ReleaseNumber);

				string instanceName = QPConfiguration.WebConfigSection.InstanceName;
				if (!String.IsNullOrEmpty(configTitle) && !String.IsNullOrEmpty(instanceName))
				{
					return instanceName + " " + configTitle;
				}

				return !String.IsNullOrEmpty(configTitle) ? configTitle : title; 
			}
		}

		public MvcHtmlString BackendComponentOptions
		{ 
			get 
			{
				dynamic result = new ExpandoObject();
				result.currentCustomerCode = QPContext.CurrentCustomerCode;
				result.currentUserId = QPContext.CurrentUserId;
				result.autoLoadHome = data.AutoOpenHome;
				if(directLinkOptions != null && directLinkOptions.IsDefined())
					result.directLinkOptions = directLinkOptions;
				return MvcHtmlString.Create(((ExpandoObject)result).ToJson());
			} 
		}
	}
}