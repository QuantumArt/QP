using Quantumart.QP8.BLL.Services.DTO;
using Quantumart.QP8.WebMvc.ViewModels.Content;

namespace Quantumart.QP8.WebMvc.ViewModels.VirtualContent
{
    public class UnionContentViewModel : ContentSelectableListViewModel
    {
        public UnionContentViewModel(ContentInitListResult result, string tabId, int parentId, int[] ids)
            : base(result, tabId, parentId, ids)
        {
        }

        public override string ActionCode => Constants.ActionCode.MultipleSelectContentForUnion;

        public override string GetDataAction => "_MultipleSelectForUnion";
    }
}
