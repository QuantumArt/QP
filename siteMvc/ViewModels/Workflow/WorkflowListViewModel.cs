using System.Collections.Generic;
using Quantumart.QP8.BLL.ListItems;
using Quantumart.QP8.BLL.Services.DTO;
using Quantumart.QP8.Resources;
using Quantumart.QP8.WebMvc.ViewModels.Abstract;

namespace Quantumart.QP8.WebMvc.ViewModels.Workflow
{
    public class WorkflowListViewModel : ListViewModel
    {
        public IEnumerable<WorkflowListItem> Data { get; set; }

        public string GettingDataActionName => "_Index";

        public override string EntityTypeCode => Constants.EntityTypeCode.Workflow;

        public override string ActionCode => Constants.ActionCode.Workflows;

        public override string AddNewItemActionCode => Constants.ActionCode.AddNewWorkflow;

        public override string AddNewItemText => WorkflowStrings.AddNewWorkflow;

        public static WorkflowListViewModel Create(InitListResult result, string tabId, int parentId)
        {
            var model = Create<WorkflowListViewModel>(tabId, parentId);
            model.ShowAddNewItemButton = result.IsAddNewAccessable && !model.IsWindow;
            return model;
        }
    }
}
