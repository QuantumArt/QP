using Quantumart.QP8.BLL.ListItems;
using Quantumart.QP8.BLL.Services.DTO;
using Quantumart.QP8.Resources;
using System.Collections.Generic;

namespace Quantumart.QP8.WebMvc.ViewModels.VisualEditor
{
    public class VisualEditorStyleListViewModel : ListViewModel
    {
        public IEnumerable<VisualEditorPluginListItem> Data { get; set; }

        public string GettingDataActionName => "_Index";

        public override string EntityTypeCode => Constants.EntityTypeCode.VisualEditorStyle;

        public override string ActionCode => Constants.ActionCode.VisualEditorStyles;

        public override string AddNewItemActionCode => Constants.ActionCode.AddNewVisualEditorStyle;

        public override string AddNewItemText => VisualEditorStrings.AddNewVisualEditorStyle;

        public static VisualEditorStyleListViewModel Create(VisualEditorStyleInitListResult result, string tabId, int parentId)
        {
            var model = Create<VisualEditorStyleListViewModel>(tabId, parentId);
            model.ShowAddNewItemButton = result.IsAddNewAccessable && !model.IsWindow;
            return model;
        }
    }
}
