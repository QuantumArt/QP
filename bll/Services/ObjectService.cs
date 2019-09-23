#if !NET_STANDARD
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Quantumart.QP8.Assembling;
using Quantumart.QP8.BLL.ListItems;
using Quantumart.QP8.BLL.Repository;
using Quantumart.QP8.BLL.Repository.ContentRepositories;
using Quantumart.QP8.BLL.Services.DTO;
using Quantumart.QP8.Constants;
using Quantumart.QP8.Resources;

namespace Quantumart.QP8.BLL.Services
{
    public class ObjectService : IObjectService
    {
        public MessageResult PromotePageObject(int id)
        {
            var obj = ReadObjectProperties(id);
            if (obj.ParentObjectId.HasValue)
            {
                return MessageResult.Info(TemplateStrings.UnableToPromote);
            }

            obj.PageId = null;
            UpdateObjectProperties(obj, null);
            return MessageResult.Info(string.Format(TemplateStrings.ObjectPromoted, obj.Name));
        }

        public BllObject ReadObjectPropertiesForUpdate(int id) => ReadObjectProperties(id, false);

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

        /// <param name="isTemplateObject"></param>
        /// else it`s page object
        public ObjectInitListResult InitObjectList(int parentId, bool isTemplateObject) => new ObjectInitListResult
        {
            IsAddNewAccessable = isTemplateObject ? SecurityRepository.IsActionAccessible(ActionCode.AddNewTemplateObject) && SecurityRepository.IsEntityAccessible(EntityTypeCode.TemplateObject, parentId, ActionTypeCode.Update) : SecurityRepository.IsActionAccessible(ActionCode.AddNewPageObject) && SecurityRepository.IsEntityAccessible(EntityTypeCode.PageObject, parentId, ActionTypeCode.Update)
        };

        public ListResult<ObjectListItem> GetTemplateObjectsByTemplateId(ListCommand listCommand, int parentId)
        {
            var list = ObjectRepository.ListTemplateObjects(listCommand, parentId, out var totalRecords);
            return new ListResult<ObjectListItem>
            {
                Data = list.ToList(),
                TotalRecords = totalRecords
            };
        }

        public IEnumerable<StatusType> GetActiveStatusesByObjectId(int objectId) => PageTemplateRepository.GetActiveStatusesByObjectId(objectId);

        public ListResult<ObjectListItem> GetPageObjectsByPageId(ListCommand listCommand, int parentId)
        {
            var list = ObjectRepository.ListPageObjects(listCommand, parentId, out var totalRecords);
            return new ListResult<ObjectListItem>
            {
                Data = list.ToList(),
                TotalRecords = totalRecords
            };
        }

        public BllObject NewObjectProperties(int parentId, bool pageOrTemplate)
        {
            var obj = BllObject.Create(parentId, pageOrTemplate);
            obj.PageTemplate = pageOrTemplate ? PageRepository.GetPagePropertiesById(parentId).PageTemplate : PageTemplateRepository.GetPageTemplatePropertiesById(parentId);
            return obj;
        }

        public BllObject NewObjectPropertiesForUpdate(int parentId, bool pageOrTemplate) => NewObjectProperties(parentId, pageOrTemplate);

        public BllObject SaveObjectProperties(BllObject bllObject, IEnumerable<int> activeStatuses, bool isReplayAction)
        {
            var result = ObjectRepository.SaveObjectProperties(bllObject);
            if (activeStatuses != null)
            {
                result.BindWithStatuses(activeStatuses, result.IsObjectContainerType);
            }
            CreateDefaultFormat(result, isReplayAction);
            return result;
        }

        public BllObject UpdateObjectProperties(BllObject bllObject, IEnumerable<int> activeStatuses)
        {
            var result = ObjectRepository.UpdateObjectProperties(bllObject);
            if (activeStatuses != null)
            {
                bllObject.BindWithStatuses(activeStatuses, bllObject.IsObjectContainerType);
            }
            return result;
        }

        private int? CreateDefaultFormat(BllObject bllObject, bool isReplayAction)
        {
            var template = PageTemplateRepository.GetPageTemplatePropertiesById(bllObject.PageTemplateId);
            var format = ObjectFormat.Create(bllObject.Id, bllObject.PageOrTemplate, template.SiteIsDotNet);
            format.Name = "default";
            if (template.SiteIsDotNet)
            {
                format.NetFormatName = "default";
            }

            else

            {
                format.NetLanguageId = template.NetLanguageId;
            }

            var netLanguagePrefix = GetLangPrefix(template.NetLanguageId);

            var pathToCopy = SitePathRepository.GetDirectoryPathToCopy() + "\\default\\";

            if (template.NetLanguageId != null && !isReplayAction)
            {
                if (bllObject.IsGenericType)
                {
                    format.CodeBehind = ReadFileAsString(string.Format("{0}generic_code_{1}.txt", pathToCopy, netLanguagePrefix));
                }

                else if (bllObject.IsObjectContainerType)
                {
                    format.CodeBehind = ReadFileAsString(string.Format("{0}container_code_{1}.txt", pathToCopy, netLanguagePrefix));
                    format.FormatBody = ReadFileAsString(string.Format("{0}container_presentation.txt", pathToCopy));
                }
            }
            format = FormatRepository.SaveObjectFormatProperties(format);

            bllObject.DefaultFormatId = format.Id;
            ObjectRepository.UpdateDefaultFormatId(bllObject.Id, format.Id);

            return format.Id;
        }

        private string ReadFileAsString(string path)
        {
            var sb = new StringBuilder();

            var objReader = new StreamReader(path);
            var sLine = "";

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
            var netLanguagePrefix = "";

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

        public MessageResult RemoveObject(int id)
        {
            var obj = ObjectRepository.GetObjectPropertiesById(id);
            if (obj == null)
            {
                throw new ApplicationException(string.Format(TemplateStrings.ObjectNotFound, id));
            }

            if (obj.LockedByAnyoneElse)
            {
                return MessageResult.Error(string.Format(TemplateStrings.LockedByAnyoneElse, obj.LockedByDisplayName));
            }
            if (obj.ChildObjectFormats.Any(x => x.Notifications.Count() > 0))
            {
                return MessageResult.Error(TemplateStrings.UnableToDeleteObject);
            }

            ObjectRepository.DeleteObject(id);
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

        public MessageResult MultipleRemovePageObject(int[] IDs)
        {
            if (IDs == null)
            {
                throw new ArgumentNullException(nameof(IDs));
            }

            PageTemplateRepository.MultipleDeleteObject(IDs);

            return null;
        }

        public MessageResult MultipleRemoveTemplateObject(int[] IDs)
        {
            if (IDs == null)
            {
                throw new ArgumentNullException(nameof(IDs));
            }

            PageTemplateRepository.MultipleDeleteObject(IDs);

            return null;
        }

        public void CancelObject(int id)
        {
            var obj = ObjectRepository.GetObjectPropertiesById(id);
            if (obj == null)
            {
                throw new Exception(string.Format(TemplateStrings.ObjectNotFound, id));
            }

            obj.AutoUnlock();
        }

        public MessageResult MultipleAssembleObjectPreAction(int[] ids)
        {
            if (ids == null)
            {
                throw new ArgumentNullException("IDs");
            }

            var site = ObjectRepository.GetObjectPropertiesById(ids[0]).PageTemplate.Site;
            var message = !site.IsLive ? null : string.Format(SiteStrings.SiteInLiveWarning, site.ModifiedToDisplay, site.LastModifiedByUserToDisplay);
            return string.IsNullOrEmpty(message) ? null : MessageResult.Confirm(message);
        }

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
                new AssembleSelectedObjectsController(id.ToString(), QPContext.CurrentDbConnectionString);
                return null;
            }

            return MessageResult.Error(SiteStrings.ShouldBeDotNet);
        }

        public MessageResult MultipleAssembleObject(int[] ids)
        {
            foreach (var id in ids)
            {
                var obj = ObjectRepository.GetObjectPropertiesById(id);
                if (obj.PageTemplate.SiteIsDotNet)
                {
                    new AssembleSelectedObjectsController(id.ToString(), QPContext.CurrentDbConnectionString).Assemble();
                }
                else
                {
                    return MessageResult.Error(SiteStrings.ShouldBeDotNet);
                }
            }

            return null;
        }

        public IEnumerable<ListItem> GetTypes()
        {
            return PageTemplateRepository.GetTypesList().Select(x => new ListItem { Text = x.Name, Value = x.Id.ToString(), HasDependentItems = true, DependentItemIDs = new[] { x.Name.Replace(' ', '_') + "_Panel" } }).ToArray();
        }

        public int GetPublishedStatusIdBySiteId(int id) => StatusTypeRepository.GetPublishedStatusIdBySiteId(id);

        public IEnumerable<ListItem> GetPermissionLevels()
        {
            return PageTemplateRepository.GetPermissionLevels().OrderBy(x => x.Level).Select(x => new ListItem { Text = x.Name, Value = x.Id.ToString() });
        }

        public Content GetContentById(int contentId) => ContentRepository.GetById(contentId);

        public IEnumerable<ListItem> GetNetLanguagesAsListItems()
        {
            return PageTemplateRepository.GetNetLanguagesList().Select(lang => new ListItem { Text = lang.Name, Value = lang.Id.ToString() })
                .ToArray();
        }

        public IEnumerable<BllObject> GetFreeTemplateObjectsByPageId(int pageId) => PageTemplateRepository.GetFreeTemplateObjects(pageId);

        public void CaptureLockPageObject(int id)
        {
            var obj = ObjectRepository.GetObjectPropertiesById(id);
            if (obj == null)
            {
                throw new Exception(string.Format(TemplateStrings.ObjectNotFound, id));
            }

            if (obj.CanBeUnlocked)
            {
                EntityObjectRepository.CaptureLock(obj);
            }
        }

        public void CaptureLockTemplateObject(int id)
        {
            var obj = ObjectRepository.GetObjectPropertiesById(id);
            if (obj == null)
            {
                throw new Exception(string.Format(TemplateStrings.ObjectNotFound, id));
            }

            if (obj.CanBeUnlocked)
            {
                EntityObjectRepository.CaptureLock(obj);
            }
        }
    }
}
#endif
