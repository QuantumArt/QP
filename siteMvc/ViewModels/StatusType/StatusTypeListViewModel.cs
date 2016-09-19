using System.Collections.Generic;
using Quantumart.QP8.BLL.ListItems;
using Quantumart.QP8.BLL.Services.DTO;
using Quantumart.QP8.Resources;

namespace Quantumart.QP8.WebMvc.ViewModels.StatusType
{
    public class StatusTypeListViewModel : ListViewModel
    {
        public IEnumerable<StatusTypeListItem> Data { get; set; }

        public string GettingDataActionName => "_Index";

        public override string EntityTypeCode => Constants.EntityTypeCode.StatusType;

        public override string ActionCode => Constants.ActionCode.StatusTypes;

        public override string AddNewItemActionCode => Constants.ActionCode.AddNewStatusType;

        public override string AddNewItemText => StatusTypeStrings.AddNewStatusType;

        public static StatusTypeListViewModel Create(StatusTypeInitListResult result, string tabId, int parentId)
        {
            var model = Create<StatusTypeListViewModel>(tabId, parentId);
            model.ShowAddNewItemButton = result.IsAddNewAccessable && !model.IsWindow;
            return model;
        }
    }
}
