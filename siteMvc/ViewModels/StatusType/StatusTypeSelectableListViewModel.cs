using System.Collections.Generic;
using Quantumart.QP8.BLL.ListItems;

namespace Quantumart.QP8.WebMvc.ViewModels.StatusType
{
    public class StatusTypeSelectableListViewModel : ListViewModel
    {
        public List<StatusTypeListItem> Data { get; set; }

        public string ParentName { get; set; }

        public string GroupName { get; set; }

        public StatusTypeSelectableListViewModel(string tabId, int parentId, int[] ids)
        {
            ParentEntityId = parentId;
            TabId = tabId;
            SelectedIDs = ids;
            AutoGenerateLink = false;
            ShowAddNewItemButton = !IsWindow;
        }

        public sealed override string EntityTypeCode => Constants.EntityTypeCode.StatusType;

        public override string ActionCode => Constants.ActionCode.MultipleSelectStatusesForWorkflow;

        public virtual string GetDataAction => "_MultipleSelectForWorkflow";
    }
}
