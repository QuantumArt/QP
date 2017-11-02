using Quantumart.QP8.BLL;

namespace Quantumart.QP8.WebMvc.ViewModels.Library
{
    public class SiteFolderViewModel : FolderViewModel
    {
        internal static SiteFolderViewModel Create(SiteFolder folder, string tabId, int parentId) => Create<SiteFolderViewModel>(folder, tabId, parentId);

        public new SiteFolder Data
        {
            get => (SiteFolder)EntityData;
            set => EntityData = value;
        }

        public override string EntityTypeCode => Constants.EntityTypeCode.SiteFolder;

        public override string ActionCode => IsNew ? Constants.ActionCode.AddNewSiteFolder : Constants.ActionCode.SiteFolderProperties;
    }
}
