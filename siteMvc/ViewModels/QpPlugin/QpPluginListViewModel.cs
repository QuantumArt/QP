using System.Collections.Generic;
using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.ListItems;
using Quantumart.QP8.BLL.Services.DTO;
using Quantumart.QP8.Resources;
using Quantumart.QP8.WebMvc.ViewModels.Abstract;

namespace Quantumart.QP8.WebMvc.ViewModels.QpPlugin
{
    public class QpPluginListViewModel : ListViewModel
    {
        public List<QpPluginListItem> Data { get; set; }

        public string GettingDataActionName => "_Index";

        public static QpPluginListViewModel Create(InitListResult result, string tabId, int parentId, int[] ids = null)
        {
            var model = Create<QpPluginListViewModel>(tabId, parentId);
            model.SelectedIDs = ids;
            model.ShowAddNewItemButton = result.IsAddNewAccessable && !model.IsWindow;
            return model;
        }

        public override string EntityTypeCode => Constants.EntityTypeCode.QpPlugin;

        public override string ActionCode => Constants.ActionCode.QpPlugins;

        public override string AddNewItemText => QpPluginStrings.AddNewPlugin;

        public override string AddNewItemActionCode => Constants.ActionCode.AddNewQpPlugin;

        public override bool IsReadOnly => base.IsReadOnly || IsSelect;

    }
}
