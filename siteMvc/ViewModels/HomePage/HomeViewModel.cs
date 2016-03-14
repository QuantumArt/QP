using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using Quantumart.QP8.Resources;
using Quantumart.QP8.WebMvc.Extensions.Helpers;
using Quantumart.QP8.BLL;
using Quantumart.QP8.Validators;
using Quantumart.QP8.BLL.Services.DTO;

namespace Quantumart.QP8.WebMvc.ViewModels.HomePage
{
	public sealed class HomeViewModel : AreaViewModel
	{
		#region creation

		public static HomeViewModel Create(string tabId, int parentId, HomeResult result)
		{
			HomeViewModel model = ViewModel.Create<HomeViewModel>(tabId, parentId);
			model.Sites = result.Sites;
			model.CurrentUser = result.CurrentUser;
			model.LockedCount = result.LockedCount;
			model.ApprovalCount = result.ApprovalCount;
			return model;
		}

		#endregion

		[LocalizedDisplayName("LoggedAs", NameResourceType = typeof(HomeStrings))]
		public string LoggedAs
		{
			get { return CurrentUser.FullName; }
		}


		[LocalizedDisplayName("Search", NameResourceType = typeof(HomeStrings))]
		public string Search
		{
			get; set;
		}


		[LocalizedDisplayName("Site", NameResourceType = typeof(HomeStrings))]
		public int SiteId
		{
			get;
			set;
		}

		[LocalizedDisplayName("LockedCount", NameResourceType = typeof(HomeStrings))]
		public int LockedCount
		{
			get;
			set;
		}

		[LocalizedDisplayName("ApprovalCount", NameResourceType = typeof(HomeStrings))]
		public int ApprovalCount
		{
			get;
			set;
		}


		public IEnumerable<ListItem> Sites
		{
			get;
			set;
		}

		public User CurrentUser
		{
			get;
			set;
		}

		public override string EntityTypeCode
		{
			get { return Constants.EntityTypeCode.CustomerCode; }
		}

		public override string ActionCode
		{
			get { return Constants.ActionCode.Home; }
		}

		public string SiteElementId
		{
			get
			{
				return UniqueId("siteElement");
			}
		}

		public string LockedElementId
		{
			get
			{
				return UniqueId("lockedElement");
			}
		}

		public string SearchElementId
		{
			get
			{
				return UniqueId("searchElement");
			}
		}

		public string ApprovalElementId
		{
			get
			{
				return UniqueId("approvalElement");
			}
		}


		public string LoggedAsElementId
		{
			get
			{
				return UniqueId("loggedAsElement");
			}
		}

		public string CustomerCode
		{
			get
			{
				return QPContext.CurrentCustomerCode;
			}
		}
	}

}