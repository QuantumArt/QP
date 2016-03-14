using System.Collections.Generic;
using Quantumart.QP8.BLL.ListItems;
using Quantumart.QP8.BLL.Services.DTO;
using Quantumart.QP8.Resources;
using Quantumart.QP8.WebMvc.ViewModels.Workflow;
using C = Quantumart.QP8.Constants;

namespace Quantumart.QP8.WebMvc.ViewModels.StatusType
{
	public class StatusTypeListViewModel : ListViewModel
	{
		public IEnumerable<StatusTypeListItem> Data { get; set; }

		public string GettingDataActionName
		{
			get
			{
				return "_Index";
			}
		}

		public override string EntityTypeCode
		{
			get { return C.EntityTypeCode.StatusType; }
		}

		public override string ActionCode
		{
			get { return C.ActionCode.StatusTypes; }
		}

		public override string AddNewItemActionCode
		{
			get
			{
				return C.ActionCode.AddNewStatusType;
			}
		}

		public override string AddNewItemText
		{
			get
			{
				return StatusTypeStrings.AddNewStatusType;
			}
		}

		public static StatusTypeListViewModel Create(StatusTypeInitListResult result, string tabId, int parentId)
		{
			var model = ViewModel.Create<StatusTypeListViewModel>(tabId, parentId);
			model.ShowAddNewItemButton = result.IsAddNewAccessable && !model.IsWindow;
			return model;
		}
	}
}