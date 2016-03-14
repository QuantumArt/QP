using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Quantumart.QP8.BLL.ListItems;
using C = Quantumart.QP8.Constants;
using Quantumart.QP8.BLL.Services.DTO;
using Quantumart.QP8.BLL.Services;

namespace Quantumart.QP8.WebMvc.ViewModels.Field
{
	public class FieldSelectableListViewModel : ListViewModel
	{
		public List<FieldListItem> Data { get; set; }

		public string ParentName { get; set; }

		public string GroupName { get; set; }

		public bool IsMultiple { get; set; }

		#region creation


		public FieldSelectableListViewModel(FieldInitListResult result, string tabId, int parentId, int[] IDs, string actionCode)
		{
			this.ParentEntityId = parentId;
			this.TabId = tabId;
			this.ParentName = result.ParentName;
			this.SelectedIDs = IDs;
			this.AutoGenerateLink = false;
			this.ShowAddNewItemButton = !IsWindow;
			this.actionCode = actionCode;
		}

		#endregion

		#region read-only members

		public sealed override string EntityTypeCode
		{
			get
			{
				return C.EntityTypeCode.Field;
			}
		}

		public override bool AllowMultipleEntitySelection
		{
			get
			{
				return IsMultiple;
			}
			set
			{
			}
		}

		private string actionCode = null;

		public override string ActionCode
		{
			get { return actionCode; }
		}


		public virtual string GetDataAction
		{
			get
			{
				if (actionCode == C.ActionCode.MultipleSelectFieldForExport)
					return "_MultipleSelectForExport";
				else if (actionCode == C.ActionCode.MultipleSelectFieldForExportExpanded)
					return "_MultipleSelectForExportExpanded";
				else
					return String.Empty;
			}
		}

		#endregion

	}
}