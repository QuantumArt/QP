using Quantumart.QP8.BLL.Repository;

namespace Quantumart.QP8.BLL.Factories.FolderFactory
{
    public class ContentFolderFactory : FolderFactory
    {
        public override Folder CreateFolder()
        {
            return new ContentFolder();
        }

        public override FolderRepository CreateRepository()
        {
            return new ContentFolderRepository { Factory = this };
        }
    }
}
