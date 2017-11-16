using System;
using Quantumart.QP8.BLL.Factories.FolderFactory;
using Quantumart.QP8.BLL.Services.DTO;
using Quantumart.QP8.Constants;
using Quantumart.QP8.Resources;

namespace Quantumart.QP8.BLL.Services
{
    public class ContentFolderService
    {
        private static PathInfo _GetPathInfo(int id) => ContentFolder.GetPathInfo(id);

        public static Folder GetById(int id)
        {
            var factory = new ContentFolderFactory();
            var folder = factory.CreateRepository().GetById(id);
            if (folder == null)
            {
                throw new Exception(string.Format(LibraryStrings.ContentFolderNotExists, id));
            }

            return folder;
        }

        public static FolderFile GetFile(int id, string fileName) => _GetPathInfo(id).GetFile(fileName);

        public static string GetPath(int id, string fileName) => _GetPathInfo(id).GetPath(fileName);

        public static PathInfo GetPathInfo(int id) => _GetPathInfo(id);

        public static void SaveFile(FolderFile file)
        {
            if (file == null)
            {
                throw new ArgumentNullException(nameof(file));
            }

            file.Rename();
        }

        public static MessageResult RemoveFiles(int id, string[] names)
        {
            if (names == null)
            {
                throw new ArgumentNullException(nameof(names));
            }

            var info = _GetPathInfo(id);
            foreach (var name in names)
            {
                var file = info.GetFile(name);
                if (file != null)
                {
                    file.Remove();
                }
            }

            return null;
        }

        public static ContentFolder New(int contentId, int parentFolderId)
        {
            var result = (ContentFolder)FolderFactory.Create(EntityTypeCode.ContentFolder).CreateFolder();
            result.ParentId = parentFolderId;
            result.ContentId = contentId;
            return result;
        }

        public static ContentFolder NewForSave(int contentId, int parentFolderId) => New(contentId, parentFolderId);

        public static ContentFolder Save(ContentFolder folder)
        {
            if (folder == null)
            {
                throw new ArgumentNullException(nameof(folder));
            }

            return (ContentFolder)FolderFactory.Create(EntityTypeCode.ContentFolder).CreateRepository().Create(folder.ParentEntityId, folder.ParentId, folder.Name);
        }

        public static ContentFolder Read(int id) => (ContentFolder)GetById(id);

        public static ContentFolder ReadForUpdate(int id) => Read(id);

        public static ContentFolder Update(ContentFolder folder)
        {
            if (!folder.ParentId.HasValue)
            {
                throw new ApplicationException(FolderStrings.CanUpdateRootFolder);
            }

            return (ContentFolder)FolderFactory.Create(EntityTypeCode.ContentFolder).CreateRepository().Update(folder);
        }

        public static MessageResult Remove(int id)
        {
            var repository = FolderFactory.Create(EntityTypeCode.ContentFolder).CreateRepository();
            var folder = repository.GetById(id);
            if (folder == null)
            {
                throw new ApplicationException(string.Format(FolderStrings.FolderNotFound, id));
            }

            if (!folder.ParentId.HasValue)
            {
                throw new ApplicationException(FolderStrings.CanDeleteRootFolder);
            }

            repository.Delete(folder);
            return null;
        }

        public static MessageResult RemovePreAction(int id)
        {
            var folder = FolderFactory.Create(EntityTypeCode.ContentFolder).CreateRepository().GetById(id);
            if (folder.IsEmpty)
            {
                return MessageResult.Confirm(string.Format(FolderStrings.FolderIsNotEmptyConfirm, folder.Name), new[] { id });
            }

            return null;
        }
    }
}
