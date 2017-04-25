using System;
using Quantumart.QP8.BLL.Repository;
using Quantumart.QP8.BLL.Services.DTO;
using Quantumart.QP8.BLL.Factories;
using Quantumart.QP8.Resources;
using Quantumart.QP8.Constants;


namespace Quantumart.QP8.BLL.Services
{
    public class ContentFolderService
	{
		#region private

		private static PathInfo _GetPathInfo(int id)
		{
			return ContentFolder.GetPathInfo(id);
		}

		#endregion

		public static Folder GetById(int id)
		{
			FolderFactory factory = new ContentFolderFactory();
			Folder folder = factory.CreateRepository().GetById(id);
			if (folder == null)
				throw new Exception(String.Format(LibraryStrings.ContentFolderNotExists, id));
			return folder;
		}

		public static FolderFile GetFile(int id, string fileName)
		{
			return _GetPathInfo(id).GetFile(fileName);
		}

		public static string GetPath(int id, string fileName)
		{
			return _GetPathInfo(id).GetPath(fileName);
		}

		public static PathInfo GetPathInfo(int id)
		{
			return _GetPathInfo(id);
		}

		public static void SaveFile(FolderFile file)
		{
			if (file == null)
				throw new ArgumentNullException("file");
			file.Rename();
		}

		public static MessageResult RemoveFiles(int id, string[] names)
		{
			if (names == null)
				throw new ArgumentNullException("names");
			PathInfo info = _GetPathInfo(id);
			foreach (string name in names)
			{
				FolderFile file = info.GetFile(name);
				if (file != null)
					file.Remove();
			}
			return null;
		}

		public static ContentFolder New(int contentId, int parentFolderId)
		{
			ContentFolder result = (ContentFolder)FolderFactory.Create(EntityTypeCode.ContentFolder).CreateFolder();
			result.ParentId = parentFolderId;
			result.ContentId = contentId;
			return result;
		}

		public static ContentFolder NewForSave(int contentId, int parentFolderId)
		{
			return New(contentId, parentFolderId);
		}

		public static ContentFolder Save(ContentFolder folder)
		{
			if (folder == null)
				throw new ArgumentNullException("folder");
			return (ContentFolder)FolderFactory.Create(EntityTypeCode.ContentFolder)
				.CreateRepository()
				.Create(folder.ParentEntityId, folder.ParentId, folder.Name);
		}

		public static ContentFolder Read(int id)
		{
			return (ContentFolder)GetById(id);
		}

		public static ContentFolder ReadForUpdate(int id)
		{
			return Read(id);
		}

		public static ContentFolder Update(ContentFolder folder)
		{
			if (!folder.ParentId.HasValue)
				throw new ApplicationException(FolderStrings.CanUpdateRootFolder);
			return (ContentFolder)FolderFactory.Create(EntityTypeCode.ContentFolder)
				.CreateRepository()
				.Update(folder);
		}

		public static MessageResult Remove(int id)
		{
			FolderRepository repository = FolderFactory.Create(EntityTypeCode.ContentFolder).CreateRepository();
			Folder folder = repository.GetById(id);			
			if (folder == null)
				throw new ApplicationException(String.Format(FolderStrings.FolderNotFound, id));
			else if(!folder.ParentId.HasValue)
				throw new ApplicationException(FolderStrings.CanDeleteRootFolder);
			repository.Delete(folder);
			return null;
		}

		public static MessageResult RemovePreAction(int id)
		{
			Folder folder = FolderFactory.Create(EntityTypeCode.ContentFolder)
					.CreateRepository()
					.GetById(id);
			if (folder.IsEmpty)
				return MessageResult.Confirm(String.Format(FolderStrings.FolderIsNotEmptyConfirm, folder.Name), new[] { id });
			else
				return null;
		}

	}
}
