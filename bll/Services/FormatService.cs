using System;
using System.Collections.Generic;
using System.Linq;
using Quantumart.QP8.BLL.ListItems;
using Quantumart.QP8.BLL.Repository;
using Quantumart.QP8.BLL.Services.DTO;
using Quantumart.QP8.Constants;
using Quantumart.QP8.Resources;

namespace Quantumart.QP8.BLL.Services
{
    public interface IFormatService
    {
        FormatInitListResult InitFormatList(int parentId, bool isTemplateObject);

        FormatVersionInitListResult InitFormatVersionList();

        IEnumerable<ListItem> GetNetLanguagesAsListItems();

        PageTemplate ReadPageTemplateProperties(int id, bool withAutoLock = true);

        BllObject ReadObjectProperties(int id, bool withAutoLock = true);

        MessageResult RemoveObjectFormat(int id, bool pageOrTemplate);

        void CancelFormat(int id, bool pageOrTemplate);

        ObjectFormat NewPageObjectFormatProperties(int parentId, bool isSiteDotNet);

        ObjectFormat ReadPageObjectFormatPropertiesForUpdate(int parentId, bool isSiteDotNet);

        ObjectFormat SaveObjectFormatProperties(ObjectFormat objectFormat);

        ObjectFormat NewPageObjectFormatPropertiesForUpdate(int parentId, bool isSiteDotNet);

        ObjectFormat ReadPageObjectFormatProperties(int parentId, bool isSiteDotNet);

        ObjectFormat UpdateObjectFormatProperties(ObjectFormat objectFormat);

        ObjectFormat ReadTemplateObjectFormatProperties(int parentId, bool isSiteDotNet);

        ObjectFormat NewTemplateObjectFormatProperties(int parentId, bool isSiteDotNet);

        ObjectFormat NewTemplateObjectFormatPropertiesForUpdate(int parentId, bool isSiteDotNet);

        ObjectFormat ReadTemplateObjectFormatPropertiesForUpdate(int parentId, bool isSiteDotNet);

        ListResult<ObjectFormatListItem> GetPageObjectFormatsByObjectId(ListCommand listCommand, int parentId);

        ListResult<ObjectFormatListItem> GetTemplateObjectFormatsByObjectId(ListCommand listCommand, int parentId);

        bool IsSiteDotNetByObjectId(int objectId);

        void CaptureLockTemplateObjectFormat(int id);

        void CaptureLockPageObjectFormat(int id);

        ListResult<ObjectFormatVersionListItem> GetTemplateObjectFormatVersionsByFormatId(ListCommand listCommand, int parentId);

        ListResult<ObjectFormatVersionListItem> GetPageObjectFormatVersionsByFormatId(ListCommand listCommand, int parentId);

        ObjectFormatVersion ReadTemplateObjectFormatVersionProperties(int id);

        ObjectFormatVersion ReadPageObjectFormatVersionProperties(int id);

        ObjectFormatVersion GetMergedObjectFormatVersion(int[] ids, int parentId, bool pageOrtemplate);

        MessageResult MultipleRemoveObjectFormatVersion(int[] ids);

        MessageResult RestoreObjectFormatVersion(int versionId);
    }

    public class FormatService : IFormatService
    {
        public FormatInitListResult InitFormatList(int parentId, bool isTemplateFormat) => new FormatInitListResult
        {
            IsAddNewAccessable = isTemplateFormat ? SecurityRepository.IsActionAccessible(ActionCode.AddNewTemplateObjectFormat) && SecurityRepository.IsEntityAccessible(EntityTypeCode.TemplateObjectFormat, parentId, ActionTypeCode.Update) : SecurityRepository.IsActionAccessible(ActionCode.AddNewPageObjectFormat) && SecurityRepository.IsEntityAccessible(EntityTypeCode.PageObjectFormat, parentId, ActionTypeCode.Update)
        };

        public FormatVersionInitListResult InitFormatVersionList() => new FormatVersionInitListResult
        {
            IsAddNewAccessable = false
        };

        public IEnumerable<ListItem> GetNetLanguagesAsListItems()
        {
            return PageTemplateRepository.GetNetLanguagesList().Select(lang => new ListItem { Text = lang.Name, Value = lang.Id.ToString() }).ToArray();
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

        public MessageResult RemoveObjectFormat(int id, bool pageOrTemplate)
        {
            var format = ObjectFormatRepository.ReadObjectFormat(id, pageOrTemplate);
            if (format == null)
            {
                throw new ApplicationException(string.Format(TemplateStrings.FormatNotFound, id));
            }

            if (format.LockedByAnyoneElse)
            {
                return MessageResult.Error(string.Format(TemplateStrings.LockedByAnyoneElse, format.LockedByDisplayName));
            }
            if (format.Notifications.Any())
            {
                return MessageResult.Error(TemplateStrings.UnableToDeleteFormat);
            }

            ManagePageAndObjectModified(format);
            PageTemplateRepository.DeleteObjectFormat(id);
            return null;
        }

        private static void ManagePageAndObjectModified(ObjectFormat format)
        {
            PageTemplateRepository.ManagePageAndObjectModified(format);
        }

        public void CancelFormat(int id, bool pageOrTemplate)
        {
            var format = ObjectFormatRepository.ReadObjectFormat(id, pageOrTemplate);
            if (format == null)
            {
                throw new ApplicationException(string.Format(TemplateStrings.FormatNotFound, id));
            }

            format.AutoUnlock();
        }

        public ObjectFormat NewPageObjectFormatProperties(int parentId, bool isSiteDotNet) => ObjectFormat.Create(parentId, true, isSiteDotNet);

        public ObjectFormat ReadPageObjectFormatPropertiesForUpdate(int parentId, bool isSiteDotNet)
        {
            var result = ReadFormatProperties(parentId, true, false);
            result.IsSiteDotNet = isSiteDotNet;
            return result;
        }

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

        public ObjectFormat SaveObjectFormatProperties(ObjectFormat objectFormat)
        {
            objectFormat.ReplaceUrlsToPlaceHolders();
            var format = FormatRepository.SaveObjectFormatProperties(objectFormat);
            ManagePageAndObjectModified(format);
            return format;
        }

        public ObjectFormat NewPageObjectFormatPropertiesForUpdate(int parentId, bool isSiteDotNet) => NewPageObjectFormatProperties(parentId, isSiteDotNet);

        public ObjectFormat ReadPageObjectFormatProperties(int parentId, bool isSiteDotNet)
        {
            var result = ReadFormatProperties(parentId, true);
            result.IsSiteDotNet = isSiteDotNet;
            return result;
        }

        public ObjectFormat UpdateObjectFormatProperties(ObjectFormat objectFormat)
        {
            objectFormat.ReplaceUrlsToPlaceHolders();
            var format = FormatRepository.UpdateObjectFormatProperties(objectFormat);
            ManagePageAndObjectModified(format);
            return format;
        }

        public ObjectFormat ReadTemplateObjectFormatProperties(int parentId, bool isSiteDotNet)
        {
            var result = ReadFormatProperties(parentId, false);
            result.IsSiteDotNet = isSiteDotNet;
            return result;
        }

        public ObjectFormat NewTemplateObjectFormatProperties(int parentId, bool isSiteDotNet) => ObjectFormat.Create(parentId, false, isSiteDotNet);

        public ObjectFormat NewTemplateObjectFormatPropertiesForUpdate(int parentId, bool isSiteDotNet) => NewTemplateObjectFormatProperties(parentId, isSiteDotNet);

        public ObjectFormat ReadTemplateObjectFormatPropertiesForUpdate(int parentId, bool isSiteDotNet)
        {
            var result = ReadFormatProperties(parentId, false, false);
            result.IsSiteDotNet = isSiteDotNet;
            return result;
        }

        public ListResult<ObjectFormatListItem> GetPageObjectFormatsByObjectId(ListCommand listCommand, int parentId) => GetObjectFormatsByObjectId(listCommand, parentId, true);

        private static ListResult<ObjectFormatListItem> GetObjectFormatsByObjectId(ListCommand listCommand, int objectId, bool pageOrTemplate)
        {
            var list = PageTemplateRepository.ListObjectFormats(listCommand, objectId, out var totalRecords, pageOrTemplate);
            return new ListResult<ObjectFormatListItem>
            {
                Data = list.ToList(),
                TotalRecords = totalRecords
            };
        }

        public ListResult<ObjectFormatListItem> GetTemplateObjectFormatsByObjectId(ListCommand listCommand, int parentId) => GetObjectFormatsByObjectId(listCommand, parentId, false);

        public bool IsSiteDotNetByObjectId(int objectId)
        {
            var obj = ObjectRepository.GetObjectPropertiesById(objectId);
            if (obj == null)
            {
                throw new ApplicationException(string.Format(TemplateStrings.ObjectNotFound, objectId));
            }

            return obj.IsSiteDotNet;
        }

        public void CaptureLockTemplateObjectFormat(int id)
        {
            var format = ObjectFormatRepository.ReadObjectFormat(id, false);
            if (format == null)
            {
                throw new Exception(string.Format(TemplateStrings.FormatNotFound, id));
            }

            if (format.CanBeUnlocked)
            {
                EntityObjectRepository.CaptureLock(format);
            }
        }

        public void CaptureLockPageObjectFormat(int id)
        {
            var format = ObjectFormatRepository.ReadObjectFormat(id, false);
            if (format == null)
            {
                throw new Exception(string.Format(TemplateStrings.FormatNotFound, id));
            }

            if (format.CanBeUnlocked)
            {
                EntityObjectRepository.CaptureLock(format);
            }
        }

        public ListResult<ObjectFormatVersionListItem> GetTemplateObjectFormatVersionsByFormatId(ListCommand listCommand, int parentId) => GetObjectFormatVersionsByFormatId(listCommand, parentId, false);

        private static ListResult<ObjectFormatVersionListItem> GetObjectFormatVersionsByFormatId(ListCommand listCommand, int formatId, bool pageOrTemplate)
        {
            var list = PageTemplateRepository.ListFormatVersions(listCommand, formatId, out var totalRecords, pageOrTemplate);
            return new ListResult<ObjectFormatVersionListItem>
            {
                Data = list.ToList(),
                TotalRecords = totalRecords
            };
        }

        public ListResult<ObjectFormatVersionListItem> GetPageObjectFormatVersionsByFormatId(ListCommand listCommand, int parentId) => GetObjectFormatVersionsByFormatId(listCommand, parentId, true);

        public ObjectFormatVersion ReadTemplateObjectFormatVersionProperties(int id) => PageTemplateRepository.ReadFormatVersion(id);

        public ObjectFormatVersion ReadPageObjectFormatVersionProperties(int id) => PageTemplateRepository.ReadFormatVersion(id);

        public ObjectFormatVersion GetMergedObjectFormatVersion(int[] ids, int parentId, bool pageOrTemplate)
        {
            if (ids == null)
            {
                throw new ArgumentNullException(nameof(ids));
            }

            if (ids.Length != 2)
            {
                throw new ArgumentException("Wrong ids length");
            }

            var result = GetOrderedIds(ids);
            var version1 = PageTemplateRepository.ReadFormatVersion(result.Item1);
            if (version1 == null)
            {
                throw new Exception(string.Format(TemplateStrings.FormatVersionNotFoundForFormat, result.Item1, parentId));
            }

            ObjectFormatVersion version2;
            if (result.Item2 == ObjectFormatVersion.CurrentVersionId)
            {
                var parent = ObjectFormatRepository.ReadObjectFormat(parentId, pageOrTemplate);
                if (parent == null)
                {
                    throw new Exception(string.Format(TemplateStrings.FormatNotFound, parentId));
                }

                version2 = new ObjectFormatVersion
                {
                    Name = parent.Name,
                    NetFormatName = parent.NetFormatName,
                    Description = parent.Description,
                    NetLanguage = parent.NetLanguageId.HasValue ? PageTemplateRepository.GetNetLanguageById(parent.NetLanguageId.Value) : null,
                    FormatBody = parent.FormatBody,
                    CodeBehind = parent.CodeBehind,
                    LastModifiedByUser = parent.LastModifiedByUser,
                    Modified = parent.Modified
                };
            }
            else
            {
                version2 = PageTemplateRepository.ReadFormatVersion(result.Item2);
                if (version2 == null)
                {
                    throw new Exception(string.Format(TemplateStrings.FormatVersionNotFoundForFormat, result.Item2, parentId));
                }
            }

            version1.MergeToVersion(version2);
            return version1;
        }

        private static void Exchange(ref int id1, ref int id2)
        {
            var temp = id1;
            id1 = id2;
            id2 = temp;
        }

        private static Tuple<int, int> GetOrderedIds(IReadOnlyList<int> ids)
        {
            var id1 = ids[0];
            var id2 = ids[1];
            if (id1 > id2)
            {
                Exchange(ref id1, ref id2);
            }

            if (id1 == ArticleVersion.CurrentVersionId)
            {
                Exchange(ref id1, ref id2);
            }

            return Tuple.Create(id1, id2);
        }

        public MessageResult MultipleRemoveObjectFormatVersion(int[] ids)
        {
            if (ids == null)
            {
                throw new ArgumentNullException(nameof(ids));
            }

            PageTemplateRepository.MultipleDeleteObjectFormatVersion(ids);

            return null;
        }

        public MessageResult RestoreObjectFormatVersion(int versionId) => PageTemplateRepository.RestoreObjectFormatVersion(versionId);
    }
}
