using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using B = Quantumart.QP8.BLL;
using Quantumart.QP8.Resources;
using C = Quantumart.QP8.Constants;
using Quantumart.QP8.BLL.ListItems;
using Quantumart.QP8.BLL.Services.DTO;
using Quantumart.QP8.BLL.Services;


namespace Quantumart.QP8.WebMvc.ViewModels
{
	public class ContentListViewModel : ListViewModel
	{
		public List<ContentListItem> Data { get; set; }

		public string ParentName { get; set; }

		public string GroupName { get; set; }

		#region creation

		public static ContentListViewModel Create(ContentInitListResult result, string tabId, int parentId)
		{
			ContentListViewModel model = ViewModel.Create<ContentListViewModel>(tabId, parentId);
            model.AllowMultipleEntitySelection = false;
			model.ParentName = result.ParentName;
			model.IsVirtual = result.IsVirtual;
			model.ShowAddNewItemButton = result.IsAddNewAccessable && !model.IsWindow;
			return model;
		}

		#endregion

		#region read-only members

		public override string EntityTypeCode
		{
			get
			{
				return (IsVirtual) ? C.EntityTypeCode.VirtualContent : C.EntityTypeCode.Content;
			}
		}

		public override string ActionCode
		{
			get
			{
				return (!IsVirtual) ? C.ActionCode.Contents : C.ActionCode.VirtualContents;
			}
		}

		public override string ContextMenuCode
		{
			get
			{
				return (IsVirtual) ? Constants.EntityTypeCode.VirtualContent : Constants.EntityTypeCode.Content;
			}
		}

		public string GetDataAction
		{
			get
			{
				return (IsVirtual) ? "_VirtualIndex" : "_Index";
			}
		}

		public override string AddNewItemText
		{
			get
			{
				return ContentStrings.Link_AddNewContent;
			}
		}

		public override string AddNewItemActionCode
		{
			get
			{
				return Constants.ActionCode.AddNewContent;
			}
		}

		#endregion

	}
}

