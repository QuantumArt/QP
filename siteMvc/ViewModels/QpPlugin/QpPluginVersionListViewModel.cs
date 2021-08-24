using System.Collections.Generic;
using Quantumart.QP8.BLL.ListItems;
using Quantumart.QP8.BLL.Services.DTO;
using Quantumart.QP8.Resources;
using Quantumart.QP8.WebMvc.ViewModels.Abstract;

namespace Quantumart.QP8.WebMvc.ViewModels.QpPlugin
{
    public class QpPluginVersionListViewModel : ListViewModel
    {
        public List<QpPluginVersionListItem> Data { get; set; }

        public string GettingDataActionName => "_Index";

        public static QpPluginVersionListViewModel Create(string tabId, int parentId)
        {
            return Create<QpPluginVersionListViewModel>(tabId, parentId);
        }

        public override string EntityTypeCode => Constants.EntityTypeCode.QpPluginVersion;

        public override string ActionCode => Constants.ActionCode.QpPluginVersions;

        public override bool IsReadOnly => base.IsReadOnly || IsSelect;

    }
}
