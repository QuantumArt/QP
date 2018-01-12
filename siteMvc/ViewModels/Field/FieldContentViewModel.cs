using Quantumart.QP8.BLL.Services.DTO;
using Quantumart.QP8.WebMvc.ViewModels.Content;

namespace Quantumart.QP8.WebMvc.ViewModels.Field
{
    public class FieldContentViewModel : ContentSelectableListViewModel
    {
        public FieldContentViewModel(ContentInitListResult result, string tabId, int parentId, int[] ids)
            : base(result, tabId, parentId, ids)
        {
        }

        public override string ActionCode => Constants.ActionCode.SelectContentForField;

        public override string GetDataAction => "_SelectForField";
    }
}
