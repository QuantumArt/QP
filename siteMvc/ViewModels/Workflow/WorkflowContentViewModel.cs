using Quantumart.QP8.BLL.Services.DTO;
using Quantumart.QP8.WebMvc.ViewModels.Content;

namespace Quantumart.QP8.WebMvc.ViewModels.Workflow
{
    public class WorkflowContentViewModel : ContentSelectableListViewModel
    {
        public WorkflowContentViewModel(ContentInitListResult result, string tabId, int parentId, int[] ids)
            : base(result, tabId, parentId, ids)
        {
        }

        public override string ActionCode => Constants.ActionCode.MultipleSelectContentForWorkflow;

        public override string GetDataAction => "_MultipleSelectForWorkflow";
    }
}
