using System;
using Quantumart.QP8.BLL.Factories.FolderFactory;
using Quantumart.QP8.BLL.Repository;
using Quantumart.QP8.Resources;

namespace Quantumart.QP8.BLL
{
    public class ContentFolder : Folder
    {
        public override int ParentEntityId
        {
            get { return ContentId; }
            set { ContentId = value; }
        }

        protected override EntityObject GetParent()
        {
            return ContentRepository.GetById(ContentId);
        }

        protected override FolderFactory GetFactory()
        {
            return new ContentFolderFactory();
        }

        public override string EntityTypeCode
        {
            get
            {
                return Constants.EntityTypeCode.ContentFolder;
            }
        }

        public int ContentId { get; set; }

        public Content Content
        {
            get { return (Content)Parent; }
        }

        public static PathInfo GetPathInfo(int id)
        {
            var info = GetPathInfo(new ContentFolderFactory(), id);
            if (info == null)
            {
                throw new Exception(String.Format(LibraryStrings.ContentFolderNotExists, id));
            }

            return info;
        }
    }
}
