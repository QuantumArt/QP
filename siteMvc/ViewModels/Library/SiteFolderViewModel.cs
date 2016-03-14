using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using C = Quantumart.QP8.Constants;
using Quantumart.QP8.BLL;

namespace Quantumart.QP8.WebMvc.ViewModels.Library
{
	public class SiteFolderViewModel : FolderViewModel
	{		

		internal static SiteFolderViewModel Create(SiteFolder folder, string tabId, int parentId)
		{
			return EntityViewModel.Create<SiteFolderViewModel>(folder, tabId, parentId);
		}

		public new SiteFolder Data
		{
			get
			{
				return (SiteFolder)EntityData;
			}
			set
			{
				EntityData = value;
			}
		}
		
		public override string EntityTypeCode
		{
			get { return C.EntityTypeCode.SiteFolder; }
		}

		public override string ActionCode
		{
			get
			{
				if (this.IsNew)
				{
					return C.ActionCode.AddNewSiteFolder;
				}
				else
				{
					return C.ActionCode.SiteFolderProperties;
				}
			}
		}
	}
}