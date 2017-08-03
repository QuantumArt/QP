using Quantumart.QP8.BLL.Repository;

namespace Quantumart.QP8.BLL.Factories.FolderFactory
{
    public class ContentFolderFactory : FolderFactory
    {
        public override Folder CreateFolder() => new ContentFolder();

        public override FolderRepository CreateRepository() => new ContentFolderRepository { Factory = this };
    }
}
