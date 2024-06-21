using System;
using System.Linq;
using System.Web;
using Quantumart.QP8.BLL.Factories.FolderFactory;
using Quantumart.QP8.BLL.Helpers;
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

        public static FolderFile GetFile(int id, string fileName, PathHelper pathHelper)
        {
            var info = _GetPathInfo(id);
            info.PathHelper = pathHelper;
            return info.GetFile(fileName);
        }

        public static string GetPath(int id, string fileName) => _GetPathInfo(id).GetPath(fileName);

        public static PathInfo GetPathInfo(int id) => _GetPathInfo(id);

        public static void SaveFile(FolderFile file, PathHelper pathHelper)
        {
            if (file == null)
            {
                throw new ArgumentNullException(nameof(file));
            }

            file.Rename(pathHelper);
        }

        public static MessageResult RemoveFiles(int id, string[] names, PathHelper pathHelper)
        {
            if (names == null)
            {
                throw new ArgumentNullException(nameof(names));
            }

            var info = _GetPathInfo(id);
            info.PathHelper = pathHelper;
            foreach (var name in names)
            {
                var file = info.GetFile(HttpUtility.UrlDecode(name));
                if (file != null)
                {
                    file.Remove(pathHelper);
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

        public static ContentFolder Save(ContentFolder folder, PathHelper pathHelper)
        {
            if (folder == null)
            {
                throw new ArgumentNullException(nameof(folder));
            }

            return (ContentFolder)FolderFactory.Create(EntityTypeCode.ContentFolder).CreateRepository().
                Create(folder.ParentEntityId, folder.ParentId, folder.Name, pathHelper);
        }

        public static ContentFolder Read(int id) => (ContentFolder)GetById(id);

        public static ContentFolder ReadForUpdate(int id) => Read(id);

        public static ContentFolder Update(ContentFolder folder, PathHelper pathHelper)
        {
            var oldFolder = GetById(folder.Id);
            if (!oldFolder.ParentId.HasValue)
            {
                throw new ApplicationException(FolderStrings.CanUpdateRootFolder);
            }

            if (pathHelper.UseS3)
            {
                var files = pathHelper.ListS3Files(oldFolder.PathInfo.Path, true);
                if (files.Any())
                {
                    throw new ApplicationException(FolderStrings.CannotRenameNonEmptyFolder);
                }
            }

            return (ContentFolder)FolderFactory.Create(EntityTypeCode.ContentFolder).CreateRepository()
                .Update(folder, pathHelper);
        }

        public static MessageResult Remove(int id, PathHelper pathHelper)
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

            repository.Delete(folder, pathHelper);
            return null;
        }

        public static MessageResult RemovePreAction(int id, PathHelper pathHelper)
        {
            var folder = FolderFactory.Create(EntityTypeCode.ContentFolder).CreateRepository().GetById(id);
            if (folder.IsEmpty(pathHelper))
            {
                return MessageResult.Confirm(string.Format(FolderStrings.FolderIsNotEmptyConfirm, folder.Name), new[] { id });
            }

            return null;
        }
    }
}
