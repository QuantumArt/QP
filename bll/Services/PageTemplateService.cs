using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using Quantumart.QP8.Assembling;
using Quantumart.QP8.BLL.ListItems;
using Quantumart.QP8.BLL.Repository;
using Quantumart.QP8.BLL.Repository.ContentRepositories;
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
            var list = PageTemplateRepository.ListTemplates(cmd, siteId, out var totalRecords);
            return new ListResult<PageTemplateListItem>
            {
                Data = list.ToList(),
                TotalRecords = totalRecords
            };
        }

        public PageTemplateInitListResult InitTemplateList(int contentId) => new PageTemplateInitListResult
        {
            IsAddNewAccessable = SecurityRepository.IsActionAccessible(ActionCode.AddNewPageTemplate) && SecurityRepository.IsEntityAccessible(EntityTypeCode.PageTemplate, contentId, ActionTypeCode.Update)
        };

        public PageTemplate NewPageTemplateProperties(int parentId) => PageTemplate.Create(parentId, SiteRepository.GetById(parentId));

        public PageTemplate NewPageTemplatePropertiesForUpdate(int parentId) => NewPageTemplateProperties(parentId);

        public PageTemplate SavePageTemplateProperties(PageTemplate template)
        {
            ManagePageTemplateFolders(template, FolderManagingType.CreateFolder);
            return PageTemplateRepository.SaveProperties(template);
        }

        private void ManagePageTemplateFolders(PageTemplate template, FolderManagingType type)
        {
            var stageDirectory = template.Site.StageDirectory + Path.DirectorySeparatorChar + template.TemplateFolder;
            var liveDirectory = template.Site.LiveDirectory + Path.DirectorySeparatorChar + template.TemplateFolder;

            switch (type)
            {
                case FolderManagingType.CreateFolder:
                    if (!Directory.Exists(stageDirectory))
                    {
                        Directory.CreateDirectory(stageDirectory);
                    }

                    if (!Directory.Exists(liveDirectory))
                    {
                        Directory.CreateDirectory(liveDirectory);
                    }

                    break;
                case FolderManagingType.DeleteFolder:
                    if (!string.IsNullOrWhiteSpace(template.TemplateFolder))
                    {
                        if (Directory.Exists(stageDirectory))
                        {
                            Directory.Delete(stageDirectory, true);
                        }

                        if (Directory.Exists(liveDirectory))
                        {
                            Directory.Delete(liveDirectory, true);
                        }
                    }

                    break;
                case FolderManagingType.ChangeFolder:
                    var oldTemplate = ReadPageTemplateProperties(template.Id, false);
                    if (oldTemplate == null)
                    {
                        throw new ApplicationException(string.Format(TemplateStrings.TemplateNotFound, template.Id));
                    }

                    var oldFolder = oldTemplate.TemplateFolder;
                    if (!string.IsNullOrWhiteSpace(oldTemplate.TemplateFolder))
                    {
                        if (string.IsNullOrWhiteSpace(template.TemplateFolder))
                        {
                            ManagePageTemplateFolders(oldTemplate, FolderManagingType.DeleteFolder);
                            return;
                        }

                        if (template.TemplateFolder == oldFolder)
                        {
                            return;
                        }

                        var oldStageDirectory = oldTemplate.Site.StageDirectory + Path.DirectorySeparatorChar + oldFolder;
                        var oldLiveDirectory = oldTemplate.Site.LiveDirectory + Path.DirectorySeparatorChar + oldFolder;
                        if (Directory.Exists(oldStageDirectory))
                        {
                            Directory.Move(oldStageDirectory, stageDirectory);
                        }

                        if (Directory.Exists(oldLiveDirectory))
                        {
                            Directory.Move(oldLiveDirectory, liveDirectory);
                        }
                    }
                    else if (!string.IsNullOrWhiteSpace(template.TemplateFolder))
                    {
                        ManagePageTemplateFolders(template, FolderManagingType.CreateFolder);
                    }

                    break;
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
            var template = PageTemplateRepository.GetPageTemplatePropertiesById(id);
            if (template == null)
            {
                throw new ApplicationException(string.Format(TemplateStrings.TemplateNotFound, id));
            }

            if (withAutoLock)
            {
                template.AutoLock();
            }

            template.LoadLockedByUser();
            return template;
        }

        public PageTemplate ReadPageTemplatePropertiesForUpdate(int id) => ReadPageTemplateProperties(id, false);

        public PageTemplate UpdatePageTemplateProperties(PageTemplate pageTemplate)
        {
            ManageTemplateInheritance(pageTemplate);
            ManagePageTemplateFolders(pageTemplate, FolderManagingType.ChangeFolder);
            return PageTemplateRepository.UpdatePageTemplateProperties(pageTemplate);
        }

        public MessageResult RemovePageTemplate(int id)
        {
            var template = PageTemplateRepository.GetPageTemplatePropertiesById(id);
            if (template == null)
            {
                throw new ApplicationException(string.Format(TemplateStrings.TemplateNotFound, id));
            }

            if (template.LockedByAnyoneElse)
            {
                return MessageResult.Error(string.Format(TemplateStrings.LockedByAnyoneElse, template.LockedByDisplayName));
            }

            ManagePageTemplateFolders(template, FolderManagingType.DeleteFolder);
            PageTemplateRepository.DeletePageTemplate(id);

            return null;
        }

        public Page ReadPageProperties(int id, bool withAutoLock = true)
        {
            var page = PageRepository.GetPagePropertiesById(id);
            if (page == null)
            {
                throw new ApplicationException(string.Format(TemplateStrings.PageNotFound, id));
            }

            if (withAutoLock)
            {
                page.AutoLock();
            }

            page.LoadLockedByUser();
            return page;
        }

        public void CancelTemplate(int id)
        {
            var template = PageTemplateRepository.GetPageTemplatePropertiesById(id);
            if (template == null)
            {
                throw new Exception(string.Format(TemplateStrings.TemplateNotFound, id));
            }

            template.AutoUnlock();
        }

        public string ReadDefaultCode(int formatId)
        {
            var format = ObjectFormatRepository.ReadObjectFormat(formatId, true);
            var obj = ObjectRepository.GetObjectPropertiesById(format.ParentEntityId);
            var netLanguagePrefix = GetLangPrefix(format.NetLanguageId);
            var pathToCopy = SitePathRepository.GetDirectoryPathToCopy() + Path.DirectorySeparatorChar + "default" + Path.DirectorySeparatorChar;
            return ReadFileAsString(obj.IsObjectContainerType ? $"{pathToCopy}container_code_{netLanguagePrefix}.txt" : $"{pathToCopy}generic_code_{netLanguagePrefix}.txt");
        }

        public string ReadDefaultPresentation(int formatId)
        {
            var format = ObjectFormatRepository.ReadObjectFormat(formatId, true);
            var obj = ObjectRepository.GetObjectPropertiesById(format.ParentEntityId);
            var pathToCopy = SitePathRepository.GetDirectoryPathToCopy() + Path.DirectorySeparatorChar + "default" + Path.DirectorySeparatorChar;
            return obj.IsObjectContainerType ? ReadFileAsString($"{pathToCopy}container_presentation.txt") : string.Empty;
        }

        private static string ReadFileAsString(string path)
        {
            var sb = new StringBuilder();
            var objReader = new StreamReader(path);
            var sLine = string.Empty;
            while (sLine != null)
            {
                sLine = objReader.ReadLine();
                if (sLine != null)
                {
                    sb.AppendLine(sLine);
                }
            }

            objReader.Close();
            return sb.ToString();
        }

        private static string GetLangPrefix(int? langId)
        {
            var netLanguagePrefix = string.Empty;
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

        private static void SetPagesAndObjectsEnableViewState(int pageTemplateId, bool enableViewState)
        {
            using (var scope = new QPConnectionScope())
            {
                Common.SetPagesAndObjectsEnableViewState(scope.DbConnection, pageTemplateId, enableViewState);
            }
        }

        private static void SetObjectsDisableDataBinding(int pageTemplateId, bool disableDataBinding)
        {
            using (var scope = new QPConnectionScope())
            {
                Common.SetObjectsDisableDataBinding(scope.DbConnection, pageTemplateId, disableDataBinding);
            }
        }

        private static void SetCustomClassForPages(int pageTemplateId, string customClass)
        {
            using (var scope = new QPConnectionScope())
            {
                Common.SetCustomClassForPages(scope.DbConnection, pageTemplateId, customClass);
            }
        }

        private static void SetCustomClassForObjects(int pageTemplateId, string customClassForGenerics, string customClassForContainers, string customClassForForms)
        {
            using (var scope = new QPConnectionScope())
            {
                Common.SetCustomClassForObjects(scope.DbConnection, pageTemplateId, customClassForGenerics, customClassForContainers, customClassForForms);
            }
        }

        private static void ManageTemplateInheritance(PageTemplate pageTemplate)
        {
            if (pageTemplate.ApplyToExistingPagesAndObjects)
            {
                SetPagesAndObjectsEnableViewState(pageTemplate.Id, pageTemplate.EnableViewstate);
            }

            if (pageTemplate.ApplyToExistingObjects)
            {
                SetObjectsDisableDataBinding(pageTemplate.Id, pageTemplate.DisableDatabind);
            }

            if (pageTemplate.OverridePageSettings)
            {
                SetCustomClassForPages(pageTemplate.Id, pageTemplate.CustomClassForPages);
            }

            if (pageTemplate.OverrideObjectSettings)
            {
                SetCustomClassForObjects(pageTemplate.Id, pageTemplate.CustomClassForGenerics, pageTemplate.CustomClassForContainers, pageTemplate.CustomClassForForms);
            }
        }

        public Content GetContentById(int contentId) => ContentRepository.GetById(contentId);

        public IEnumerable<int> GetStatusIdsByContentId(int contentId, out bool hasWf) => PageTemplateRepository.GetStatusIdsByContentId(contentId, out hasWf);

        public void CaptureLockTemplate(int id)
        {
            var template = PageTemplateRepository.GetPageTemplatePropertiesById(id);
            if (template == null)
            {
                throw new Exception(string.Format(TemplateStrings.TemplateNotFound, id));
            }

            if (template.CanBeUnlocked)
            {
                EntityObjectRepository.CaptureLock(template);
            }
        }

        public BllObject ReadObjectProperties(int id, bool withAutoLock = true)
        {
            var obj = ObjectRepository.GetObjectPropertiesById(id);
            if (obj == null)
            {
                throw new ApplicationException(string.Format(TemplateStrings.ObjectNotFound, id));
            }

            if (withAutoLock)
            {
                obj.AutoLock();
            }

            obj.LoadLockedByUser();
            return obj;
        }

        public MessageResult AssemblePageFromPageObject(int pageId) => AssemblePage(pageId);

        public MessageResult AssemblePageFromPageObjectPreAction(int pageId) => AssemblePagePreAction(pageId);

        public MessageResult AssemblePageFromPageObjectFormatPreAction(int objectId)
        {
            var props = ReadObjectProperties(objectId, false);
            if (props.PageId == null)
            {
                throw new ArgumentException(@"Wrong argument", nameof(objectId));
            }

            return AssemblePagePreAction(props.PageId.Value);
        }

        public MessageResult AssemblePageFromPageObjectList(int parentId) => AssemblePage(parentId);

        public MessageResult AssemblePageFromPageObjectListPreAction(int parentId) => AssemblePagePreAction(parentId);

        public MessageResult AssembleObjectFromPageObjectFormat(int parentId) => AssembleObject(parentId);

        public MessageResult AssembleObjectFromPageObjectFormatPreAction(int parentId) => AssembleObjectPreAction(parentId);

        public MessageResult AssembleObjectFromTemplateObjectFormatPreAction(int parentId) => AssembleObjectPreAction(parentId);

        public MessageResult AssembleObjectFromTemplateObjectFormat(int parentId) => AssembleObject(parentId);

        public MessageResult AssembleObjectPreAction(int id)
        {
            var site = ObjectRepository.GetObjectPropertiesById(id).PageTemplate.Site;
            var message = !site.IsLive ? null : string.Format(SiteStrings.SiteInLiveWarning, site.ModifiedToDisplay, site.LastModifiedByUserToDisplay);
            return string.IsNullOrEmpty(message) ? null : MessageResult.Confirm(message);
        }

        public MessageResult AssembleObject(int id)
        {
            var obj = ObjectRepository.GetObjectPropertiesById(id);
            if (obj.PageTemplate.SiteIsDotNet)
            {
                new AssembleSelectedObjectsController(id.ToString(), QPContext.CurrentDbConnectionString).Assemble();
                return null;
            }

            return MessageResult.Error(SiteStrings.ShouldBeDotNet);
        }

        public MessageResult AssemblePageFromPageObjectFormat(int parentId)
        {
            var props = ReadObjectProperties(parentId);
            if (props.PageId == null)
            {
                throw new ArgumentException(@"Wrong argument", nameof(parentId));
            }

            return AssemblePage(props.PageId.Value);
        }

        public IEnumerable<PageTemplate> GetAllSiteTemplates(int siteId) => PageTemplateRepository.GetSiteTemplates(siteId);

        public ListResult<ObjectFormatSearchResultListItem> FormatSearch(ListCommand listCommand, int siteId, int? templateId, int? pageId, string filter)
        {
            var data = PageTemplateRepository.GetSearchFormatPage(listCommand, siteId, templateId, pageId, filter, out var totalRecords).ToList();
            ManageFormatSearchItemsDescription(data, filter);
            return new ListResult<ObjectFormatSearchResultListItem>
            {
                Data = data,
                TotalRecords = totalRecords
            };
        }

        private static void ManageFormatSearchItemsDescription(IEnumerable<ObjectFormatSearchResultListItem> data, string filter)
        {
            if (string.IsNullOrWhiteSpace(filter))
            {
                return;
            }

            foreach (var item in data)
            {
                var format = ObjectFormatRepository.ReadObjectFormat(item.Id, true);
                if (!string.IsNullOrWhiteSpace(format.CodeBehind) && format.CodeBehind.Contains(filter))
                {
                    item.Description = FoundTextMarker.GetSimpleRelevantMarkedText(WebUtility.HtmlEncode(format.CodeBehind), filter, 20, "<span class='seachResultHighlight'>", "</span>");
                    continue;
                }

                if (!string.IsNullOrWhiteSpace(format.FormatBody) && format.FormatBody.Contains(filter))
                {
                    item.Description = FoundTextMarker.GetSimpleRelevantMarkedText(WebUtility.HtmlEncode(format.FormatBody), filter, 20, "<span class='seachResultHighlight'>", "</span>");
                }
            }
        }

        public ListResult<PageTemplateSearchListItem> TemplateSearch(ListCommand listCommand, int siteId, string filter)
        {
            var data = PageTemplateRepository.GetSearchTemplatePage(listCommand, siteId, filter, out var totalRecords).ToList();
            ManageTemplateSearchItemsDescription(data, filter);
            return new ListResult<PageTemplateSearchListItem>
            {
                Data = data,
                TotalRecords = totalRecords
            };
        }

        private static void ManageTemplateSearchItemsDescription(IEnumerable<PageTemplateSearchListItem> data, string filter)
        {
            if (string.IsNullOrWhiteSpace(filter))
            {
                return;
            }

            foreach (var item in data)
            {
                var template = PageTemplateRepository.GetPageTemplatePropertiesById(item.Id);
                if (!string.IsNullOrWhiteSpace(template.CodeBehind) && template.CodeBehind.Contains(filter))
                {
                    item.Description = FoundTextMarker.GetSimpleRelevantMarkedText(WebUtility.HtmlEncode(template.CodeBehind), filter, 20, "<span class='seachResultHighlight'>", "</span>");
                    continue;
                }

                if (!string.IsNullOrWhiteSpace(template.TemplateBody) && template.TemplateBody.Contains(filter))
                {
                    item.Description = FoundTextMarker.GetSimpleRelevantMarkedText(WebUtility.HtmlEncode(template.TemplateBody), filter, 20, "<span class='seachResultHighlight'>", "</span>");
                }
            }
        }

        public ListResult<ObjectSearchListItem> ObjectSearch(ListCommand listCommand, int siteId, int? templateId, int? pageId, string filter)
        {
            var data = PageTemplateRepository.GetSearchObjectPage(listCommand, siteId, templateId, pageId, filter, out var totalRecords).ToList();
            ManageObjectSearchItemsDescription(data, filter);
            return new ListResult<ObjectSearchListItem>
            {
                Data = data,
                TotalRecords = totalRecords
            };
        }

        private static void ManageObjectSearchItemsDescription(IEnumerable<ObjectSearchListItem> data, string filter)
        {
            if (string.IsNullOrWhiteSpace(filter))
            {
                return;
            }

            foreach (var item in data)
            {
                var obj = ObjectRepository.GetObjectPropertiesById(item.Id);
                var defaultValues = obj.DefaultValues?.ToList();
                if (defaultValues != null && defaultValues.Any())
                {
                    var names = defaultValues.Where(x => x.VariableName.Contains(filter)).ToList();
                    var dName = names.Any() ? names.First() : null;
                    if (dName != null)
                    {
                        item.Description = FoundTextMarker.GetSimpleRelevantMarkedText(WebUtility.HtmlEncode(dName.VariableName), filter, 20, "<span class='seachResultHighlight'>", "</span>");
                        continue;
                    }

                    var filteredDefaultValues = defaultValues.Where(x => x.VariableValue.Contains(filter)).ToList();
                    var dVal = filteredDefaultValues.Any() ? filteredDefaultValues.First() : null;
                    if (dVal != null)
                    {
                        item.Description = FoundTextMarker.GetSimpleRelevantMarkedText(WebUtility.HtmlEncode(dVal.VariableValue), filter, 20, "<span class='seachResultHighlight'>", "</span>");
                        continue;
                    }
                }

                if (obj.Container != null)
                {
                    if (!string.IsNullOrWhiteSpace(obj.Container.FilterValue) && obj.Container.FilterValue.Contains(filter))
                    {
                        item.Description = FoundTextMarker.GetSimpleRelevantMarkedText(WebUtility.HtmlEncode(obj.Container.FilterValue), filter, 20, "<span class='seachResultHighlight'>", "</span>");
                        continue;
                    }

                    if (!string.IsNullOrWhiteSpace(obj.Container.OrderDynamic) && obj.Container.OrderDynamic.Contains(filter))
                    {
                        item.Description = FoundTextMarker.GetSimpleRelevantMarkedText(WebUtility.HtmlEncode(obj.Container.OrderDynamic), filter, 20, "<span class='seachResultHighlight'>", "</span>");
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
                    }
                }
            }
        }

        public IEnumerable<BllObject> GetAllTemplateObjects(int templateId) => ObjectRepository.GetTemplateObjects(templateId);

        public IEnumerable<BllObject> GetAllPageObjects(int pageId) => ObjectRepository.GetPageObjects(pageId);

        public Content ReadContentProperties(int id) => ContentRepository.GetByIdWithFields(id);

        IEnumerable<TemplateObjectFormatDto> IPageTemplateService.GetRestTemplateObjects(int templateId) => ObjectRepository.GetRestTemplateObjects(templateId);

        public ObjectFormat ReadFormatProperties(int id, bool pageOrTemplate, bool withAutoLock = true)
        {
            var format = ObjectFormatRepository.ReadObjectFormat(id, pageOrTemplate);
            if (format == null)
            {
                throw new ApplicationException(string.Format(TemplateStrings.FormatNotFound, id));
            }

            if (withAutoLock)
            {
                format.AutoLock();
            }
            format.LoadLockedByUser();
            format.PageOrTemplate = pageOrTemplate;
            format.ReplacePlaceHoldersToUrls();
            return format;
        }

        public MessageResult AssemblePage(int id)
        {
            var page = PageRepository.GetPagePropertiesById(id);
            if (page.PageTemplate.SiteIsDotNet)
            {
                new AssemblePageController(id, QPContext.CurrentDbConnectionString).Assemble();
                AssembleRepository.UpdatePageStatus(id, QPContext.CurrentUserId);
                return null;
            }

            return MessageResult.Error(SiteStrings.ShouldBeDotNet);
        }

        public MessageResult AssemblePagePreAction(int id)
        {
            var site = PageRepository.GetPagePropertiesById(id).PageTemplate.Site;
            var message = !site.IsLive ? null : string.Format(SiteStrings.SiteInLiveWarning, site.ModifiedToDisplay, site.LastModifiedByUserToDisplay);
            return string.IsNullOrEmpty(message) ? null : MessageResult.Confirm(message);
        }

        public static int CopySiteTemplates(int sourceSiteId, int destinationSiteId, int templateNumber)
        {
            var templateIdNew = PageTemplateRepository.CopySiteTemplates(sourceSiteId, destinationSiteId, templateNumber);
            var relBetweenTemplates = PageTemplateRepository.GetRelationsBetweenTemplates(sourceSiteId, destinationSiteId, templateIdNew);
            var relBetweenContents = ContentRepository.GetRelationsBetweenContentsXml(sourceSiteId, destinationSiteId, string.Empty);
            PageRepository.CopySiteTemplatePages(sourceSiteId, destinationSiteId, relBetweenTemplates);

            var relBetweenPages = PageRepository.GetRelationsBetweenPages(relBetweenTemplates);
            ObjectRepository.CopySiteTemplateObjects(relBetweenTemplates, relBetweenPages, out var relBetweenObjects);

            ObjectFormatRepository.CopySiteTemplateObjectFormats(relBetweenObjects, out var relBetweenObjectFormats);

            ObjectRepository.CopySiteUpdateObjects(relBetweenObjectFormats, relBetweenObjects);
            ObjectRepository.CopySiteObjectValues(relBetweenObjects);
            ObjectRepository.CopySiteContainers(relBetweenObjects, relBetweenContents);

            var relBetweenStatuses = ContentRepository.GetRelationsBetweenStatuses(sourceSiteId, destinationSiteId);
            ObjectRepository.CopyContainerStatuses(relBetweenStatuses, relBetweenObjects);

            NotificationRepository.CopySiteUpdateNotifications(relBetweenObjectFormats, relBetweenContents);

            return templateIdNew != 0 ? 1 : 0;
        }
    }
}
