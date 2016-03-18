using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Quantumart.QP8.BLL.Repository;
using Quantumart.QP8.Resources;
using Quantumart.QP8.BLL.Services.DTO;
using Quantumart.QP8.BLL.Factories;
using Quantumart.QP8.Constants;
using Quantumart.QP8.Assembling;
using Quantumart.QP8.Configuration;
using System.IO;
using Quantumart.QP8.BLL.ListItems;
using Quantumart.QP8.BLL.Helpers;
using Quantumart.QP8.DAL;

namespace Quantumart.QP8.BLL.Services
{
    public class SiteService
    {

        #region Private Members


        private static Site Read(int id, bool withAutoLock = true)
        {
            Site site = SiteRepository.GetById(id);
			if (site == null)
				throw new Exception(String.Format(SiteStrings.SiteNotFound, id));
			if (site.LockedByAnyoneElse || !site.IsUpdatable)
				site.IsReadOnly = true;
			else if (withAutoLock)
                site.AutoLock();
			site.LoadLockedByUser();			
			return site;
        }
        #endregion


		public static SiteInitListResult InitList(int parentId)
		{
			return new SiteInitListResult
			{
				IsAddNewAccessable = SecurityRepository.IsActionAccessible(ActionCode.AddNewSite)
			};
		}

		public static SiteInitListResult MultipleInitList(int parentId)
		{
			return new SiteInitListResult
			{
				IsAddNewAccessable = false
			};
		}

		public static ListResult<SiteListItem> List(ListCommand cmd, IEnumerable<int> selectedIDs = null)
        {
			return SiteRepository.GetList(cmd, selectedIDs);
        }

		public static IEnumerable<ListItem> GetAllSites()
		{
			return SiteRepository.GetAll().Select(s => new ListItem(s.Id.ToString(), s.Name)).ToArray();
		}

		public static IEnumerable<ListItem> GetSites()
		{
			ListCommand cmd = new ListCommand() { PageSize = Int32.MaxValue, StartPage = 1 };
			return SiteRepository.GetList(cmd, null).Data.Select(s => new ListItem(s.Id.ToString(), s.Name)).ToArray();
		}

        public static Site Read(int id)
        {
			return Read(id, true);
        }

        public static Site ReadForUpdate(int id)
        {
            return Read(id, false);
        }

		public static Site Save(Site item, int[] activeCommands, int[] activeStyles)
        {
			if (item == null)
				throw new ArgumentNullException("item");
			Site result = SiteRepository.Save(item);
			item.Id = result.Id;
			item.SaveVisualEditorStyles(activeStyles);
			item.SaveVisualEditorCommands(activeCommands);
            item.CreateSiteFolders();
            return result;
        }

        public static Site Update(Site item, int[] activeCommands, int[] activeStyles)
        {
			if (item == null)
				throw new ArgumentNullException("item");
			if (!SiteRepository.Exists(item.Id))
				throw new Exception(String.Format(SiteStrings.SiteNotFound, item.Id));
			Site result = SiteRepository.Update(item);
			item.SaveVisualEditorCommands(activeCommands);
			item.SaveVisualEditorStyles(activeStyles);
            item.CreateSiteFolders();            
            return result;
        }

        public static MessageResult SimpleRemove(int id)
        {
            Site site = SiteRepository.GetById(id);
			if (site == null)
				throw new Exception(String.Format(SiteStrings.SiteNotFound, id));
            if (site.LockedByAnyoneElse)
                return MessageResult.Error(String.Format(SiteStrings.LockedByAnyoneElse, site.LockedByDisplayName));
            SiteRepository.Delete(id);
            return null;
        }

		public static MessageResult AssembleContentsPreAction(int id)
		{
			Site site = SiteRepository.GetById(id);
			string message = (!site.IsLive) ? null : String.Format(SiteStrings.SiteInLiveWarning, site.ModifiedToDisplay, site.LastModifiedByUserToDisplay);
			return (String.IsNullOrEmpty(message)) ? null : MessageResult.Confirm(message);
		}

		public static MessageResult AssembleContents(int id)
		{
			Site site = SiteRepository.GetById(id);
			if (site == null)
				throw new Exception(String.Format(SiteStrings.SiteNotFound, id));
			if (!site.IsDotNet)
				return MessageResult.Error(String.Format(SiteStrings.ShouldBeDotNet));
			
			string sqlMetalPath = QPConfiguration.ConfigVariable(Config.SqlMetalKey);
			if (String.IsNullOrEmpty(sqlMetalPath))
				return MessageResult.Error(String.Format(GlobalStrings.SqlMetalPathEmpty));


			site.CreateLinqDirectories();

			AssembleContentsController cnt = new AssembleContentsController(id, sqlMetalPath, QPContext.CurrentCustomerCode);
			cnt.Assemble();

			return null;
		}

        public static void Cancel(int id)
        {
            Site site = SiteRepository.GetById(id);
			if (site == null)
				throw new Exception(String.Format(SiteStrings.SiteNotFound, id));
            site.AutoUnlock();
        }

        public static void CaptureLock(int id)
        {
            Site site = SiteRepository.GetById(id);
			if (site == null)
				throw new Exception(String.Format(SiteStrings.SiteNotFound, id));
            if (site.CanBeUnlocked)
            {
                EntityObjectRepository.CaptureLock(site);
            }
        }

        public static Site New()
        {
            return new Site();
        }

        public static Site NewForSave()
        {
            return new Site();
        }

		public static LibraryResult Library(int id, string subFolder)
		{
			if (!SiteRepository.Exists(id))
				throw new Exception(String.Format(SiteStrings.SiteNotFound, id));
			SiteFolderFactory factory = new SiteFolderFactory();
			FolderRepository repository = factory.CreateRepository();
			Folder folder = repository.GetBySubFolder(id, subFolder);
			return new LibraryResult() { Folder = folder };
		}

		public static ListResult<FolderFile> GetFileList(ListCommand command, int parentFolderId, LibraryFileFilter filter)
		{
			SiteFolderFactory factory = new SiteFolderFactory();
			FolderRepository repository = factory.CreateRepository();
			Folder folder = repository.GetById(parentFolderId);
			if (folder == null)
				throw new Exception(String.Format(LibraryStrings.SiteFolderNotExists, parentFolderId));
			return folder.GetFiles(command, filter);			
		}

		public static PathInfo GetPathInfo(int folderId)
		{
			return SiteFolder.GetPathInfo(folderId);
		}

		internal static IEnumerable<ListItem> GetSites(IEnumerable<int> siteIDs)
		{
			return SiteRepository.GetSimpleList(siteIDs);
		}

		public static IEnumerable<VisualEditorCommand> GetAllVisualEditorCommands()
		{
			return VisualEditorRepository.GetDefaultCommands();
		}				

		public static IEnumerable<VisualEditorStyle> GetAllVeStyles()
		{
			return VisualEditorRepository.GetAllStyles().OrderBy(s => s.Order).ToList();
		}				

		public static Dictionary<int, bool> GetCommandBinding(int siteId)
		{
			return VisualEditorRepository.GetCommandBindingBySiteId(siteId);
		}

		public static Dictionary<int, bool> GetStyleBinding(int siteId)
		{
			return VisualEditorRepository.GetStyleBindingBySiteId(siteId);
		}

        internal static void CopySiteSettings(int sourceSiteId, int destinationSiteId)
        {
            SiteRepository.CopySiteSettings(sourceSiteId, destinationSiteId);
            string relBetweenFolders = SiteRepository.CopyFolders(sourceSiteId, destinationSiteId);
            SiteRepository.CopyFolderAccess(relBetweenFolders);
        }
    }
}
