using Quantumart.QP8.BLL.Services.DTO;
using Quantumart.QP8.WebMvc.ViewModels.Content;
using C = Quantumart.QP8.Constants;

namespace Quantumart.QP8.WebMvc.ViewModels.VirtualContent
{
    public class JoinContentViewModel : ContentSelectableListViewModel
    {
        public JoinContentViewModel(ContentInitListResult result, string tabId, int parentId, int[] IDs)
            : base(result, tabId, parentId, IDs)
        {
        }

        public override string ActionCode => C.ActionCode.SelectContentForJoin;

        public override string GetDataAction => "_SelectForJoin";
    }
}
