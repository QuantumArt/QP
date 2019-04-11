using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Quantumart.QP8.BLL.Facades;
using Quantumart.QP8.BLL.Helpers;
using Quantumart.QP8.BLL.ListItems;
using Quantumart.QP8.BLL.Mappers;
using Quantumart.QP8.BLL.Repository.ContentRepositories;
using Quantumart.QP8.BLL.Services.DTO;
using Quantumart.QP8.DAL;
using Quantumart.QP8.Resources;
using Quantumart.QP8.Utils;

namespace Quantumart.QP8.BLL.Repository
{
    internal class PageTemplateRepository
    {
        internal static IEnumerable<PageTemplateListItem> ListTemplates(ListCommand cmd, int siteId, out int totalRecords)
        {
            using (var scope = new QPConnectionScope())
            {
                var rows = Common.GetPageTemplatesBySiteId(scope.DbConnection, siteId, cmd.SortExpression, out totalRecords, cmd.StartRecord, cmd.PageSize);
                return MapperFacade.PageTemplateRowMapper.GetBizList(rows.ToList());
            }
        }

        /// <summary>
        /// Возвращает список по ids
        /// </summary>
        /// <returns></returns>
        internal static IEnumerable<PageTemplate> GetPageTemplateList(IEnumerable<int> ids)
        {
            IEnumerable<decimal> decIDs = Converter.ToDecimalCollection(ids).Distinct().ToArray();
            return DefaultMapper.GetBizList<PageTemplate, PageTemplateDAL>(QPContext.EFContext.PageTemplateSet.Where(f => decIDs.Contains(f.Id)).ToList());
        }

        internal static PageTemplate SaveProperties(PageTemplate template) => DefaultRepository.Save<PageTemplate, PageTemplateDAL>(template);

        internal static IEnumerable<NetLanguage> GetNetLanguagesList()
        {
            var entities = QPContext.EFContext;
            return MapperFacade.NetLanguageMapper.GetBizList(entities.NetLanguagesSet.ToList());
        }

        internal static IEnumerable<Locale> GetLocalesList()
        {
            var entities = QPContext.EFContext;
            return MapperFacade.LocaleMapper.GetBizList(entities.LocaleSet.ToList());
        }

        internal static IEnumerable<Charset> GetCharsetsList()
        {
            var entities = QPContext.EFContext;
            return MapperFacade.CharsetMapper.GetBizList(entities.CharsetSet.ToList());
        }

        internal static PageTemplate GetPageTemplatePropertiesById(int id)
        {
            return MapperFacade.PageTemplateMappper.GetBizObject(QPContext.EFContext.PageTemplateSet.Include("Site").Include("LastModifiedByUser")
                .SingleOrDefault(g => g.Id == id)
            );
        }

        internal static PageTemplate UpdatePageTemplateProperties(PageTemplate pageTemplate) => DefaultRepository.Update<PageTemplate, PageTemplateDAL>(pageTemplate);

        internal static void DeletePageTemplate(int id)
        {
            DefaultRepository.Delete<PageTemplateDAL>(id);
        }

        internal static IEnumerable<BllObject> GetFreeTemplateObjects(int pageId)
        {
            var entities = QPContext.EFContext;
            var templateId = MapperFacade.PageMapper.GetBizObject(entities.PageSet.ToList().SingleOrDefault(x => x.Id == pageId)).TemplateId;
            return MapperFacade.ObjectMapper.GetBizList(entities.ObjectSet.Include("InheritedObjects").Where(x => x.PageId == null && x.PageTemplateId.Value == templateId && x.InheritedObjects.All(y => y.PageId != pageId)).ToList());
        }

        internal static IEnumerable<ObjectType> GetTypesList()
        {
            var entities = QPContext.EFContext;
            return MapperFacade.ObjectTypeMapper.GetBizList(entities.ObjectTypeSet.ToList());
        }

        internal static void DeleteObjectFormat(int id)
        {
            DefaultRepository.Delete<ObjectFormatDAL>(id);
        }

        internal static IEnumerable<ObjectFormatListItem> ListObjectFormats(ListCommand cmd, int parentId, out int totalRecords, bool pageOrTemplate)
        {
            using (var scope = new QPConnectionScope())
            {
                var rows = Common.GetObjectFormatsByObjectId(scope.DbConnection, parentId, cmd.SortExpression, out totalRecords, cmd.StartRecord, cmd.PageSize, pageOrTemplate);
                return MapperFacade.ObjectFormatRowMapper.GetBizList(rows.ToList());
            }
        }

        internal static IEnumerable<EntityPermissionLevel> GetPermissionLevels()
        {
            var entities = QPContext.EFContext;
            return MapperFacade.EntityPermissionLevelMapper.GetBizList(entities.PermissionLevelSet.ToList());
        }

        internal static EntityPermissionLevel GetPermissionLevelByName(string name)
        {
            var entities = QPContext.EFContext;
            return MapperFacade.EntityPermissionLevelMapper.GetBizObject(entities.PermissionLevelSet.Single(x => x.Name == name));
        }

        internal static Charset GetCharsetByName(string name)
        {
            var entities = QPContext.EFContext;
            return MapperFacade.CharsetMapper.GetBizObject(entities.CharsetSet.SingleOrDefault(x => x.Subj == name));
        }

        internal static Locale GetLocaleByName(string name)
        {
            var entities = QPContext.EFContext;
            return MapperFacade.LocaleMapper.GetBizObject(entities.LocaleSet.SingleOrDefault(x => x.Name == name));
        }

        internal static void MultipleDeletePage(int[] ids)
        {
            if (ids.Length > 0)
            {
                DefaultRepository.Delete<PageDAL>(ids);
            }
        }

        internal static void MultipleDeleteObject(int[] ids)
        {
            if (ids.Length > 0)
            {
                DefaultRepository.Delete<ObjectDAL>(ids);
            }
        }

        internal static ContentForm GetContentFormByObjectId(int objectId)
        {
            var entities = QPContext.EFContext;
            return MapperFacade.ContentFormMapper.GetBizObject(entities.ContentFormSet.Include("Content").Include("Page").SingleOrDefault(x => x.ObjectId == objectId));
        }

        internal static Container GetContainerByObjectId(int objectId)
        {
            var entities = QPContext.EFContext;
            return MapperFacade.ContainerMapper.GetBizObject(entities.ContainerSet.Include("Content").SingleOrDefault(x => x.ObjectId == objectId));
        }

        internal static bool PageTemplateNetNameUnique(string netName, int parentId, int templateId)
        {
            var entities = QPContext.EFContext;
            return MapperFacade.PageTemplateMappper.GetBizObject(entities.PageTemplateSet.SingleOrDefault(x => x.SiteId == parentId && x.NetTemplateName == netName && x.Id != templateId)) == null;
        }

        internal static IEnumerable<ListItem> GetPageSimpleList(int[] selectedEntitiesIDs)
        {
            if (selectedEntitiesIDs == null)
            {
                return Enumerable.Empty<ListItem>();
            }

            var decPageIDs = Converter.ToDecimalCollection(selectedEntitiesIDs);
            return QPContext.EFContext.PageSet
                .Where(n => decPageIDs.Contains(n.Id))
                .Select(g => new { g.Id, g.Name })
                .ToArray()
                .Select(g => new ListItem { Value = g.Id.ToString(), Text = g.Name });
        }

        internal static bool ObjectFormatNetNameUnique(string netFormatName, int objectId, int id)
        {
            var entities = QPContext.EFContext;
            return MapperFacade.ObjectFormatMapper.GetBizObject(entities.ObjectFormatSet.SingleOrDefault(x => x.ObjectId == objectId && x.NetFormatName == netFormatName && x.Id != id)) == null;
        }

        internal static IEnumerable<StatusType> GetActiveStatusesByObjectId(int objectId)
        {
            using (var scope = new QPConnectionScope())
            {
                IEnumerable<DataRow> rows = Common.GetActiveStatusesByObjectId(scope.DbConnection, objectId);
                return MapperFacade.StatusTypeRowMapper.GetBizList(rows.ToList());
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
            var entities = QPContext.EFContext;
            return MapperFacade.PageMapper.GetBizObject(entities.PageSet.SingleOrDefault(x => x.PageTemplate.Id == parentId && x.Filename == fileName && x.Id != pageId)) == null;
        }

        internal static NetLanguage GetNetLanguageByName(string name)
        {
            var entities = QPContext.EFContext;
            return DefaultMapper.GetBizObject<NetLanguage, NetLanguagesDAL>(entities.NetLanguagesSet.SingleOrDefault(x => x.Name == name));
        }

        internal static NetLanguage GetNetLanguageById(int id)
        {
            var entities = QPContext.EFContext;
            return DefaultMapper.GetBizObject<NetLanguage, NetLanguagesDAL>(entities.NetLanguagesSet.SingleOrDefault(x => x.Id == id));
        }

        internal static IEnumerable<int> GetTemplatePagesId(int templateId)
        {
            var entities = QPContext.EFContext;
            return MapperFacade.PageMapper.GetBizList(entities.PageSet.Where(x => x.TemplateId == templateId).ToList()).Select(y => y.Id);
        }

        internal static IEnumerable<int> GetFormatIdsByTemplateId(int templateId)
        {
            using (var scope = new QPConnectionScope())
            {
                var rows = Common.GetFormatIdsByTemplateId(scope.DbConnection, templateId);
                return rows.Select(x => Converter.ToInt32(x.Field<decimal>("OBJECT_FORMAT_ID")));
            }
        }

        internal static void ManagePageAndObjectModified(ObjectFormat format)
        {
            var parentObj = ObjectRepository.GetObjectPropertiesById(format.ObjectId);
            using (var scope = new QPConnectionScope())
            {
                if (parentObj.PageId.HasValue)
                {
                    Common.UpdatePageAndObjectDateModifiedByObjectId(format.ObjectId, parentObj.PageId.Value, scope.DbConnection);
                }
                else
                {
                    Common.UpdateObjectDateModified(format.ObjectId, scope.DbConnection);
                }
            }
        }

        internal static IEnumerable<BackendActionType> GetActionTypeList()
        {
            return BackendActionTypeRepository.GetList().Where(r => r.RequiredPermissionLevel >= 3).ToArray();
        }

        internal static IEnumerable<PageTemplate> GetSiteTemplates(int siteId)
        {
            var entities = QPContext.EFContext;
            return MapperFacade.PageTemplateMappper.GetBizList(entities.PageTemplateSet.Include("Site").Where(x => x.SiteId == siteId).ToList());
        }

        internal static IEnumerable<ObjectFormatSearchResultListItem> GetSearchFormatPage(ListCommand listCommand, int siteId, int? templateId, int? pageId, string filter, out int totalRecords)
        {
            using (var scope = new QPConnectionScope())
            {
                var rows = Common.GetSearchFormatPage(scope.DbConnection, listCommand.SortExpression, siteId, templateId, pageId, filter, out totalRecords, listCommand.StartPage, listCommand.PageSize);
                var result = MapperFacade.ObjectFormatSearchResultRowMapper.GetBizList(rows.ToList());
                return result;
            }
        }

        internal static IEnumerable<PageTemplateSearchListItem> GetSearchTemplatePage(ListCommand listCommand, int siteId, string filter, out int totalRecords)
        {
            using (var scope = new QPConnectionScope())
            {
                var rows = Common.GetSearchTemplatePage(scope.DbConnection, listCommand.SortExpression, siteId, filter, out totalRecords, listCommand.StartRecord, listCommand.PageSize);
                var result = MapperFacade.PageTemplateSearchResultRowMapper.GetBizList(rows.ToList());
                return result;
            }
        }

        internal static IEnumerable<ObjectSearchListItem> GetSearchObjectPage(ListCommand listCommand, int siteId, int? templateId, int? pageId, string filter, out int totalRecords)
        {
            using (var scope = new QPConnectionScope())
            {
                var rows = Common.GetSearchObjectPage(scope.DbConnection, listCommand.SortExpression, siteId, templateId, pageId, filter, out totalRecords, listCommand.StartRecord, listCommand.PageSize);
                var result = MapperFacade.ObjectSearchResultRowMapper.GetBizList(rows.ToList());
                return result;
            }
        }

        internal static IEnumerable<ObjectFormatVersionListItem> ListFormatVersions(ListCommand cmd, int formatId, out int totalRecords, bool pageOrTemplate)
        {
            using (var scope = new QPConnectionScope())
            {
                var rows = Common.GetFormatVersionsByFormatId(scope.DbConnection, formatId, cmd.SortExpression, out totalRecords, cmd.StartRecord, cmd.PageSize, pageOrTemplate);
                return MapperFacade.ObjectFormatVersionRowMapper.GetBizList(rows.ToList());
            }
        }

        internal static ObjectFormatVersion ReadFormatVersion(int id)
        {
            var entities = QPContext.EFContext;
            return MapperFacade.ObjectFormatVersionMapper.GetBizObject(entities.ObjectFormatVersionSet.Include("NetLanguage").Include("ObjectFormat")
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

        internal static void MultipleDeleteObjectFormatVersion(int[] ids)
        {
            if (ids.Length > 0)
            {
                DefaultRepository.Delete<ObjectFormatVersionDAL>(ids);
            }
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
                var rows = Common.GetRelationsBetweenTemplates(scope.DbConnection, sourceSiteId, destinationSiteId, templateIdNew);
                return MultistepActionHelper.GetXmlFromDataRows(rows, "template");
            }
        }
    }
}
