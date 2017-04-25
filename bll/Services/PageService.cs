using Quantumart.QP8.Assembling;
using Quantumart.QP8.BLL.ListItems;
using Quantumart.QP8.BLL.Repository;
using Quantumart.QP8.BLL.Services.DTO;
using Quantumart.QP8.Constants;
using Quantumart.QP8.Resources;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Quantumart.QP8.BLL.Services
{
    public interface IPageService
    {
        PageInitListResult InitPageList(int parentId);

        PageInitListResult InitPageListForSite(int siteId);

        ListResult<PageListItem> GetPagesByTemplateId(ListCommand listCommand, int parentId);

        IEnumerable<ListItem> GetLocalesAsListItems();

        IEnumerable<ListItem> GetCharsetsAsListItems();

        Page ReadPagePropertiesForUpdate(int id);

        Page UpdatePageProperties(Page page);

        Page NewPageProperties(int parentId);

        Page NewPagePropertiesForUpdate(int parentId);

        Page SavePageProperties(Page page);

        MessageResult RemovePage(int id);

        void CancelPage(int id);

        MessageResult MultipleRemovePage(int[] IDs);

        PageTemplate ReadPageTemplateProperties(int id, bool withAutoLock = true);

        Page ReadPageProperties(int id, bool withAutoLock = true);

        ListResult<PageListItem> ListPagesForSite(ListCommand listCommand, int parentId, int id);

        MessageResult AssemblePagePreAction(int id);

        MessageResult MultipleAssemblePagePreAction(int[] ids);

        MessageResult MultipleAssemblePage(int[] ids);

        MessageResult AssemblePage(int id);

        void CaptureLockPage(int id);

        CopyResult Copy(int id);
    }

    public class PageService: IPageService
    {
        private readonly IQP7Service _qp7Service;

        public PageService()
        {
            _qp7Service = new QP7Service();
        }

        public PageTemplate ReadPageTemplateProperties(int id, bool withAutoLock = true)
        {
            PageTemplate template = PageTemplateRepository.GetPageTemplatePropertiesById(id);
            if (template == null)
                throw new ApplicationException(String.Format(TemplateStrings.TemplateNotFound, id));

            if (withAutoLock)
                template.AutoLock();

            template.LoadLockedByUser();
            template.ReplacePlaceHoldersToUrls();
            return template;
        }

        public PageInitListResult InitPageList(int parentId)
        {
            return new PageInitListResult
            {
                IsAddNewAccessable = SecurityRepository.IsActionAccessible(ActionCode.AddNewPage) && SecurityRepository.IsEntityAccessible(EntityTypeCode.Page, parentId, ActionTypeCode.Update)
            };
        }

        public PageInitListResult InitPageListForSite(int siteId)
        {
            var site = SiteRepository.GetById(siteId);
            if (site == null)
                throw new Exception(String.Format(SiteStrings.SiteNotFound, siteId));

            return new PageInitListResult
            {
                ParentName = site.Name,
                IsAddNewAccessable = false
            };
        }

        public ListResult<PageListItem> GetPagesByTemplateId(ListCommand cmd, int parentId)
        {
            int totalRecords;
            IEnumerable<PageListItem> list = PageRepository.ListPages(cmd, parentId, out totalRecords);
            return new ListResult<PageListItem>
            {
                Data = list.ToList(),
                TotalRecords = totalRecords
            };
        }

        public IEnumerable<ListItem> GetLocalesAsListItems()
        {
            return PageTemplateRepository.GetLocalesList().Select(locale => new ListItem { Text = locale.Name, Value = locale.Id.ToString() });
        }

        public IEnumerable<ListItem> GetCharsetsAsListItems()
        {
            return PageTemplateRepository.GetCharsetsList().Select(charset => new ListItem { Text = charset.Subj, Value = charset.Subj });
        }

        public Page ReadPagePropertiesForUpdate(int id)
        {
            return ReadPageProperties(id, false);
        }

        public Page ReadPageProperties(int id, bool withAutoLock = true)
        {
            Page page = PageRepository.GetPagePropertiesById(id);
            if (page == null)
                throw new ApplicationException(String.Format(TemplateStrings.PageNotFound, id));

            if (withAutoLock)
                page.AutoLock();

            page.LoadLockedByUser();
            return page;
        }

        public Page UpdatePageProperties(Page page)
        {
            ManagePageInheritance(page);
            ManagePageFolders(page, FolderManagingType.ChangeFolder);
            return PageRepository.UpdatePageProperties(page);
        }

        private void ManagePageInheritance(Page page)
        {
            if (page.ApplyToExistingObjects)
                SetPageObjectEnableViewState(page.Id, page.EnableViewState);
        }

        private void ManagePageFolders(Page page, FolderManagingType type)
        {
            string stageDirectory = page.PageTemplate.Site.StageDirectory + "\\" + page.PageTemplate.TemplateFolder + "\\" + page.Folder;
            string liveDirectory = page.PageTemplate.Site.LiveDirectory + "\\" + page.PageTemplate.TemplateFolder + "\\" + page.Folder;

            if (type == FolderManagingType.CreateFolder)
            {
                if (!Directory.Exists(stageDirectory))
                    Directory.CreateDirectory(stageDirectory);
                if (!Directory.Exists(liveDirectory))
                    Directory.CreateDirectory(liveDirectory);
            }

            else if (type == FolderManagingType.DeleteFolder)
            {
                if (!string.IsNullOrWhiteSpace(page.Folder))
                {
                    if (Directory.Exists(stageDirectory))
                        Directory.Delete(stageDirectory, true);
                    if (Directory.Exists(liveDirectory))
                        Directory.Delete(liveDirectory, true);
                }
            }

            else if (type == FolderManagingType.ChangeFolder)
            {
                var oldPage = ReadPageProperties(page.Id, false);
                if (oldPage == null)
                    throw new ApplicationException(String.Format(TemplateStrings.PageNotFound, oldPage.Id));
                string oldFolder = oldPage.Folder;
                if (!string.IsNullOrWhiteSpace(oldFolder))
                {
                    if (string.IsNullOrWhiteSpace(page.Folder))
                    {
                        ManagePageFolders(oldPage, FolderManagingType.DeleteFolder);
                        return;
                    }
                    if (page.Folder == oldFolder)
                        return;
                    string oldStageDirectory = oldPage.PageTemplate.Site.StageDirectory + "\\" + oldPage.PageTemplate.TemplateFolder + "\\" + oldFolder;
                    string oldLiveDirectory = oldPage.PageTemplate.Site.LiveDirectory + "\\" + oldPage.PageTemplate.TemplateFolder + "\\" + oldFolder;
                    if (Directory.Exists(oldStageDirectory))
                        Directory.Move(oldStageDirectory, stageDirectory);
                    if (Directory.Exists(oldLiveDirectory))
                        Directory.Move(oldLiveDirectory, liveDirectory);
                }

                else if (!string.IsNullOrWhiteSpace(page.Folder))
                {
                    ManagePageFolders(page, FolderManagingType.CreateFolder);
                }
            }
        }

        private void SetPageObjectEnableViewState(int pageId, bool enableViewState)
        {
            ObjectRepository.SetPageObjectEnableViewState(pageId, enableViewState);
        }

        public Page NewPageProperties(int parentId)
        {
            return Page.Create(parentId);
        }

        public Page NewPagePropertiesForUpdate(int parentId)
        {
            return NewPageProperties(parentId);
        }

        public Page SavePageProperties(Page page)
        {
            ManagePageFolders(page, FolderManagingType.CreateFolder);
            return PageRepository.SavePageProperties(page);
        }

        public MessageResult RemovePage(int id)
        {
            Page page = PageRepository.GetPagePropertiesById(id);
            if (page == null)
                throw new ApplicationException(String.Format(TemplateStrings.TemplateNotFound, id));
            if (page.LockedByAnyoneElse)
                return MessageResult.Error(String.Format(TemplateStrings.LockedByAnyoneElse, page.LockedByDisplayName));
            ManagePageFolders(page, FolderManagingType.DeleteFolder);
            PageRepository.DeletePage(id);
            return null;
        }

        public void CancelPage(int id)
        {
            Page page = PageRepository.GetPagePropertiesById(id);
            if (page == null)
                throw new Exception(String.Format(TemplateStrings.PageNotFound, id));
            page.AutoUnlock();
        }

        public MessageResult MultipleRemovePage(int[] IDs)
        {
            if (IDs == null)
                throw new ArgumentNullException("IDs");

            PageTemplateRepository.MultipleDeletePage(IDs);

            return null;
        }

        public ListResult<PageListItem> ListPagesForSite(ListCommand listCommand, int parentId, int id)
        {
            int totalRecords;
            IEnumerable<PageListItem> list = PageRepository.ListSitePages(listCommand, parentId, out totalRecords);
            return new ListResult<PageListItem>
            {
                Data = list.ToList(),
                TotalRecords = totalRecords
            };
        }

        public MessageResult AssemblePagePreAction(int id)
        {
            var page = PageRepository.GetPagePropertiesById(id);
            var site = page.PageTemplate.Site;
            string message = (!site.IsLive) ? null : String.Format(SiteStrings.SiteInLiveWarning, site.ModifiedToDisplay, site.LastModifiedByUserToDisplay);
            return (String.IsNullOrEmpty(message)) ? null : MessageResult.Confirm(message);
        }

        public MessageResult MultipleAssemblePagePreAction(int[] ids)
        {
            if (ids == null)
                throw new ArgumentNullException("IDs");
            var site = PageRepository.GetPagePropertiesById(ids[0]).PageTemplate.Site;
            string message = (!site.IsLive) ? null : String.Format(SiteStrings.SiteInLiveWarning, site.ModifiedToDisplay, site.LastModifiedByUserToDisplay);
            return (String.IsNullOrEmpty(message)) ? null : MessageResult.Confirm(message);
        }

        public MessageResult MultipleAssemblePage(int[] IDs)
        {
            var failedIds = new List<int>();

            foreach (int id in IDs)
            {
                QP7Token token = null;
                var page = PageRepository.GetPagePropertiesById(id);
                var site = page.PageTemplate.Site;
                if (page.PageTemplate.SiteIsDotNet)
                {
                    new AssemblePageController(id, QPContext.CurrentDbConnectionString).Assemble();
                    AssembleRepository.UpdatePageStatus(id, QPContext.CurrentUserId);
                }
                else
                {
                    if (token == null)
                    {
                        token = _qp7Service.Authenticate();
                    }

                    var message = _qp7Service.AssemblePage(id, token);  
                    
                    if (!string.IsNullOrEmpty(message))
                    {
                        failedIds.Add(id);
                    }
                }
            }

            if (failedIds.Any())
            {
                return MessageResult.Error(SiteStrings.AssemblePagesError + string.Join(", ", failedIds), failedIds.ToArray());
            }
            else
            {
                return null;
            }
        }

        public MessageResult AssemblePage(int id)
        {
            var page = PageRepository.GetPagePropertiesById(id);
            if (page.PageTemplate.SiteIsDotNet)
            {
                new AssemblePageController(id, QPContext.CurrentDbConnectionString).Assemble();
                AssembleRepository.UpdatePageStatus(id, QPContext.CurrentUserId);
            }
            else
            {
                var token = _qp7Service.Authenticate();
                var message = _qp7Service.AssemblePage(id, token);

                if (!string.IsNullOrEmpty(message))
                {
                    return MessageResult.Error(message);
                }
            }

            return null;
        }

        public void CaptureLockPage(int id)
        {
            var page = PageRepository.GetPagePropertiesById(id);
            if (page == null)
                throw new Exception(String.Format(TemplateStrings.PageNotFound, id));
            if (page.CanBeUnlocked)
            {
                EntityObjectRepository.CaptureLock(page);
            }
        }


        public CopyResult Copy(int id)
        {
            CopyResult result = new CopyResult();
            Page page = PageRepository.GetPagePropertiesById(id);
            if (page == null)
                throw new ApplicationException(String.Format(TemplateStrings.PageNotFound, id));

            page.MutatePage();
            ManagePageFolders(page, FolderManagingType.CreateFolder);
            int newId = PageRepository.CopyPage(page, QPContext.CurrentUserId);
            if (newId == 0)
                result.Message = MessageResult.Error(TemplateStrings.PageNotCreated);
            else
                result.Id = newId;
            return result;
        }
    }
}
