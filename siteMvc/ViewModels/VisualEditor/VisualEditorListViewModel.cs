using System.Collections.Generic;
using Quantumart.QP8.BLL.ListItems;
using Quantumart.QP8.BLL.Services.DTO;
using Quantumart.QP8.Resources;

namespace Quantumart.QP8.WebMvc.ViewModels.VisualEditor
{
    public class VisualEditorListViewModel : ListViewModel
    {
        public IEnumerable<VisualEditorPluginListItem> Data { get; set; }

        public string GettingDataActionName => "_Index";

        public override string EntityTypeCode => Constants.EntityTypeCode.VisualEditorPlugin;

        public override string ActionCode => Constants.ActionCode.VisualEditorPlugins;

        public override string AddNewItemActionCode => Constants.ActionCode.AddNewVisualEditorPlugin;

        public override string AddNewItemText => VisualEditorStrings.AddNewVisualEditorPlugin;

        public static VisualEditorListViewModel Create(VisualEditorInitListResult result, string tabId, int parentId)
        {
            var model = Create<VisualEditorListViewModel>(tabId, parentId);
            model.ShowAddNewItemButton = result.IsAddNewAccessable && !model.IsWindow;
            return model;
        }
    }
}
