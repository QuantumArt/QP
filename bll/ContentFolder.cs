using System;
using System.Collections.Generic;
using Quantumart.QP8.BLL.Repository;
using Quantumart.QP8.BLL.Factories;
using System.IO;
using Quantumart.QP8.Constants;
using Quantumart.QP8.Resources;

namespace Quantumart.QP8.BLL
{
	public class ContentFolder : Folder
    {

        #region overrides

		/// <summary>
		/// ID родительского контента
		/// </summary>
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
        #endregion

        /// <summary>
		/// идентификатор контента
		/// </summary>
		public int ContentId { get; set; }

        public Content Content
        {
            get { return (Content)Parent; }
        }

		public static PathInfo GetPathInfo(int id)
		{
			PathInfo info = Folder.GetPathInfo(new ContentFolderFactory(), id);
			if (info == null)
				throw new Exception(String.Format(LibraryStrings.ContentFolderNotExists, id));
			return info;
		}
	}
}
