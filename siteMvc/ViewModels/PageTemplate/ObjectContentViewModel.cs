using Quantumart.QP8.BLL.Services.DTO;
using Quantumart.QP8.Constants;
using Quantumart.QP8.WebMvc.ViewModels.Content;

namespace Quantumart.QP8.WebMvc.ViewModels.PageTemplate
{
    public class ObjectContentViewModel : ContentSelectableListViewModel
    {
        public ObjectContentViewModel(ContentInitListResult result, string tabId, int parentId, int[] ids, ContentSelectMode selectMode)
            : base(result, tabId, parentId, ids)
        {
            _selectMode = selectMode;
        }

        private readonly ContentSelectMode _selectMode;

        public override string ActionCode => _selectMode == ContentSelectMode.ForForm ? Constants.ActionCode.SelectContentForObjectForm : Constants.ActionCode.SelectContentForObjectContainer;

        public override string GetDataAction => _selectMode == ContentSelectMode.ForForm ? "_SelectForObjectForm" : "_SelectForObjectContainer";
    }
}
