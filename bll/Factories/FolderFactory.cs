using Quantumart.QP8.BLL.Repository;
using Quantumart.QP8.Constants;

namespace Quantumart.QP8.BLL.Factories
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

    public class SiteFolderFactory : FolderFactory
	{
        public override Folder CreateFolder()
        {
			return new SiteFolder();
        }

		public override FolderRepository CreateRepository()
		{
			return new SiteFolderRepository{ Factory = this };
		}
    }

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
