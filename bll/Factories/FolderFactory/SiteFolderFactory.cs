using Quantumart.QP8.BLL.Repository;

namespace Quantumart.QP8.BLL.Factories.FolderFactory
{
    public class SiteFolderFactory : FolderFactory
    {
        public override Folder CreateFolder() => new SiteFolder();

        public override FolderRepository CreateRepository() => new SiteFolderRepository { Factory = this };
    }
}
