using System.Collections.Generic;
using Quantumart.QP8.BLL.ListItems;
using Quantumart.QP8.BLL.Services.DTO;
using Quantumart.QP8.Resources;
using Quantumart.QP8.WebMvc.ViewModels.Abstract;

namespace Quantumart.QP8.WebMvc.ViewModels.CustomAction
{
    public class CustomActionListViewModel : ListViewModel
    {
        public IEnumerable<CustomActionListItem> Data { get; set; }

        public string GettingDataActionName => "_Index";

        public static CustomActionListViewModel Create(CustomActionInitListResult initList, string tabId, int parentId)
        {
            var model = Create<CustomActionListViewModel>(tabId, parentId);
            model.ShowAddNewItemButton = initList.IsAddNewAccessable && !model.IsWindow;
            return model;
        }

        public override string EntityTypeCode => Constants.EntityTypeCode.CustomAction;

        public override string ActionCode => Constants.ActionCode.CustomActions;

        public override string AddNewItemActionCode => Constants.ActionCode.AddNewCustomAction;

        public override string AddNewItemText => CustomActionStrings.AddNewCustomAction;
    }
}
