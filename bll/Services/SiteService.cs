using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Quantumart.QP8.Assembling;
using Quantumart.QP8.BLL.Factories.FolderFactory;
using Quantumart.QP8.BLL.ListItems;
using Quantumart.QP8.BLL.Repository;
using Quantumart.QP8.BLL.Services.DTO;
using Quantumart.QP8.BLL.Services.VisualEditor;
using Quantumart.QP8.Configuration;
using Quantumart.QP8.Constants;
using Quantumart.QP8.Resources;

namespace Quantumart.QP8.BLL.Services
{
    public class SiteService
    {

        private static HttpContext HttpContext => new HttpContextAccessor().HttpContext;

        public static SiteInitListResult InitList(int parentId) => new SiteInitListResult
        {
            IsAddNewAccessable = SecurityRepository.IsActionAccessible(ActionCode.AddNewSite)
        };

        public static SiteInitListResult MultipleInitList(int parentId) => new SiteInitListResult
        {
            IsAddNewAccessable = false
        };

        public static ListResult<SiteListItem> List(ListCommand cmd, IEnumerable<int> selectedIDs = null) => SiteRepository.GetList(cmd, selectedIDs);

        public static IEnumerable<ListItem> GetAllSites()
        {
            return SiteRepository.GetAll().Select(s => new ListItem(s.Id.ToString(), s.Name)).ToArray();
        }

        public static IEnumerable<ListItem> GetSites()
        {
            var cmd = new ListCommand { PageSize = int.MaxValue, StartPage = 1 };
            return SiteRepository.GetList(cmd, null).Data.Select(s => new ListItem(s.Id.ToString(), s.Name)).ToArray();
        }

        public static Site Read(int id) => Read(id, true);

        public static Site ReadForUpdate(int id) => Read(id, false);

        public static Site Save(Site item, int[] activeCommands, int[] activeStyles)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            if (item.ExternalDevelopment)
            {
                item.NullifyField();
            }

            var result = SiteRepository.Save(item);
            item.Id = result.Id;
            item.SaveVisualEditorStyles(activeStyles);
            item.SaveVisualEditorCommands(activeCommands);
            item.CreateSiteFolders();

            return result;
        }

        public static Site Update(Site item, int[] activeCommands, int[] activeStyles)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            if (!SiteRepository.Exists(item.Id))
            {
                throw new Exception(string.Format(SiteStrings.SiteNotFound, item.Id));
            }

            var result = SiteRepository.Update(item);
            item.SaveVisualEditorCommands(activeCommands);
            item.SaveVisualEditorStyles(activeStyles);
            item.CreateSiteFolders();
            return result;
        }

        public static MessageResult SimpleRemove(int id)
        {
            var site = SiteRepository.GetById(id);
            if (site == null)
            {
                throw new Exception(string.Format(SiteStrings.SiteNotFound, id));
            }

            if (site.LockedByAnyoneElse)
            {
                return MessageResult.Error(string.Format(SiteStrings.LockedByAnyoneElse, site.LockedByDisplayName));
            }

            SiteRepository.Delete(id);
            return null;
        }
        public static MessageResult AssembleContentsPreAction(int id)
        {
            var site = SiteRepository.GetById(id);
            var message = !site.IsLive ? null : string.Format(SiteStrings.SiteInLiveWarning, site.ModifiedToDisplay, site.LastModifiedByUserToDisplay);
            return string.IsNullOrEmpty(message) ? null : MessageResult.Confirm(message);
        }

        public static MessageResult AssembleContents(int id)
        {
            var site = SiteRepository.GetById(id);
            if (site == null)
            {
                throw new Exception(string.Format(SiteStrings.SiteNotFound, id));
            }

            if (!site.IsDotNet)
            {
                return MessageResult.Error(string.Format(SiteStrings.ShouldBeDotNet));
            }

            var sqlMetalPath = QPConfiguration.ConfigVariable(Config.SqlMetalKey);
            var extDbType = (QP.ConfigurationService.Models.DatabaseType)QPContext.DatabaseType;

            if (site.ExternalDevelopment)
            {
                var liveTempDirectory = $@"{site.TempDirectoryForClasses}\live";
                var stageTempDirectory = $@"{site.TempDirectoryForClasses}\stage";

                if (Directory.Exists(liveTempDirectory))
                {
                    Directory.Delete(liveTempDirectory, true);
                }

                Directory.CreateDirectory(liveTempDirectory);
                if (Directory.Exists(stageTempDirectory))
                {
                    Directory.Delete(stageTempDirectory, true);
                }

                Directory.CreateDirectory(stageTempDirectory);
                if (File.Exists(site.TempArchiveForClasses))
                {
                    File.Delete(site.TempArchiveForClasses);
                }
                new AssembleContentsController(id, sqlMetalPath, QPContext.CurrentDbConnectionString, extDbType)
                {
                    SiteRoot = liveTempDirectory,
                    IsLive = true,
                    DisableClassGeneration = site.DownloadEfSource
                }.Assemble();

                new AssembleContentsController(id, sqlMetalPath, QPContext.CurrentDbConnectionString, extDbType)
                {
                    SiteRoot = stageTempDirectory,
                    IsLive = false,
                    DisableClassGeneration = site.DownloadEfSource
                }.Assemble();
                var urlHelper =  HttpContext.RequestServices.GetRequiredService<IUrlHelper>();
                ZipFile.CreateFromDirectory(site.TempDirectoryForClasses, site.TempArchiveForClasses);
                return MessageResult.Download(urlHelper.Content($"~/Site/GetClassesZip/{id}"));
            }
            new AssembleContentsController(id, sqlMetalPath, QPContext.CurrentDbConnectionString, extDbType).Assemble();

            return null;
        }

        public static void Cancel(int id)
        {
            var site = SiteRepository.GetById(id);
            if (site == null)
            {
                throw new Exception(string.Format(SiteStrings.SiteNotFound, id));
            }

            site.AutoUnlock();
        }

        public static void CaptureLock(int id)
        {
            var site = SiteRepository.GetById(id);
            if (site == null)
            {
                throw new Exception(string.Format(SiteStrings.SiteNotFound, id));
            }

            if (site.CanBeUnlocked)
            {
                EntityObjectRepository.CaptureLock(site);
            }
        }

        public static Site New() => new Site();

        public static Site NewForSave() => new Site();

        public static LibraryResult Library(int id, string subFolder)
        {
            if (!SiteRepository.Exists(id))
            {
                throw new Exception(string.Format(SiteStrings.SiteNotFound, id));
            }

            var factory = new SiteFolderFactory();
            var repository = factory.CreateRepository();
            var folder = repository.GetBySubFolder(id, subFolder);
            return new LibraryResult { Folder = folder };
        }

        public static ListResult<FolderFile> GetFileList(ListCommand command, int parentFolderId, LibraryFileFilter filter)
        {
            var factory = new SiteFolderFactory();
            var repository = factory.CreateRepository();
            var folder = repository.GetById(parentFolderId);
            if (folder == null)
            {
                throw new Exception(string.Format(LibraryStrings.SiteFolderNotExists, parentFolderId));
            }

            return folder.GetFiles(command, filter);
        }

        public static PathInfo GetPathInfo(int folderId) => SiteFolder.GetPathInfo(folderId);

        internal static IEnumerable<ListItem> GetSites(IEnumerable<int> siteIDs) => SiteRepository.GetSimpleList(siteIDs);

        public static IEnumerable<VisualEditorCommand> GetAllVisualEditorCommands() => VisualEditorRepository.GetDefaultCommands();

        public static IEnumerable<VisualEditorStyle> GetAllVeStyles()
        {
            return VisualEditorRepository.GetAllStyles().OrderBy(s => s.Order).ToList();
        }

        public static Dictionary<int, bool> GetCommandBinding(int siteId) => VisualEditorRepository.GetCommandBindingBySiteId(siteId);

        public static Dictionary<int, bool> GetStyleBinding(int siteId) => VisualEditorRepository.GetStyleBindingBySiteId(siteId);

        internal static void CopySiteSettings(int sourceSiteId, int destinationSiteId)
        {
            SiteRepository.CopySiteSettings(sourceSiteId, destinationSiteId);
            var relBetweenFolders = SiteRepository.CopyFolders(sourceSiteId, destinationSiteId);
            SiteRepository.CopyFolderAccess(relBetweenFolders);
        }

        private static Site Read(int id, bool withAutoLock)
        {
            var site = SiteRepository.GetById(id);
            if (site == null)
            {
                throw new Exception(string.Format(SiteStrings.SiteNotFound, id));
            }

            if (site.LockedByAnyoneElse || !site.IsUpdatable)
            {
                site.IsReadOnly = true;
            }
            else if (withAutoLock)
            {
                site.AutoLock();
            }

            site.LoadLockedByUser();
            return site;
        }
    }
}
