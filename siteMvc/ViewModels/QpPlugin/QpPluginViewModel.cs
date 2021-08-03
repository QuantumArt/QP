using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Quantumart.QP8.BLL;
using Quantumart.QP8.Resources;
using Quantumart.QP8.WebMvc.ViewModels.Abstract;

namespace Quantumart.QP8.WebMvc.ViewModels.QpPlugin
{
    public class QpPluginViewModel : EntityViewModel
    {
        public static QpPluginViewModel Create(BLL.QpPlugin plugin, string tabId, int parentId)
        {
            var model = Create<QpPluginViewModel>(plugin, tabId, parentId);
            model.CreationMode = QpPluginCreationMode.ByServiceUrl;
            return model;
        }

        public BLL.QpPlugin Data
        {
            get => (BLL.QpPlugin)EntityData;
            set => EntityData = value;
        }

        public override string EntityTypeCode => Constants.EntityTypeCode.QpPlugin;
        public override string ActionCode  => IsNew ? Constants.ActionCode.AddNewQpPlugin : Constants.ActionCode.QpPluginProperties;

        [Display(Name = "PluginCreationMode", ResourceType = typeof(QpPluginStrings))]
        public QpPluginCreationMode CreationMode { get; set; }

        public IEnumerable<ListItem> GetCreationModes() => new[]
        {
            new ListItem(QpPluginCreationMode.ByServiceUrl.ToString(), QpPluginStrings.ByServiceUrl, "ByServiceUrlPanel"),
            new ListItem(QpPluginCreationMode.ByContract.ToString(), QpPluginStrings.ByContract, "ByContractPanel")
        };
    }
}
