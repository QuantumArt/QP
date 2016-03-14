using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using B = Quantumart.QP8.BLL;
using Quantumart.QP8.Resources;
using C = Quantumart.QP8.Constants;
using Quantumart.QP8.BLL.ListItems;
using Quantumart.QP8.BLL.Services.DTO;


namespace Quantumart.QP8.WebMvc.ViewModels
{
    public class SiteListViewModel : ListViewModel
    {

		public List<SiteListItem> Data { get; set; }

		public string GettingDataActionName
		{
			get
			{
				return (IsSelect) ? "_MultipleSelect" : "_Index";				
			}
		}

		#region creation

		public static SiteListViewModel Create(SiteInitListResult result, string tabId, int parentId, bool isSelect = false, int[] ids = null)
		{
			SiteListViewModel model = ViewModel.Create<SiteListViewModel>(tabId, parentId);
            model.IsSelect = isSelect;
            if (isSelect)
				model.AutoGenerateLink = false;
			model.SelectedIDs = ids;
			model.ShowAddNewItemButton = result.IsAddNewAccessable && !model.IsWindow;
			return model;
		}

		#endregion

		#region read-only members
		public override string EntityTypeCode
		{
			get
			{
				return C.EntityTypeCode.Site;
			}
		}

		public override string ActionCode
		{
			get
			{
				if (IsSelect)
					return C.ActionCode.MultipleSelectSites;
				else
					return C.ActionCode.Sites;
			}
		}

		public override string AddNewItemText
		{
			get
			{
				return SiteStrings.Link_AddNewSite;
			}
		}

		public override string AddNewItemActionCode
		{
			get
			{
				return C.ActionCode.AddNewSite;
			}
		}

		public override bool IsReadOnly
		{
			get
			{
				return base.IsReadOnly || IsSelect;
			}
		}
		#endregion

    }
}