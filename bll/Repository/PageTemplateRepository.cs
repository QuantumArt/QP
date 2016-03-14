using Quantumart.QP8.BLL.Helpers;
using Quantumart.QP8.BLL.ListItems;
using Quantumart.QP8.BLL.Mappers;
using Quantumart.QP8.DAL;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using Quantumart.QP8.Utils;
using Quantumart.QP8.BLL.Services.DTO;
using Quantumart.QP8.Resources;
using System;



namespace Quantumart.QP8.BLL.Repository
{
    class PageTemplateRepository
    {
        internal static IEnumerable<PageTemplateListItem> ListTemplates(ListCommand cmd, int siteId, out int totalRecords)
        {
            using (var scope = new QPConnectionScope())
            {
                IEnumerable<DataRow> rows = Common.GetPageTemplatesBySiteId(scope.DbConnection, siteId, cmd.SortExpression, out totalRecords, cmd.StartRecord, cmd.PageSize);
                return MappersRepository.PageTemplateRowMapper.GetBizList(rows.ToList());
            }
        }

		/// <summary>
		/// Возвращает список по ids
		/// </summary>
		/// <returns></returns>
		internal static IEnumerable<PageTemplate> GetPageTemplateList(IEnumerable<int> IDs)
		{
			IEnumerable<decimal> decIDs = Converter.ToDecimalCollection(IDs).Distinct().ToArray();
			return DefaultMapper
				.GetBizList <PageTemplate, PageTemplateDAL>(QPContext.EFContext.PageTemplateSet
					.Where(f => decIDs.Contains(f.Id))
					.ToList()
				);
		}

        internal static PageTemplate SaveProperties(PageTemplate template)
        {
            return DefaultRepository.Save<PageTemplate, PageTemplateDAL>(template);
        }

        internal static IEnumerable<NetLanguage> GetNetLanguagesList()
        {
            QP8Entities entities = QPContext.EFContext;
            return MappersRepository.NetLanguageMapper.GetBizList(entities.NetLanguagesSet.ToList());
        }

        internal static IEnumerable<Locale> GetLocalesList()
        {
            QP8Entities entities = QPContext.EFContext;
            return MappersRepository.LocaleMapper.GetBizList(entities.LocaleSet.ToList());
        }

        internal static IEnumerable<Charset> GetCharsetsList()
        {
            QP8Entities entities = QPContext.EFContext;
            return MappersRepository.CharsetMapper.GetBizList(entities.CharsetSet.ToList());
        }

        internal static PageTemplate GetPageTemplatePropertiesById(int id)
        {
			return MappersRepository.PageTemplateMappper.GetBizObject(QPContext.EFContext.PageTemplateSet.Include("Site").Include("LastModifiedByUser")
                .SingleOrDefault(g => g.Id == id)
            );
        }

        internal static PageTemplate UpdatePageTemplateProperties(PageTemplate pageTemplate)
        {
            return DefaultRepository.Update<PageTemplate, PageTemplateDAL>(pageTemplate);
        }

        internal static void DeletePageTemplate(int id)
        {
            DefaultRepository.Delete<PageTemplateDAL>(id);
        }

		internal static IEnumerable<BllObject> GetFreeTemplateObjects(int pageId)
		{
			QP8Entities entities = QPContext.EFContext;
			var templateId = MappersRepository.PageMapper.GetBizObject(entities.PageSet.ToList().SingleOrDefault(x => x.Id == pageId)).TemplateId;
			return MappersRepository.ObjectMapper.GetBizList(entities.ObjectSet.Include("InheritedObjects")
				.Where(x => x.PageId == null && x.PageTemplateId.Value == templateId && (!x.InheritedObjects.Where(y => y.PageId == pageId).Any()) ).ToList());
		}

		internal static IEnumerable<ObjectType> GetTypesList()
		{
			QP8Entities entities = QPContext.EFContext;
			return MappersRepository.ObjectTypeMapper.GetBizList(entities.ObjectTypeSet.ToList());
		}



		internal static void DeleteObjectFormat(int id)
		{
			DefaultRepository.Delete<ObjectFormatDAL>(id);
		}

		internal static IEnumerable<ObjectFormatListItem> ListObjectFormats(ListCommand cmd, int parentId, out int totalRecords, bool pageOrTemplate)
		{
			using (var scope = new QPConnectionScope())
			{
				IEnumerable<DataRow> rows = Common.GetObjectFormatsByObjectId(scope.DbConnection, parentId, cmd.SortExpression, out totalRecords, cmd.StartRecord, cmd.PageSize, pageOrTemplate);
				return MappersRepository.ObjectFormatRowMapper.GetBizList(rows.ToList());
			}
		}

		internal static IEnumerable<EntityPermissionLevel> GetPermissionLevels()
		{
			QP8Entities entities = QPContext.EFContext;
			return MappersRepository.EntityPermissionLevelMapper.GetBizList(entities.PermissionLevelSet.ToList());
		}

		internal static EntityPermissionLevel GetPermissionLevelByName(string name)
		{
			QP8Entities entities = QPContext.EFContext;
			return MappersRepository.EntityPermissionLevelMapper.GetBizObject(entities.PermissionLevelSet.Single(x => x.Name == name));
		}

		internal static Charset GetCharsetByName(string name)
		{
			QP8Entities entities = QPContext.EFContext;
			return MappersRepository.CharsetMapper.GetBizObject(entities.CharsetSet.SingleOrDefault(x => x.Subj == name));
		}

		internal static Locale GetLocaleByName(string name)
		{
			QP8Entities entities = QPContext.EFContext;
			return MappersRepository.LocaleMapper.GetBizObject(entities.LocaleSet.SingleOrDefault(x => x.Name == name));
		}

		internal static void MultipleDeletePage(int[] IDs)
		{
			if (IDs.Length > 0)
				DefaultRepository.Delete<PageDAL>(IDs);
		}

		internal static void MultipleDeleteObject(int[] IDs)
		{
			if (IDs.Length > 0)
				DefaultRepository.Delete<ObjectDAL>(IDs);
		}

		internal static ContentForm GetContentFormByObjectId(int objectId)
		{
			QP8Entities entities = QPContext.EFContext;
			return MappersRepository.ContentFormMapper.GetBizObject(entities.ContentFormSet.Include("Content").Include("Page").SingleOrDefault(x => x.ObjectId == objectId));
		}

		internal static Container GetContainerByObjectId(int objectId)
		{
			QP8Entities entities = QPContext.EFContext;
			return MappersRepository.ContainerMapper.GetBizObject(entities.ContainerSet.Include("Content").SingleOrDefault(x => x.ObjectId == objectId));
		}

		internal static bool PageTemplateNetNameUnique(string netName, int parentId, int templateId)
		{
			QP8Entities entities = QPContext.EFContext;
			return MappersRepository.PageTemplateMappper.GetBizObject(entities.PageTemplateSet.SingleOrDefault(x => x.SiteId == parentId && x.NetTemplateName == netName && x.Id != templateId)) == null;
		}

		internal static IEnumerable<ListItem> GetPageSimpleList(int[] selectedEntitiesIDs)
		{
			if (selectedEntitiesIDs == null)
				return Enumerable.Empty<ListItem>();
			IEnumerable<decimal> decPageIDs = Converter.ToDecimalCollection(selectedEntitiesIDs);
			return QPContext.EFContext.PageSet
				.Where(n => decPageIDs.Contains(n.Id))
				.Select(g => new { Id = g.Id, Name = g.Name })
				.ToArray()
				.Select(g => new ListItem { Value = g.Id.ToString(), Text = g.Name })
				;
		}

		internal static bool ObjectFormatNetNameUnique(string NetFormatName, int ObjectId, int Id)
		{
			QP8Entities entities = QPContext.EFContext;
			return MappersRepository.ObjectFormatMapper.GetBizObject(entities.ObjectFormatSet.SingleOrDefault(x => x.ObjectId == ObjectId && x.NetFormatName == NetFormatName && x.Id != Id)) == null;
		}

		internal static IEnumerable<StatusType> GetActiveStatusesByObjectId(int objectId)
		{
			using (var scope = new QPConnectionScope())
			{
				IEnumerable<DataRow> rows = Common.GetActiveStatusesByObjectId(scope.DbConnection, objectId);
				return MappersRepository.StatusTypeRowMapper.GetBizList(rows.ToList());
			}
		}

		internal static IEnumerable<int> GetStatusIdsByContentId(int contentId, out bool hasWorkflow)
		{
			var result = new List<int>();
			var content = ContentRepository.GetById(contentId);
			if (content.WorkflowBinding == null || content.WorkflowBinding.WorkflowId == 0)
			{
				result.Add(StatusTypeRepository.GetPublishedStatusIdBySiteId(content.SiteId));
				hasWorkflow = false;
			}
			else
			{
				result.AddRange(StatusTypeRepository.GetAllForWorkflow(content.WorkflowBinding.WorkflowId).Select(x => x.Id));
				hasWorkflow = true;
			}
			return result;
		}

		internal static bool PageFileNameUnique(string fileName, int parentId, int pageId)
		{
			QP8Entities entities = QPContext.EFContext;
			return MappersRepository.PageMapper.GetBizObject(entities.PageSet.SingleOrDefault(x => x.PageTemplate.Id == parentId && x.Filename == fileName && x.Id != pageId)) == null;
		}

		internal static NetLanguage GetNetLanguageByName(string name)
		{
			QP8Entities entities = QPContext.EFContext;
			return DefaultMapper.GetBizObject<NetLanguage, NetLanguagesDAL>(entities.NetLanguagesSet.SingleOrDefault(x => x.Name == name));
		}

		internal static NetLanguage GetNetLanguageById(int id)
		{
			QP8Entities entities = QPContext.EFContext;
			return DefaultMapper.GetBizObject<NetLanguage, NetLanguagesDAL>(entities.NetLanguagesSet.SingleOrDefault(x => x.Id == id));
		}

		internal static IEnumerable<int> GetTemplatePagesId(int templateId)
		{
			QP8Entities entities = QPContext.EFContext;
			return MappersRepository.PageMapper.GetBizList(entities.PageSet.Where(x => x.TemplateId == templateId).ToList()).Select(y => y.Id);
		}

		internal static IEnumerable<int> GetFormatIdsByTemplateId(int TemplateId)
		{
			using (var scope = new QPConnectionScope())
			{
				IEnumerable<DataRow> rows = Common.GetFormatIdsByTemplateId(scope.DbConnection, TemplateId);
				return rows.Select(x => Converter.ToInt32(x.Field<decimal>("OBJECT_FORMAT_ID")));
			}
		}

		internal static void ManagePageAndObjectModified(ObjectFormat format)
		{
			var parentObj = ObjectRepository.GetObjectPropertiesById(format.ObjectId);
			using (var scope = new QPConnectionScope())
			{
				if (parentObj.PageId.HasValue)
					Common.UpdatePageAndObjectDateModifiedByObjectId(format.ObjectId, parentObj.PageId.Value, scope.DbConnection);
				else
					Common.UpdateObjectDateModified(format.ObjectId, scope.DbConnection);
			}
		}


		internal static IEnumerable<BackendActionType> GetActionTypeList()
		{
			return BackendActionTypeRepository.GetList().Where(r => r.RequiredPermissionLevel >= 3).ToArray();
		}

		internal static IEnumerable<PageTemplate> GetSiteTemplates(int siteId)
		{
			QP8Entities entities = QPContext.EFContext;
			return MappersRepository.PageTemplateMappper.GetBizList(entities.PageTemplateSet.Include("Site").Where(x => x.SiteId == siteId).ToList());
		}

		internal static IEnumerable<ObjectFormatSearchResultListItem> GetSearchFormatPage(ListCommand listCommand, int siteId, int? templateId, int? pageId, string filter, out int totalRecords)
		{
			using (var scope = new QPConnectionScope())
			{
				IEnumerable<DataRow> rows = Common.GetSearchFormatPage(scope.DbConnection, listCommand.SortExpression, siteId, templateId, pageId, filter, out totalRecords, listCommand.StartPage, listCommand.PageSize);
				var result = MappersRepository.ObjectFormatSearchResultRowMapper.GetBizList(rows.ToList());
				return result;
			}
		}



		internal static IEnumerable<PageTemplateSearchListItem> GetSearchTemplatePage(ListCommand listCommand, int siteId, string filter, out int totalRecords)
		{
			using (var scope = new QPConnectionScope())
			{
				IEnumerable<DataRow> rows = Common.GetSearchTemplatePage(scope.DbConnection, listCommand.SortExpression, siteId, filter, out totalRecords, listCommand.StartRecord, listCommand.PageSize);
				var result = MappersRepository.PageTemplateSearchResultRowMapper.GetBizList(rows.ToList());
				return result;
			}
		}

		internal static IEnumerable<ObjectSearchListItem> GetSearchObjectPage(ListCommand listCommand, int siteId, int? templateId, int? pageId, string filter, out int totalRecords)
		{
			using (var scope = new QPConnectionScope())
			{
				IEnumerable<DataRow> rows = Common.GetSearchObjectPage(scope.DbConnection, listCommand.SortExpression, siteId, templateId, pageId, filter, out totalRecords, listCommand.StartRecord, listCommand.PageSize);
				var result = MappersRepository.ObjectSearchResultRowMapper.GetBizList(rows.ToList());
				return result;
			}
		}

		internal static IEnumerable<ObjectFormatVersionListItem> ListFormatVersions(ListCommand cmd, int formatId, out int totalRecords, bool pageOrTemplate)
		{
			using (var scope = new QPConnectionScope())
			{
				IEnumerable<DataRow> rows = Common.GetFormatVersionsByFormatId(scope.DbConnection, formatId, cmd.SortExpression, out totalRecords, cmd.StartRecord, cmd.PageSize, pageOrTemplate);
				return MappersRepository.ObjectFormatVersionRowMapper.GetBizList(rows.ToList());
			}
		}

		internal static ObjectFormatVersion ReadFormatVersion(int id)
		{
			QP8Entities entities = QPContext.EFContext;
			return MappersRepository.ObjectFormatVersionMapper.GetBizObject(entities.ObjectFormatVersionSet.Include("NetLanguage").Include("ObjectFormat")
				.Include("LastModifiedByUser").SingleOrDefault(x => x.Id == id));
		}

		internal static MessageResult RestoreObjectFormatVersion(int versionId)
		{
			try
			{
				using (var scope = new QPConnectionScope())
				{
					Common.RestoreObjectFormatVersion(scope.DbConnection, versionId);
				}
				return MessageResult.Info(string.Format(TemplateStrings.VersionRestored, versionId));
			}
			catch (Exception)
			{
				return MessageResult.Info(string.Format(TemplateStrings.VersionRestoreError, versionId));
			}
		}

		internal static void DeleteObjectFormatVersion(int id)
		{
			DefaultRepository.Delete<ObjectFormatVersionDAL>(id);
		}

		internal static void MultipleDeleteObjectFormatVersion(int[] IDs)
		{
			if (IDs.Length > 0)
				DefaultRepository.Delete<ObjectFormatVersionDAL>(IDs);
		}

        internal static int CopySiteTemplates(int sourceSiteId, int destinationSiteId, int templateNumber)
        {
            using (var scope = new QPConnectionScope())
            {
                return Common.CopySiteTemplates(scope.DbConnection, sourceSiteId, destinationSiteId, templateNumber);
            }
        }

        internal static string GetRelationsBetweenTemplates(int sourceSiteId, int destinationSiteId, int templateIdNew)
        {
            using (var scope = new QPConnectionScope())
            {
                IEnumerable<DataRow> rows = Common.GetRelationsBetweenTemplates(scope.DbConnection, sourceSiteId, destinationSiteId, templateIdNew);
                return MultistepActionHelper.GetXmlFromDataRows(rows, "template");
            }
        }
	}
}
