using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Quantumart.QP8.BLL.Repository;
using Quantumart.QP8.BLL.Services.DTO;
using Quantumart.QP8.BLL.Factories;
using Quantumart.QP8.Resources;
using Quantumart.QP8.Constants;

namespace Quantumart.QP8.BLL.Services
{
    public class SiteFolderService
    {
		#region private

		private static PathInfo _GetPathInfo(int id)
		{
			return SiteFolder.GetPathInfo(id);
		}

		#endregion		

		public static Folder GetById(int id)
		{
			FolderFactory factory = FolderFactory.Create(EntityTypeCode.SiteFolder);
			Folder folder = factory.CreateRepository().GetById(id);
			if (folder == null)
				throw new Exception(String.Format(LibraryStrings.SiteFolderNotExists, id));
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

		public static SiteFolder New(int siteId, int parentFolderId)
		{
			SiteFolder result = (SiteFolder)FolderFactory.Create(EntityTypeCode.SiteFolder).CreateFolder();
			result.ParentId = parentFolderId;
			result.SiteId = siteId;
			return result;
		}

		public static SiteFolder NewForSave(int siteId, int parentFolderId)
		{
			return New(siteId, parentFolderId);
		}

		public static SiteFolder Save(SiteFolder folder)
		{
			if (folder == null)
				throw new ArgumentNullException("folder");
			return (SiteFolder)FolderFactory.Create(EntityTypeCode.SiteFolder)
				.CreateRepository()
				.Create(folder.ParentEntityId, folder.ParentId, folder.Name);			
		}

		public static SiteFolder Read(int id)
		{
			return (SiteFolder)GetById(id);
		}

		public static SiteFolder ReadForUpdate(int id)
		{
			return Read(id);
		}

		public static SiteFolder Update(SiteFolder siteFolder)
		{
			if (!siteFolder.ParentId.HasValue)
				throw new ApplicationException(FolderStrings.CanUpdateRootFolder);
			return (SiteFolder)FolderFactory.Create(EntityTypeCode.SiteFolder)
				.CreateRepository()
				.Update(siteFolder);
		}

		public static MessageResult Remove(int id)
		{
			FolderRepository repository = FolderFactory.Create(EntityTypeCode.SiteFolder).CreateRepository();
			Folder folder = repository.GetById(id);								
			if (folder == null)
				throw new ApplicationException(String.Format(FolderStrings.FolderNotFound, id));
			else if (!folder.ParentId.HasValue)
				throw new ApplicationException(FolderStrings.CanDeleteRootFolder);
			repository.Delete(folder);
			return null;
		}

		public static MessageResult RemovePreAction(int id)
		{
			Folder folder = FolderFactory.Create(EntityTypeCode.SiteFolder)
					.CreateRepository()
					.GetById(id);					
			if (folder.IsEmpty)
				return MessageResult.Confirm(String.Format(FolderStrings.FolderIsNotEmptyConfirm, folder.Name), new[] { id });
			else
				return null;
		}
	}
}
