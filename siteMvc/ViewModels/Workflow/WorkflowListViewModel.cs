using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Quantumart.QP8.BLL.ListItems;
using Quantumart.QP8.BLL.Services.DTO;
using Quantumart.QP8.Resources;
using Quantumart.QP8.WebMvc.ViewModels.Workflow;
using C = Quantumart.QP8.Constants;

namespace Quantumart.QP8.WebMvc.ViewModels.Workflow
{
	public class WorkflowListViewModel : ListViewModel
	{
		public IEnumerable<WorkflowListItem> Data { get; set; }

		public string GettingDataActionName
		{
			get
			{
				return "_Index";
			}
		}

		public override string EntityTypeCode
		{
			get { return C.EntityTypeCode.Workflow; }
		}

		public override string ActionCode
		{
			get { return C.ActionCode.Workflows; }
		}

		public override string AddNewItemActionCode
		{
			get
			{
				return C.ActionCode.AddNewWorkflow;
			}
		}

		public override string AddNewItemText
		{
			get
			{
				return WorkflowStrings.AddNewWorkflow;
			}
		}

		public static WorkflowListViewModel Create(WorkflowInitListResult result, string tabId, int parentId)
		{
			var model = ViewModel.Create<WorkflowListViewModel>(tabId, parentId);
			model.ShowAddNewItemButton = result.IsAddNewAccessable && !model.IsWindow;
			return model;
		}
	}
}