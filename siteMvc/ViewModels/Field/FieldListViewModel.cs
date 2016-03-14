using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using B = Quantumart.QP8.BLL;
using Quantumart.QP8.Resources;
using C = Quantumart.QP8.Constants;
using Quantumart.QP8.BLL.Services.DTO;
using Quantumart.QP8.BLL.ListItems;


namespace Quantumart.QP8.WebMvc.ViewModels.Field
{
	public class FieldListViewModel : ListViewModel
	{
		public List<FieldListItem> Data { get; set; }

		public string ParentName { get; set; }

		#region creation

		public static FieldListViewModel Create(FieldInitListResult result, string tabId, int parentId)
		{
			FieldListViewModel model = ViewModel.Create<FieldListViewModel>(tabId, parentId);
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
				return (IsVirtual) ? C.EntityTypeCode.VirtualField : C.EntityTypeCode.Field;
			}
		}

		public override string ActionCode
		{
			get
			{
				return (!IsVirtual) ? C.ActionCode.Fields : C.ActionCode.VirtualFields;
			}
		}

		public override string ContextMenuCode
		{
			get
			{
				return (IsVirtual) ? Constants.EntityTypeCode.VirtualField : Constants.EntityTypeCode.Field;
			}
		}

		public override string AddNewItemText
		{
			get
			{
				return ContentStrings.Link_AddNewField;
			}
		}

		public override string AddNewItemActionCode
		{
			get
			{
				return Constants.ActionCode.AddNewField;
			}
		}

		#endregion

	}
}

