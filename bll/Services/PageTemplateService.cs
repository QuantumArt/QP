using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using Assembling;
using Quantumart.QP8.Assembling;
using Quantumart.QP8.BLL.ListItems;
using Quantumart.QP8.BLL.Repository;
using Quantumart.QP8.BLL.Services.DTO;
using Quantumart.QP8.Constants;
using Quantumart.QP8.DAL;
using Quantumart.QP8.Resources;
using Quantumart.QP8.Utils.FullTextSearch;

namespace Quantumart.QP8.BLL.Services
{
    public enum FolderManagingType
    {
        CreateFolder = 0,
        DeleteFolder = 1,
        ChangeFolder = 2
    }

    public interface IPageTemplateService
    {
        Content ReadContentProperties(int id);

        ListResult<PageTemplateListItem> GetPageTemplatesBySiteId(ListCommand cmd, int siteId);

        PageTemplateInitListResult InitTemplateList(int parentId);

        PageTemplate NewPageTemplateProperties(int parentId);

        PageTemplate NewPageTemplatePropertiesForUpdate(int parentId);

        PageTemplate SavePageTemplateProperties(PageTemplate template);

        IEnumerable<ListItem> GetNetLanguagesAsListItems();

        IEnumerable<ListItem> GetLocalesAsListItems();

        IEnumerable<ListItem> GetCharsetsAsListItems();

        PageTemplate ReadPageTemplateProperties(int id, bool withAutoLock = true);

        PageTemplate ReadPageTemplatePropertiesForUpdate(int id);

        PageTemplate UpdatePageTemplateProperties(PageTemplate pageTemplate);

        MessageResult RemovePageTemplate(int id);

        Page ReadPageProperties(int id, bool withAutoLock = true);

        void CancelTemplate(int id);

        ObjectFormat ReadFormatProperties(int id, bool pageOrTemplate, bool withAutoLock = true);

        Content GetContentById(int contentId);

        IEnumerable<int> GetStatusIdsByContentId(int contentId, out bool hasWf);

        void CaptureLockTemplate(int id);

        MessageResult AssemblePageFromPageObject(int id);

        MessageResult AssemblePageFromPageObjectPreAction(int id);

        MessageResult AssemblePageFromPageObjectFormatPreAction(int id);

        MessageResult AssemblePageFromPageObjectList(int id);

        MessageResult AssemblePageFromPageObjectListPreAction(int id);

        MessageResult AssembleObjectFromPageObjectFormat(int id);

        MessageResult AssembleObjectFromPageObjectFormatPreAction(int id);

        MessageResult AssembleObjectFromTemplateObjectFormatPreAction(int parentId);

        MessageResult AssembleObjectFromTemplateObjectFormat(int parentId);

        MessageResult AssemblePageFromPageObjectFormat(int parentId);

        //IEnumerable<BackendActionType> GetActionTypeList();

        IEnumerable<PageTemplate> GetAllSiteTemplates(int siteId);

        IEnumerable<BllObject> GetAllTemplateObjects(int templateId);

        IEnumerable<BllObject> GetAllPageObjects(int pageId);

        ListResult<ObjectFormatSearchResultListItem> FormatSearch(ListCommand listCommand, int siteId, int? templateId, int? pageId, string filter);

        ListResult<PageTemplateSearchListItem> TemplateSearch(ListCommand listCommand, int id, string filter);

        ListResult<ObjectSearchListItem> ObjectSearch(ListCommand listCommand, int id, int? templateId, int? pageId, string filter);

        string ReadDefaultCode(int formatId);

        string ReadDefaultPresentation(int formatId);

        IEnumerable<TemplateObjectFormatDto> GetRestTemplateObjects(int templateId);

        BllObject ReadObjectProperties(int id, bool withAutoLock = true);
    }

    public class PageTemplateService : IPageTemplateService
    {
        public ListResult<PageTemplateListItem> GetPageTemplatesBySiteId(ListCommand cmd, int siteId)
        {
            int totalRecords;
            IEnumerable<PageTemplateListItem> list = PageTemplateRepository.ListTemplates(cmd, siteId, out totalRecords);
            return new ListResult<PageTemplateListItem>
            {
                Data = list.ToList(),
                TotalRecords = totalRecords
            };
        }

        public PageTemplateInitListResult InitTemplateList(int contentId)
        {
            return new PageTemplateInitListResult
            {
                IsAddNewAccessable = SecurityRepository.IsActionAccessible(ActionCode.AddNewPageTemplate) && SecurityRepository.IsEntityAccessible(EntityTypeCode.PageTemplate, contentId, ActionTypeCode.Update)
            };
        }

        public PageTemplate NewPageTemplateProperties(int parentId)
        {
            var site = SiteRepository.GetById(parentId);
            return PageTemplate.Create(parentId, site);
        }


        public PageTemplate NewPageTemplatePropertiesForUpdate(int parentId)
        {
            return NewPageTemplateProperties(parentId);
        }

        public PageTemplate SavePageTemplateProperties(PageTemplate template)
        {
            template.ReplaceUrlsToPlaceHolders();
            ManagePageTemplateFolders(template, FolderManagingType.CreateFolder);
            return PageTemplateRepository.SaveProperties(template);
        }

        private void ManagePageTemplateFolders(PageTemplate template, FolderManagingType type)
        {
            string stageDirectory = template.Site.StageDirectory + "\\" + template.TemplateFolder;
            string liveDirectory = template.Site.LiveDirectory + "\\" + template.TemplateFolder;

            if (type == FolderManagingType.CreateFolder)
            {
                if (!Directory.Exists(stageDirectory))
                    Directory.CreateDirectory(stageDirectory);
                if (!Directory.Exists(liveDirectory))
                    Directory.CreateDirectory(liveDirectory);
            }

            else if (type == FolderManagingType.DeleteFolder)
            {
                if (!string.IsNullOrWhiteSpace(template.TemplateFolder))
                {
                    if (Directory.Exists(stageDirectory))
                        Directory.Delete(stageDirectory, true);
                    if (Directory.Exists(liveDirectory))
                        Directory.Delete(liveDirectory, true);
                }
            }

            else if (type == FolderManagingType.ChangeFolder)
            {
                var oldTemplate = ReadPageTemplateProperties(template.Id, false);
                if (oldTemplate == null)
                    throw new ApplicationException(String.Format(TemplateStrings.TemplateNotFound, template.Id));
                string oldFolder = oldTemplate.TemplateFolder;
                if (!string.IsNullOrWhiteSpace(oldTemplate.TemplateFolder))
                {
                    if (string.IsNullOrWhiteSpace(template.TemplateFolder))
                    {
                        ManagePageTemplateFolders(oldTemplate, FolderManagingType.DeleteFolder);
                        return;
                    }
                    if (template.TemplateFolder == oldFolder)
                        return;
                    string oldStageDirectory = oldTemplate.Site.StageDirectory + "\\" + oldFolder;
                    string oldLiveDirectory = oldTemplate.Site.LiveDirectory + "\\" + oldFolder;
                    if (Directory.Exists(oldStageDirectory))
                        Directory.Move(oldStageDirectory, stageDirectory);
                    if (Directory.Exists(oldLiveDirectory))
                        Directory.Move(oldLiveDirectory, liveDirectory);
                }

                else if (!string.IsNullOrWhiteSpace(template.TemplateFolder))
                {
                    ManagePageTemplateFolders(template, FolderManagingType.CreateFolder);
                }
            }
        }

        public IEnumerable<ListItem> GetNetLanguagesAsListItems()
        {
            return PageTemplateRepository.GetNetLanguagesList().Select(lang => new ListItem { Text = lang.Name, Value = lang.Id.ToString() })
                    .ToArray();
        }

        public IEnumerable<ListItem> GetLocalesAsListItems()
        {
            return PageTemplateRepository.GetLocalesList().Select(locale => new ListItem { Text = locale.Name, Value = locale.Id.ToString() });
        }

        public IEnumerable<ListItem> GetCharsetsAsListItems()
        {
            return PageTemplateRepository.GetCharsetsList().Select(charset => new ListItem { Text = charset.Subj, Value = charset.Subj });
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


        public PageTemplate ReadPageTemplatePropertiesForUpdate(int id)
        {
            return ReadPageTemplateProperties(id, false);
        }


        public PageTemplate UpdatePageTemplateProperties(PageTemplate pageTemplate)
        {
            pageTemplate.ReplaceUrlsToPlaceHolders();
            ManageTemplateInheritance(pageTemplate);
            ManagePageTemplateFolders(pageTemplate, FolderManagingType.ChangeFolder);
            return PageTemplateRepository.UpdatePageTemplateProperties(pageTemplate);
        }


        public MessageResult RemovePageTemplate(int id)
        {
            PageTemplate template = PageTemplateRepository.GetPageTemplatePropertiesById(id);
            if (template == null)
                throw new ApplicationException(String.Format(TemplateStrings.TemplateNotFound, id));
            if (template.LockedByAnyoneElse)
                return MessageResult.Error(String.Format(TemplateStrings.LockedByAnyoneElse, template.LockedByDisplayName));
            ManagePageTemplateFolders(template, FolderManagingType.DeleteFolder);
            PageTemplateRepository.DeletePageTemplate(id);

            return null;
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

        public void CancelTemplate(int id)
        {
            PageTemplate template = PageTemplateRepository.GetPageTemplatePropertiesById(id);
            if (template == null)
                throw new Exception(String.Format(TemplateStrings.TemplateNotFound, id));
            template.AutoUnlock();
        }



        public string ReadDefaultCode(int formatId)
        {
            string netLanguagePrefix;
            var format = ObjectFormatRepository.ReadObjectFormat(formatId, true);
            var obj = ObjectRepository.GetObjectPropertiesById(format.ParentEntityId);
            netLanguagePrefix = GetLangPrefix(format.NetLanguageId);
            string pathToCopy = SitePathRepository.GetDirectoryPathToCopy() + "\\default\\";
            if (obj.IsObjectContainerType)
            {
                return ReadFileAsString(String.Format("{0}container_code_{1}.txt", pathToCopy, netLanguagePrefix));
            }
            else
                return ReadFileAsString(String.Format("{0}generic_code_{1}.txt", pathToCopy, netLanguagePrefix));
        }

        public string ReadDefaultPresentation(int formatId)
        {
            string netLanguagePrefix;
            var format = ObjectFormatRepository.ReadObjectFormat(formatId, true);
            var obj = ObjectRepository.GetObjectPropertiesById(format.ParentEntityId);
            netLanguagePrefix = GetLangPrefix(format.NetLanguageId);
            string pathToCopy = SitePathRepository.GetDirectoryPathToCopy() + "\\default\\";
            if (obj.IsObjectContainerType)
            {
                return ReadFileAsString(String.Format("{0}container_presentation.txt", pathToCopy));
            }
            else
                return string.Empty;
        }


        private string ReadFileAsString(string path)
        {
            var sb = new StringBuilder();

            StreamReader objReader = new StreamReader(path);
            string sLine = "";

            while (sLine != null)
            {
                sLine = objReader.ReadLine();
                if (sLine != null)
                    sb.AppendLine(sLine);
            }
            objReader.Close();

            return sb.ToString();
        }

        private static string GetLangPrefix(int? langId)
        {
            string netLanguagePrefix = "";

            if (langId == NetLanguage.GetcSharp().Id)
            {
                netLanguagePrefix = "cs";
            }

            else if (langId == NetLanguage.GetVbNet().Id)
            {
                netLanguagePrefix = "vb";
            }
            return netLanguagePrefix;
        }

        private void SetPagesAndObjectsEnableViewState(int pageTemplateId, bool enableViewState)
        {
            using (var scope = new QPConnectionScope())
            {
                Common.SetPagesAndObjectsEnableViewState(scope.DbConnection, pageTemplateId, enableViewState);
            }
        }

        private void SetObjectsDisableDataBinding(int pageTemplateId, bool disableDataBinding)
        {
            using (var scope = new QPConnectionScope())
            {
                Common.SetObjectsDisableDataBinding(scope.DbConnection, pageTemplateId, disableDataBinding);
            }
        }

        private void SetCustomClassForPages(int pageTemplateId, string customClass)
        {
            using (var scope = new QPConnectionScope())
            {
                Common.SetCustomClassForPages(scope.DbConnection, pageTemplateId, customClass);
            }
        }

        private void SetCustomClassForObjects(int pageTemplateId, string customClassForGenerics, string customClassForContainers, string customClassForForms)
        {
            using (var scope = new QPConnectionScope())
            {
                Common.SetCustomClassForObjects(scope.DbConnection, pageTemplateId, customClassForGenerics, customClassForContainers, customClassForForms);
            }
        }

        private void ManageTemplateInheritance(PageTemplate pageTemplate)
        {
            if (pageTemplate.ApplyToExistingPagesAndObjects)
                SetPagesAndObjectsEnableViewState(pageTemplate.Id, pageTemplate.EnableViewstate);
            if (pageTemplate.ApplyToExistingObjects)
                SetObjectsDisableDataBinding(pageTemplate.Id, pageTemplate.DisableDatabind);
            if (pageTemplate.OverridePageSettings)
                SetCustomClassForPages(pageTemplate.Id, pageTemplate.CustomClassForPages);
            if (pageTemplate.OverrideObjectSettings)
                SetCustomClassForObjects(pageTemplate.Id, pageTemplate.CustomClassForGenerics, pageTemplate.CustomClassForContainers, pageTemplate.CustomClassForForms);
        }

        public Content GetContentById(int contentId)
        {
            return ContentRepository.GetById(contentId);
        }

        public IEnumerable<int> GetStatusIdsByContentId(int contentId, out bool hasWf)
        {
            return PageTemplateRepository.GetStatusIdsByContentId(contentId, out hasWf);
        }

        public void CaptureLockTemplate(int id)
        {
            var template = PageTemplateRepository.GetPageTemplatePropertiesById(id);
            if (template == null)
                throw new Exception(String.Format(TemplateStrings.TemplateNotFound, id));
            if (template.CanBeUnlocked)
            {
                EntityObjectRepository.CaptureLock(template);
            }
        }


        public MessageResult AssemblePageFromPageObject(int pageId)
        {
            return AssemblePage(pageId);
        }

        public MessageResult AssemblePageFromPageObjectPreAction(int pageId)
        {
            return AssemblePagePreAction(pageId);
        }

        public MessageResult AssemblePageFromPageObjectFormatPreAction(int objectId)
        {
            var obj = ReadObjectProperties(objectId, false);
            return AssemblePagePreAction(obj.PageId.Value);
        }

        public BllObject ReadObjectProperties(int id, bool withAutoLock = true)
        {
            BllObject obj = ObjectRepository.GetObjectPropertiesById(id);
            if (obj == null)
                throw new ApplicationException(String.Format(TemplateStrings.ObjectNotFound, id));

            if (withAutoLock)
                obj.AutoLock();

            obj.LoadLockedByUser();
            return obj;
        }

        public MessageResult AssemblePageFromPageObjectList(int parentId)
        {
            return AssemblePage(parentId);
        }

        public MessageResult AssemblePageFromPageObjectListPreAction(int parentId)
        {
            return AssemblePagePreAction(parentId);
        }

        public MessageResult AssembleObjectFromPageObjectFormat(int parentId)
        {
            return AssembleObject(parentId);
        }

        public MessageResult AssembleObjectFromPageObjectFormatPreAction(int parentId)
        {
            return AssembleObjectPreAction(parentId);
        }


        public MessageResult AssembleObjectFromTemplateObjectFormatPreAction(int parentId)
        {
            return AssembleObjectPreAction(parentId);
        }

        public MessageResult AssembleObjectFromTemplateObjectFormat(int parentId)
        {
            return AssembleObject(parentId);
        }

        public MessageResult AssembleObjectPreAction(int id)
        {
            var site = ObjectRepository.GetObjectPropertiesById(id).PageTemplate.Site;
            string message = (!site.IsLive) ? null : String.Format(SiteStrings.SiteInLiveWarning, site.ModifiedToDisplay, site.LastModifiedByUserToDisplay);
            return (String.IsNullOrEmpty(message)) ? null : MessageResult.Confirm(message);
        }

        public MessageResult AssembleObject(int id)
        {
            var obj = ObjectRepository.GetObjectPropertiesById(id);
            if (obj.PageTemplate.SiteIsDotNet)
            {
                new AssembleSelectedObjectsController(id.ToString(), QPContext.CurrentCustomerCode).Assemble();
                return null;
            }
            return MessageResult.Error(SiteStrings.ShouldBeDotNet);
        }

        public MessageResult AssemblePageFromPageObjectFormat(int parentId)
        {
            var obj = ReadObjectProperties(parentId);
            return AssemblePage(obj.PageId.Value);
        }

        public IEnumerable<PageTemplate> GetAllSiteTemplates(int siteId)
        {
            return PageTemplateRepository.GetSiteTemplates(siteId);
        }

        public ListResult<ObjectFormatSearchResultListItem> FormatSearch(ListCommand listCommand, int siteId, int? templateId, int? pageId, string filter)
        {
            int totalRecords;
            List<ObjectFormatSearchResultListItem> data = PageTemplateRepository.GetSearchFormatPage(listCommand, siteId, templateId, pageId, filter, out totalRecords).ToList();
            ManageFormatSearchItemsDescription(data, filter);
            return new ListResult<ObjectFormatSearchResultListItem>
            {
                Data = data,
                TotalRecords = totalRecords
            };
        }

        private void ManageFormatSearchItemsDescription(List<ObjectFormatSearchResultListItem> data, string filter)
        {
            if (string.IsNullOrWhiteSpace(filter))
                return;
            foreach (var item in data)
            {
                var format = ObjectFormatRepository.ReadObjectFormat(item.Id, true);
                if (!string.IsNullOrWhiteSpace(format.CodeBehind) && format.CodeBehind.Contains(filter))
                {
                    item.Description = FoundTextMarker.GetSimpleRelevantMarkedText(System.Web.HttpUtility.HtmlEncode(format.CodeBehind), filter, 20, "<span class='seachResultHighlight'>", "</span>");
                    continue;
                }

                if (!string.IsNullOrWhiteSpace(format.FormatBody) && format.FormatBody.Contains(filter))
                {
                    item.Description = FoundTextMarker.GetSimpleRelevantMarkedText(System.Web.HttpUtility.HtmlEncode(format.FormatBody), filter, 20, "<span class='seachResultHighlight'>", "</span>");
                    continue;
                }
            }
        }


        public ListResult<PageTemplateSearchListItem> TemplateSearch(ListCommand listCommand, int siteId, string filter)
        {
            int totalRecords;
            List<PageTemplateSearchListItem> data = PageTemplateRepository.GetSearchTemplatePage(listCommand, siteId, filter, out totalRecords).ToList();
            ManageTemplateSearchItemsDescription(data, filter);
            return new ListResult<PageTemplateSearchListItem>
            {
                Data = data,
                TotalRecords = totalRecords
            };
        }

        private void ManageTemplateSearchItemsDescription(List<PageTemplateSearchListItem> data, string filter)
        {
            if (string.IsNullOrWhiteSpace(filter))
                return;
            foreach (var item in data)
            {
                var template = PageTemplateRepository.GetPageTemplatePropertiesById(item.Id);
                if (!string.IsNullOrWhiteSpace(template.CodeBehind) && template.CodeBehind.Contains(filter))
                {
                    item.Description = FoundTextMarker.GetSimpleRelevantMarkedText(System.Web.HttpUtility.HtmlEncode(template.CodeBehind), filter, 20, "<span class='seachResultHighlight'>", "</span>");
                    continue;
                }

                if (!string.IsNullOrWhiteSpace(template.TemplateBody) && template.TemplateBody.Contains(filter))
                {
                    item.Description = FoundTextMarker.GetSimpleRelevantMarkedText(System.Web.HttpUtility.HtmlEncode(template.TemplateBody), filter, 20, "<span class='seachResultHighlight'>", "</span>");
                    continue;
                }
            }
        }

        public ListResult<ObjectSearchListItem> ObjectSearch(ListCommand listCommand, int siteId, int? templateId, int? pageId, string filter)
        {
            int totalRecords;
            List<ObjectSearchListItem> data = PageTemplateRepository.GetSearchObjectPage(listCommand, siteId, templateId, pageId, filter, out totalRecords).ToList();
            ManageObjectSearchItemsDescription(data, filter);
            return new ListResult<ObjectSearchListItem>
            {
                Data = data,
                TotalRecords = totalRecords
            };
        }

        private void ManageObjectSearchItemsDescription(List<ObjectSearchListItem> data, string filter)
        {
            if (string.IsNullOrWhiteSpace(filter))
                return;
            foreach (var item in data)
            {
                var obj = ObjectRepository.GetObjectPropertiesById(item.Id);
                var defVals = obj.DefaultValues;
                if (defVals != null && defVals.Count() > 0)
                {
                    var names = defVals.Where(x => x.VariableName.Contains(filter));
                    var dName = names.Count() > 0 ? names.First() : null;
                    if (dName != null)
                    {
                        item.Description = FoundTextMarker.GetSimpleRelevantMarkedText(System.Web.HttpUtility.HtmlEncode(dName.VariableName), filter, 20, "<span class='seachResultHighlight'>", "</span>");
                        continue;
                    }

                    var vals = defVals.Where(x => x.VariableValue.Contains(filter));
                    var dVal = vals.Count() > 0 ? vals.First() : null;
                    if (dVal != null)
                    {
                        item.Description = FoundTextMarker.GetSimpleRelevantMarkedText(System.Web.HttpUtility.HtmlEncode(dVal.VariableValue), filter, 20, "<span class='seachResultHighlight'>", "</span>");
                        continue;
                    }
                }
                if (obj.Container != null)
                {
                    if (!string.IsNullOrWhiteSpace(obj.Container.FilterValue) && obj.Container.FilterValue.Contains(filter))
                    {
                        item.Description = FoundTextMarker.GetSimpleRelevantMarkedText(System.Web.HttpUtility.HtmlEncode(obj.Container.FilterValue), filter, 20, "<span class='seachResultHighlight'>", "</span>");
                        continue;
                    }

                    if (!string.IsNullOrWhiteSpace(obj.Container.OrderDynamic) && obj.Container.OrderDynamic.Contains(filter))
                    {
                        item.Description = FoundTextMarker.GetSimpleRelevantMarkedText(System.Web.HttpUtility.HtmlEncode(obj.Container.OrderDynamic), filter, 20, "<span class='seachResultHighlight'>", "</span>");
                        continue;
                    }

                    if (obj.Container.SelectStart == filter)
                    {
                        item.Description = "<span class='seachResultHighlight'>" + obj.Container.SelectStart + "</span>";
                        continue;
                    }

                    if (obj.Container.SelectTotal == filter)
                    {
                        item.Description = "<span class='seachResultHighlight'>" + obj.Container.SelectTotal + "</span>";
                        continue;
                    }
                }
            }
        }

        private ObjectFormatVersion ReadFormatVersion(int id)
        {
            return PageTemplateRepository.ReadFormatVersion(id);
        }

        public IEnumerable<BllObject> GetAllTemplateObjects(int templateId)
        {
            return ObjectRepository.GetTemplateObjects(templateId);
        }

        public IEnumerable<BllObject> GetAllPageObjects(int pageId)
        {
            return ObjectRepository.GetPageObjects(pageId);
        }

        public Content ReadContentProperties(int id)
        {
            return ContentRepository.GetByIdWithFields(id);
        }

        IEnumerable<TemplateObjectFormatDto> IPageTemplateService.GetRestTemplateObjects(int templateId)
        {
            return ObjectRepository.GetRestTemplateObjects(templateId);
        }


        public ObjectFormat ReadFormatProperties(int id, bool pageOrTemplate, bool withAutoLock = true)
        {
            ObjectFormat format = ObjectFormatRepository.ReadObjectFormat(id, pageOrTemplate);
            if (format == null)
                throw new ApplicationException(String.Format(TemplateStrings.FormatNotFound, id));
            if (withAutoLock)
                format.AutoLock();
            format.LoadLockedByUser();
            format.PageOrTemplate = pageOrTemplate;
            format.ReplacePlaceHoldersToUrls();
            return format;
        }

        public MessageResult AssemblePage(int id)
        {
            var page = PageRepository.GetPagePropertiesById(id);
            var site = page.PageTemplate.Site;
            if (page.PageTemplate.SiteIsDotNet)
            {
                new AssemblePageController(id, QPContext.CurrentCustomerCode).Assemble();
                AssembleRepository.UpdatePageStatus(id, QPContext.CurrentUserId);
                return null;
            }
            return MessageResult.Error(SiteStrings.ShouldBeDotNet);
        }

        public MessageResult AssemblePagePreAction(int id)
        {
            var site = PageRepository.GetPagePropertiesById(id).PageTemplate.Site;
            string message = (!site.IsLive) ? null : String.Format(SiteStrings.SiteInLiveWarning, site.ModifiedToDisplay, site.LastModifiedByUserToDisplay);
            return (String.IsNullOrEmpty(message)) ? null : MessageResult.Confirm(message);
        }

        public static int CopySiteTemplates(int sourceSiteId, int destinationSiteId, int templateNumber)
        {
            int templateIdNew = PageTemplateRepository.CopySiteTemplates(sourceSiteId, destinationSiteId, templateNumber);

            string relBetweenTemplates = PageTemplateRepository.GetRelationsBetweenTemplates(sourceSiteId, destinationSiteId, templateIdNew);
            string relBetweenContents = ContentRepository.GetRelationsBetweenContentsXml(sourceSiteId, destinationSiteId, String.Empty);

            PageRepository.CopySiteTemplatePages(sourceSiteId, destinationSiteId, relBetweenTemplates);

            string relBetweenPages = PageRepository.GetRelationsBetweenPages(relBetweenTemplates);

            string relBetweenObjects = String.Empty;
            ObjectRepository.CopySiteTemplateObjects(relBetweenTemplates, relBetweenPages, ref relBetweenObjects);

            string relBetweenObjectFormats = String.Empty;
            ObjectFormatRepository.CopySiteTemplateObjectFormats(relBetweenObjects, ref relBetweenObjectFormats);

            ObjectRepository.CopySiteUpdateObjects(relBetweenObjectFormats, relBetweenObjects);
            ObjectRepository.CopySiteObjectValues(relBetweenObjects);
            ObjectRepository.CopySiteContainers(relBetweenObjects, relBetweenContents);

            string relBetweenStatuses = ContentRepository.GetRelationsBetweenStatuses(sourceSiteId, destinationSiteId);
            ObjectRepository.CopyContainerStatuses(relBetweenStatuses, relBetweenObjects);

            NotificationRepository.CopySiteUpdateNotifications(relBetweenObjectFormats, relBetweenContents);

            return templateIdNew != 0 ? 1 : 0;
        }
    }
}
