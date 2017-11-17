using System;
using Quantumart.QP8.BLL.Factories.FolderFactory;
using Quantumart.QP8.BLL.Services.DTO;
using Quantumart.QP8.Constants;
using Quantumart.QP8.Resources;

namespace Quantumart.QP8.BLL.Services
{
    public class SiteFolderService
    {
        private static PathInfo _GetPathInfo(int id) => SiteFolder.GetPathInfo(id);

        public static Folder GetById(int id)
        {
            var factory = FolderFactory.Create(EntityTypeCode.SiteFolder);
            var folder = factory.CreateRepository().GetById(id);
            if (folder == null)
            {
                throw new Exception(string.Format(LibraryStrings.SiteFolderNotExists, id));
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

        public static SiteFolder New(int siteId, int parentFolderId)
        {
            var result = (SiteFolder)FolderFactory.Create(EntityTypeCode.SiteFolder).CreateFolder();
            result.ParentId = parentFolderId;
            result.SiteId = siteId;
            return result;
        }

        public static SiteFolder NewForSave(int siteId, int parentFolderId) => New(siteId, parentFolderId);

        public static SiteFolder Save(SiteFolder folder)
        {
            if (folder == null)
            {
                throw new ArgumentNullException(nameof(folder));
            }

            return (SiteFolder)FolderFactory.Create(EntityTypeCode.SiteFolder)
                .CreateRepository()
                .Create(folder.ParentEntityId, folder.ParentId, folder.Name);
        }

        public static SiteFolder Read(int id) => (SiteFolder)GetById(id);

        public static SiteFolder ReadForUpdate(int id) => Read(id);

        public static SiteFolder Update(SiteFolder siteFolder)
        {
            if (!siteFolder.ParentId.HasValue)
            {
                throw new ApplicationException(FolderStrings.CanUpdateRootFolder);
            }

            return (SiteFolder)FolderFactory.Create(EntityTypeCode.SiteFolder)
                .CreateRepository()
                .Update(siteFolder);
        }

        public static MessageResult Remove(int id)
        {
            var repository = FolderFactory.Create(EntityTypeCode.SiteFolder).CreateRepository();
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
            var folder = FolderFactory.Create(EntityTypeCode.SiteFolder)
                .CreateRepository()
                .GetById(id);
            if (folder.IsEmpty)
            {
                return MessageResult.Confirm(string.Format(FolderStrings.FolderIsNotEmptyConfirm, folder.Name), new[] { id });
            }

            return null;
        }
    }
}
