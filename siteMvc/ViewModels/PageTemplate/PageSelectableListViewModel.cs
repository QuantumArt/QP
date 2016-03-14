using Quantumart.QP8.BLL.ListItems;
using Quantumart.QP8.BLL.Services.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using C = Quantumart.QP8.Constants;

namespace Quantumart.QP8.WebMvc.ViewModels.PageTemplate
{	
	public class PageSelectableListViewModel : ListViewModel
	{
		public List<PageListItem> Data { get; set; }

		public string ParentName { get; set; }

		public string GroupName { get; set; }

		#region creation

		public PageSelectableListViewModel(PageInitListResult result, string tabId, int parentId, int[] IDs)
		{
			this.ParentEntityId = parentId;
			this.TabId = tabId;
			this.ParentName = result.ParentName;
			this.SelectedIDs = IDs;
			this.AutoGenerateLink = false;
			this.ShowAddNewItemButton = !IsWindow;							
		}

		#endregion

		#region read-only members

		public sealed override string EntityTypeCode
		{
			get
			{
				return C.EntityTypeCode.Page;
			}
		}

		public override string ActionCode
		{
			get
			{
				return C.ActionCode.SelectPageForObjectForm;
			}
		}


		public virtual string GetDataAction
		{
			get
			{
				return "_SelectPages";
			}
		}

		#endregion
	}
}