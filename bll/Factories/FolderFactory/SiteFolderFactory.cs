using Quantumart.QP8.BLL.Repository;

namespace Quantumart.QP8.BLL.Factories.FolderFactory
{
    public class SiteFolderFactory : FolderFactory
    {
        public override Folder CreateFolder()
        {
            return new SiteFolder();
        }

        public override FolderRepository CreateRepository()
        {
            return new SiteFolderRepository { Factory = this };
        }
    }
}
