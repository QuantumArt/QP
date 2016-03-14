using Quantumart.QP8.BLL.ListItems;
using Quantumart.QP8.BLL.Repository;
using Quantumart.QP8.BLL.Services.DTO;
using Quantumart.QP8.Constants;
using Quantumart.QP8.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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

		ObjectFormatVersion GetMergedObjectFormatVersion(int[] IDs, int parentId, bool pageOrtemplate);

		MessageResult MultipleRemoveObjectFormatVersion(int[] IDs);

		MessageResult RestoreObjectFormatVersion(int versionId);
	}

	public class FormatService : IFormatService
	{
		public FormatInitListResult InitFormatList(int parentId, bool isTemplateFormat)
		{
			return new FormatInitListResult
			{
				IsAddNewAccessable = isTemplateFormat ?
				SecurityRepository.IsActionAccessible(ActionCode.AddNewTemplateObjectFormat) && SecurityRepository.IsEntityAccessible(EntityTypeCode.TemplateObjectFormat, parentId, ActionTypeCode.Update) :
				SecurityRepository.IsActionAccessible(ActionCode.AddNewPageObjectFormat) && SecurityRepository.IsEntityAccessible(EntityTypeCode.PageObjectFormat, parentId, ActionTypeCode.Update)
			};
		}

		public FormatVersionInitListResult InitFormatVersionList()
		{
			return new FormatVersionInitListResult
			{
				IsAddNewAccessable = false
			};
		}

		public IEnumerable<ListItem> GetNetLanguagesAsListItems()
		{
			return PageTemplateRepository.GetNetLanguagesList().Select(lang => new ListItem { Text = lang.Name, Value = lang.Id.ToString() })
					.ToArray();
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

		public MessageResult RemoveObjectFormat(int id, bool pageOrTemplate)
		{
			ObjectFormat format = ObjectFormatRepository.ReadObjectFormat(id, pageOrTemplate);
			if (format == null)
				throw new ApplicationException(String.Format(TemplateStrings.FormatNotFound, id));
			if (format.LockedByAnyoneElse)
				return MessageResult.Error(String.Format(TemplateStrings.LockedByAnyoneElse, format.LockedByDisplayName));
			if (format.Notifications.Count() > 0)
				return MessageResult.Error(TemplateStrings.UnableToDeleteFormat);
			ManagePageAndObjectModified(format);
			PageTemplateRepository.DeleteObjectFormat(id);
			return null;
		}

		private void ManagePageAndObjectModified(ObjectFormat format)
		{
			PageTemplateRepository.ManagePageAndObjectModified(format);
		}

		public void CancelFormat(int id, bool pageOrTemplate)
		{
			ObjectFormat format = ObjectFormatRepository.ReadObjectFormat(id, pageOrTemplate);
			if (format == null)
				throw new ApplicationException(String.Format(TemplateStrings.FormatNotFound, id));
			format.AutoUnlock();
		}

		public ObjectFormat NewPageObjectFormatProperties(int parentId, bool isSiteDotNet)
		{
			return ObjectFormat.Create(parentId, true, isSiteDotNet);
		}

		public ObjectFormat ReadPageObjectFormatPropertiesForUpdate(int parentId, bool isSiteDotNet)
		{
			var result = ReadFormatProperties(parentId, true, false);
			result.IsSiteDotNet = isSiteDotNet;
			return result;
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

		public ObjectFormat SaveObjectFormatProperties(ObjectFormat objectFormat)
		{
			objectFormat.ReplaceUrlsToPlaceHolders();
			var format = FormatRepository.SaveObjectFormatProperties(objectFormat);
			ManagePageAndObjectModified(format);
			return format;
		}

		public ObjectFormat NewPageObjectFormatPropertiesForUpdate(int parentId, bool isSiteDotNet)
		{
			return NewPageObjectFormatProperties(parentId, isSiteDotNet);
		}

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
		
		public ObjectFormat NewTemplateObjectFormatProperties(int parentId, bool isSiteDotNet)
		{
			return ObjectFormat.Create(parentId, false, isSiteDotNet);
		}

		public ObjectFormat NewTemplateObjectFormatPropertiesForUpdate(int parentId, bool isSiteDotNet)
		{
			return NewTemplateObjectFormatProperties(parentId, isSiteDotNet);
		}

		public ObjectFormat ReadTemplateObjectFormatPropertiesForUpdate(int parentId, bool isSiteDotNet)
		{
			var result = ReadFormatProperties(parentId, false, false);
			result.IsSiteDotNet = isSiteDotNet;
			return result;
		}

		public ListResult<ObjectFormatListItem> GetPageObjectFormatsByObjectId(ListCommand listCommand, int parentId)
		{
			return GetObjectFormatsByObjectId(listCommand, parentId, true);
		}

		private ListResult<ObjectFormatListItem> GetObjectFormatsByObjectId(ListCommand listCommand, int objectId, bool pageOrTemplate)
		{
			int totalRecords;
			IEnumerable<ObjectFormatListItem> list = PageTemplateRepository.ListObjectFormats(listCommand, objectId, out totalRecords, pageOrTemplate);
			return new ListResult<ObjectFormatListItem>
			{
				Data = list.ToList(),
				TotalRecords = totalRecords
			};
		}

		public ListResult<ObjectFormatListItem> GetTemplateObjectFormatsByObjectId(ListCommand listCommand, int parentId)
		{
			return GetObjectFormatsByObjectId(listCommand, parentId, false);
		}

		public bool IsSiteDotNetByObjectId(int objectId)
		{
			var obj = ObjectRepository.GetObjectPropertiesById(objectId);
			if (obj == null)
				throw new ApplicationException(String.Format(TemplateStrings.ObjectNotFound, objectId));
			return obj.IsSiteDotNet;
		}

		public void CaptureLockTemplateObjectFormat(int id)
		{
			var format = ObjectFormatRepository.ReadObjectFormat(id, false);
			if (format == null)
				throw new Exception(String.Format(TemplateStrings.FormatNotFound, id));
			if (format.CanBeUnlocked)
			{
				EntityObjectRepository.CaptureLock(format);
			}
		}

		public void CaptureLockPageObjectFormat(int id)
		{
			var format = ObjectFormatRepository.ReadObjectFormat(id, false);
			if (format == null)
				throw new Exception(String.Format(TemplateStrings.FormatNotFound, id));
			if (format.CanBeUnlocked)
			{
				EntityObjectRepository.CaptureLock(format);
			}
		}

		public ListResult<ObjectFormatVersionListItem> GetTemplateObjectFormatVersionsByFormatId(ListCommand listCommand, int parentId)
		{
			return GetObjectFormatVersionsByFormatId(listCommand, parentId, false);
		}

		private ListResult<ObjectFormatVersionListItem> GetObjectFormatVersionsByFormatId(ListCommand listCommand, int formatId, bool pageOrTemplate)
		{
			int totalRecords;
			IEnumerable<ObjectFormatVersionListItem> list = PageTemplateRepository.ListFormatVersions(listCommand, formatId, out totalRecords, pageOrTemplate);
			return new ListResult<ObjectFormatVersionListItem>
			{
				Data = list.ToList(),
				TotalRecords = totalRecords
			};
		}

		public ListResult<ObjectFormatVersionListItem> GetPageObjectFormatVersionsByFormatId(ListCommand listCommand, int parentId)
		{
			return GetObjectFormatVersionsByFormatId(listCommand, parentId, true);
		}

		public ObjectFormatVersion ReadTemplateObjectFormatVersionProperties(int id)
		{
			return PageTemplateRepository.ReadFormatVersion(id);
		}

		public ObjectFormatVersion ReadPageObjectFormatVersionProperties(int id)
		{
			return PageTemplateRepository.ReadFormatVersion(id);
		}

		public ObjectFormatVersion GetMergedObjectFormatVersion(int[] ids, int parentId, bool pageOrTemplate)
		{
			if (ids == null)
				throw new ArgumentNullException("ids");
			if (ids.Length != 2)
				throw new ArgumentException("Wrong ids length");

			Tuple<int, int> result = GetOrderedIds(ids);
			ObjectFormatVersion version1 = PageTemplateRepository.ReadFormatVersion(result.Item1);
			if (version1 == null)
				throw new Exception(String.Format(TemplateStrings.FormatVersionNotFoundForFormat, result.Item1, parentId));

			ObjectFormatVersion version2;
			if (result.Item2 == ObjectFormatVersion.CurrentVersionId)
			{
				ObjectFormat parent = ObjectFormatRepository.ReadObjectFormat(parentId, pageOrTemplate);
				if (parent == null)
					throw new Exception(String.Format(TemplateStrings.FormatNotFound, parentId));
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
					throw new Exception(String.Format(TemplateStrings.FormatVersionNotFoundForFormat, result.Item2, parentId));
			}
			version1.MergeToVersion(version2);
			return version1;
		}

		private static void Exchange(ref int id1, ref int id2)
		{
			int temp = id1;
			id1 = id2;
			id2 = temp;
		}

		private static Tuple<int, int> GetOrderedIds(int[] ids)
		{
			int id1 = ids[0];
			int id2 = ids[1];

			if (id1 > id2)
				Exchange(ref id1, ref id2);

			if (id1 == ArticleVersion.CurrentVersionId)
				Exchange(ref id1, ref id2);

			return Tuple.Create<int, int>(id1, id2);
		}

		public MessageResult MultipleRemoveObjectFormatVersion(int[] IDs)
		{
			if (IDs == null)
				throw new ArgumentNullException("IDs");

			PageTemplateRepository.MultipleDeleteObjectFormatVersion(IDs);

			return null;
		}

		public MessageResult RestoreObjectFormatVersion(int versionId)
		{
			return PageTemplateRepository.RestoreObjectFormatVersion(versionId);
		}
	}
}
