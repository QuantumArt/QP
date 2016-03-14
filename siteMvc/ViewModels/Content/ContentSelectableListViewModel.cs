using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Quantumart.QP8.BLL.ListItems;
using C = Quantumart.QP8.Constants;
using Quantumart.QP8.BLL.Services.DTO;
using Quantumart.QP8.BLL.Services;

namespace Quantumart.QP8.WebMvc.ViewModels.Content
{
	public class ContentSelectableListViewModel : ListViewModel
	{
		public List<ContentListItem> Data { get; set; }

		public string ParentName { get; set; }

		public string GroupName { get; set; }

		public bool IsMultiple { get; set; }

		#region creation


		public ContentSelectableListViewModel(ContentInitListResult result, string tabId, int parentId, int[] IDs)
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
				return C.EntityTypeCode.Content;
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

		public override string ActionCode
		{
			get
			{
				return IsMultiple ? C.ActionCode.MultipleSelectContent : C.ActionCode.SelectContent ;
			}
		}


		public virtual string GetDataAction
		{
			get
			{
				return IsMultiple ? "_MultipleSelect" : "_Select";
			}
		}

		#endregion

	}
}