using Quantumart.QP8.BLL.Repository;
using Quantumart.QP8.Constants;

namespace Quantumart.QP8.BLL.Factories.FolderFactory
{
    public abstract class FolderFactory
    {
        public abstract Folder CreateFolder();

        public abstract FolderRepository CreateRepository();

        public static FolderFactory Create(string typeCode)
        {
            if (typeCode == EntityTypeCode.SiteFolder)
            {
                return new SiteFolderFactory();
            }

            return new ContentFolderFactory();
        }
    }
}
