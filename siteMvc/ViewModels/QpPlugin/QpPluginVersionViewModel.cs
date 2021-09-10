using System.Dynamic;
using Quantumart.QP8.WebMvc.ViewModels.Abstract;
using Quantumart.QP8.WebMvc.ViewModels.ArticleVersion;

namespace Quantumart.QP8.WebMvc.ViewModels.QpPlugin
{
    public class QpPluginVersionViewModel : EntityViewModel
    {
        public QpPluginVersionViewType ViewType { get; set; }

        public BLL.QpPluginVersion Data
        {
            get => (BLL.QpPluginVersion)EntityData;
            set => EntityData = value;
        }

        public QpPluginVersionViewModel()
        {
            ViewType = QpPluginVersionViewType.Preview;
        }

        public static QpPluginVersionViewModel Create(BLL.QpPluginVersion version, string tabId, int parentId, bool? boundToExternal) => Create(version, tabId, parentId, string.Empty, boundToExternal);

        public static QpPluginVersionViewModel Create(BLL.QpPluginVersion version, string tabId, int parentId, string succesfulActionCode, bool? boundToExternal)
        {
            var model = Create<QpPluginVersionViewModel>(version, tabId, parentId);
            model.SuccesfulActionCode = succesfulActionCode;
            model.BoundToExternal = boundToExternal;
            return model;
        }

        public bool? BoundToExternal { get; set; }

        public override string Id => IsComparison ? "0" : base.Id;

        public override string EntityTypeCode => Constants.EntityTypeCode.QpPluginVersion;

        public override string ActionCode
        {
            get
            {
                switch (ViewType)
                {
                    case QpPluginVersionViewType.CompareWithCurrent:
                        return Constants.ActionCode.CompareQpPluginVersionWithCurrent;
                    case QpPluginVersionViewType.CompareVersions:
                        return Constants.ActionCode.CompareQpPluginVersions;
                }

                return Constants.ActionCode.PreviewQpPluginVersion;
            }
        }

        public bool IsComparison => Data.VersionToMerge != null;

        public override ExpandoObject MainComponentParameters
        {
            get
            {
                dynamic result = base.MainComponentParameters;
                if (IsComparison)
                {
                    var firstId = Id;
                    var secondId = Data.VersionToMerge.Id.ToString();
                    result.entities = new[] { new ClientEntity { Id = firstId, Name = firstId }, new ClientEntity { Id = secondId, Name = secondId } };
                }

                return result;
            }
        }

        public override void Validate()
        {
            Data.Plugin.Validate();
        }

    }
}
