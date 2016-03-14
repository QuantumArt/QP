using Quantumart.QP8.BLL.ListItems;
using Quantumart.QP8.BLL.Services.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using C = Quantumart.QP8.Constants;

namespace Quantumart.QP8.WebMvc.ViewModels.StatusType
{
	public class StatusTypeSelectableListViewModel : ListViewModel
	{
		public List<StatusTypeListItem> Data { get; set; }

		public string ParentName { get; set; }

		public string GroupName { get; set; }		

		#region creation


		public StatusTypeSelectableListViewModel(StatusTypeInitListResult result, string tabId, int parentId, int[] IDs)
		{
			this.ParentEntityId = parentId;
			this.TabId = tabId;
//			this.ParentName = result.ParentName;
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
				return C.EntityTypeCode.StatusType;
			}
		}

		public override string ActionCode
		{
			get
			{
				return C.ActionCode.MultipleSelectStatusesForWorkflow;
			}
		}


		public virtual string GetDataAction
		{
			get
			{
				return "_MultipleSelectForWorkflow";
			}
		}

		#endregion

	}
}