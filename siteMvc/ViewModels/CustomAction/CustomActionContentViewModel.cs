using Quantumart.QP8.BLL.Services.DTO;
using Quantumart.QP8.WebMvc.ViewModels.Content;

namespace Quantumart.QP8.WebMvc.ViewModels.CustomAction
{
    public class CustomActionContentViewModel : ContentSelectableListViewModel
    {
        public CustomActionContentViewModel(ContentInitListResult result, string tabId, int parentId, int[] ids)
            : base(result, tabId, parentId, ids)
        {
        }

        public override string ActionCode => Constants.ActionCode.MultipleSelectContentForCustomAction;

        public override string GetDataAction => "_MultipleSelectForCustomAction";
    }
}
