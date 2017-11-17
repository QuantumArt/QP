using System;
using Quantumart.QP8.BLL.Factories.FolderFactory;
using Quantumart.QP8.BLL.Repository.ContentRepositories;
using Quantumart.QP8.Resources;

namespace Quantumart.QP8.BLL
{
    public class ContentFolder : Folder
    {
        public override int ParentEntityId
        {
            get => ContentId;
            set => ContentId = value;
        }

        protected override EntityObject GetParent() => ContentRepository.GetById(ContentId);

        protected override FolderFactory GetFactory() => new ContentFolderFactory();

        public override string EntityTypeCode => Constants.EntityTypeCode.ContentFolder;

        public int ContentId { get; set; }

        public Content Content => (Content)Parent;

        public static PathInfo GetPathInfo(int id)
        {
            var info = GetPathInfo(new ContentFolderFactory(), id);
            if (info == null)
            {
                throw new Exception(string.Format(LibraryStrings.ContentFolderNotExists, id));
            }

            return info;
        }
    }
}
