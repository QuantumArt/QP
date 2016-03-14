using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Quantumart.QP8.WebMvc.Extensions.Helpers;

namespace Quantumart.QP8.WebMvc.ViewModels
{
	public class UserSearchBlockViewModel
	{
		private readonly string hostId;

		public UserSearchBlockViewModel(string hostId)
		{
			this.hostId = hostId;
		}

		public string LoginElementId
		{
			get
			{
				return HtmlHelperFieldExtensions.UniqueId("txtLogin", hostId);
			}
		}

		public string EmailElementId
		{
			get
			{
				return HtmlHelperFieldExtensions.UniqueId("txtEmail", hostId);
			}
		}

		public string FirstNameElementId
		{
			get
			{
				return HtmlHelperFieldExtensions.UniqueId("txtFirstName", hostId);
			}
		}

		public string LastNameElementId
		{
			get
			{
				return HtmlHelperFieldExtensions.UniqueId("txtLastName", hostId);
			}
		}
	}
}