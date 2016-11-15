using Quantumart.QP8.BLL;

namespace Quantumart.QP8.WebMvc.ViewModels.Library
{
    public class ContentFolderViewModel : FolderViewModel
    {
        internal static ContentFolderViewModel Create(ContentFolder folder, string tabId, int parentId)
        {
            return Create<ContentFolderViewModel>(folder, tabId, parentId);
        }

        public new ContentFolder Data
        {
            get
            {
                return (ContentFolder)EntityData;
            }
            set
            {
                EntityData = value;
            }
        }

        public override string EntityTypeCode => Constants.EntityTypeCode.ContentFolder;

        public override string ActionCode => IsNew ? Constants.ActionCode.AddNewContentFolder : Constants.ActionCode.ContentFolderProperties;
    }
}
