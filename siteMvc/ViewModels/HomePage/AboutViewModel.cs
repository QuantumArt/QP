using System;
using Quantumart.QP8.Resources;
using Quantumart.QP8.Constants;
using Quantumart.QP8.Validators;
using Quantumart.QP8.BLL.Helpers;
using Quantumart.QP8.BLL.Repository;

namespace Quantumart.QP8.WebMvc.ViewModels.HomePage
{
	public sealed class AboutViewModel : AreaViewModel
	{
		#region creation

		public static AboutViewModel Create(string tabId, int parentId)
		{
			AboutViewModel model = ViewModel.Create<AboutViewModel>(tabId, parentId);
			return model;
		}

		#endregion

		public override string EntityTypeCode
		{
			get { return Constants.EntityTypeCode.CustomerCode; }
		}

		public override string ActionCode
		{
			get { return Constants.ActionCode.About; }
		}


		[LocalizedDisplayName("ProductName", NameResourceType = typeof(HomeStrings))]
		public string Product
		{
			get
			{
				return String.Format(HomeStrings.ProductValue, Default.ReleaseNumber, DateTime.Now.Year);
			}
		}

		[LocalizedDisplayName("DBVersion", NameResourceType = typeof(HomeStrings))]
		public string DBVersion
		{
			get
			{
				return new ApplicationInfoHelper().GetCurrentDbVersion();
			}
		}

		[LocalizedDisplayName("BackendVersion", NameResourceType = typeof(HomeStrings))]
		public string BackendVersion
		{
			get
			{
				return new ApplicationInfoHelper().GetCurrentBackendVersion();
			}
		}

		[LocalizedDisplayName("DBName", NameResourceType = typeof(HomeStrings))]
		public string DBName
		{
			get
			{
				return DbRepository.GetDbName();
			}
		}

		[LocalizedDisplayName("DBServerName", NameResourceType = typeof(HomeStrings))]
		public string DBServerName
		{
			get
			{
				return DbRepository.GetDbServerName();
			}
		}
	}

}
