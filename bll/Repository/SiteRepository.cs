using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.XPath;
using Microsoft.EntityFrameworkCore;
using QP8.Plugins.Contract;
using Quantumart.QP8.BLL.Facades;
using Quantumart.QP8.BLL.Helpers;
using Quantumart.QP8.BLL.ListItems;
using Quantumart.QP8.Constants;
using Quantumart.QP8.DAL;
using Quantumart.QP8.DAL.DTO;
using Quantumart.QP8.DAL.Entities;
using Quantumart.QP8.Utils;

namespace Quantumart.QP8.BLL.Repository
{
    internal class SiteRepository
    {
        /// <summary>
        /// Возвращает информацию о сайте по его идентификатору
        /// </summary>
        /// <param name="id">идентификатор сайта</param>
        /// <returns>информация о сайте</returns>
        internal static Site GetById(int id)
        {
            var result = GetByIdFromCache(id);
            if (result != null)
            {
                return result;
            }

            return MapperFacade.SiteMapper.GetBizObject(QPContext.EFContext.SiteSet.Include("LastModifiedByUser").Include("LockedByUser").SingleOrDefault(n => n.Id == id));
        }

        internal static Site GetByTemplateId(int templateId)
        {
            var siteId = QPContext.EFContext.PageTemplateSet
                .Where(t => t.Id == templateId)
                .Select(t => (int)t.SiteId)
                .FirstOrDefault();

            return GetById(siteId);
        }

        private static Site GetByIdFromCache(int id)
        {
            Site result = null;
            var cache = QPContext.GetSiteCache();
            if (cache != null && cache.ContainsKey(id))
            {
                result = cache[id];
            }

            return result;
        }

        /// <summary>
        /// Возвращает список сайтов
        /// </summary>
        internal static ListResult<SiteListItem> GetList(ListCommand cmd, IEnumerable<int> selectedIDs)
        {
            var options = new SitePageOptions
            {
                SelectedIDs = selectedIDs,
                SortExpression = cmd.SortExpression,
                StartRecord = cmd.StartRecord,
                PageSize = cmd.PageSize,
                UserId = QPContext.CurrentUserId,
                UseSecurity = !QPContext.IsAdmin
            };

            using (var scope = new QPConnectionScope())
            {
                var rows = Common.GetSitesPage(scope.DbConnection, options, out var totalRecords);
                return new ListResult<SiteListItem> { Data = MapperFacade.SiteListItemRowMapper.GetBizList(rows.ToList()), TotalRecords = totalRecords };
            }
        }

        internal static IEnumerable<Site> GetAll()
        {
            return MapperFacade.SiteMapper.GetBizList(QPContext.EFContext.SiteSet.OrderBy(ss => ss.Name).ToList());
        }

        private static void ChangeInsertAccessTriggerState(bool enable)
        {
            Common.ChangeTriggerState(QPContext.CurrentConnectionScope.DbConnection, "ti_access_site", enable);
        }

        private static void ChangeInsertDefaultTriggerState(bool enable)
        {
            Common.ChangeTriggerState(QPContext.CurrentConnectionScope.DbConnection, "ti_statuses_and_default_notif", enable);
        }

        /// <summary>
        /// Добавляет новый сайт
        /// </summary>
        /// <param name="site">информация о сайте</param>
        /// <returns>информация о сайте</returns>
        internal static Site Save(Site site)
        {
            using (var scope = new QPConnectionScope())
            {
                if (QPContext.DatabaseType == DatabaseType.SqlServer)
                {
                    ChangeInsertAccessTriggerState(false);
                    ChangeInsertDefaultTriggerState(false);
                    DefaultRepository.TurnIdentityInsertOn(EntityTypeCode.Site, site);
                }

                var fieldValues = site.QpPluginFieldValues;
                var result = DefaultRepository.Save<Site, SiteDAL>(site);

                CommonSecurity.CreateSiteAccess(scope.DbConnection, result.Id);
                CreateDefaultStatuses(result);
                CreateDefaultNotificationTemplate(result);
                CreateDefaultGroup(result);
                UpdatePluginValues(fieldValues, result.Id);

                if (QPContext.DatabaseType == DatabaseType.SqlServer)
                {
                    DefaultRepository.TurnIdentityInsertOff(EntityTypeCode.Site);
                    ChangeInsertAccessTriggerState(true);
                    ChangeInsertDefaultTriggerState(true);
                }

                return result;
            }
        }

        private static void CreateDefaultGroup(Site site)
        {

            var group = new ContentGroupDAL() { Name = ContentGroup.DefaultName, SiteId = site.Id };
            DefaultRepository.SimpleSave(group);
        }

        private static void CreateDefaultNotificationTemplate(Site site)
        {
            if (!site.ExternalDevelopment)
            {
                var template = new PageTemplateDAL()
                {
                    SiteId = site.Id,
                    Name = "Default Notification Template",
                    NetTemplateName = "Default_Notification_Template",
                    TemplatePicture = "",
                    Created = site.Created,
                    Modified = site.Modified,
                    LastModifiedBy = site.LastModifiedBy,
                    Charset = "utf-8",
                    Locale = 65001,
                    Codepage = 1049,
                    IsSystem = true,
                    NetLanguageId = NetLanguage.GetcSharp().Id
                };
                DefaultRepository.SimpleSave(template);
            }
        }

        private static StatusTypeDAL GetNewBuiltInStatus(Site site, int weight, string name, string description) =>

            new StatusTypeDAL
            {
                SiteId = site.Id,
                Name = name,
                Weight = weight,
                Description = description,
                Created = site.Created,
                Modified = site.Modified,
                LastModifiedBy = site.LastModifiedBy,
                BuiltIn = true,
            };

        private static void CreateDefaultStatuses(Site site)
        {
            var statuses = new StatusTypeDAL[]
            {
                GetNewBuiltInStatus(site, 10, "Created","Article has been created"),
                GetNewBuiltInStatus(site, 50, "Approved","Article has been approved"),
                GetNewBuiltInStatus(site, 100, "Published","Article has been published"),
                GetNewBuiltInStatus(site, 0, "None","No Status has been assigned")
            };

            DefaultRepository.SimpleSaveBulk(statuses);
        }

        /// <summary>
        /// Обновляет информацию о сайте
        /// </summary>
        /// <param name="site">информация о сайте</param>
        /// <returns>информация о сайте</returns>
        internal static Site Update(Site site)
        {
            var fieldValues = site.QpPluginFieldValues;
            var result = DefaultRepository.Update<Site, SiteDAL>(site);
            UpdatePluginValues(fieldValues, result.Id);
            return result;
        }

        /// <summary>
        /// Удаляет сайт по его идентификатору
        /// </summary>
        /// <param name="id">идентификатор сайта</param>
        internal static void Delete(int id)
        {
            DefaultRepository.Delete<SiteDAL>(id);
        }

        internal static List<PathSecurityInfo> GetPaths()
        {
            return QPContext.EFContext.SiteSet.Select(n => new PathSecurityInfo { Id = (int)n.Id, Path = n.UploadDir }).ToList();
        }

        internal static List<PathInfo> GetPathInfoList()
        {
            return QPContext.EFContext.SiteSet.Select(n => new PathInfo { Url = (n.UseAbsoluteUploadUrl == 1 ? n.UploadUrlPrefix : "//" + n.Dns) + n.UploadUrl, Path = n.UploadDir, BaseUploadUrl = (n.UseAbsoluteUploadUrl == 1 ? n.UploadUrlPrefix : "//" + n.Dns)}).ToList();
        }

        internal static bool ContextClassNameExists(Site site)
        {
            return QPContext.EFContext.ContentSet.Any(n => n.AdditionalContextClassName == site.FullyQualifiedContextClassName && n.SiteId == site.Id);
        }

        internal static bool Exists(int id)
        {
            return QPContext.EFContext.SiteSet.Any(n => n.Id == id);
        }

        internal static IEnumerable<Site> GetList(IEnumerable<int> siteIDs)
        {
            if (siteIDs != null && siteIDs.Any())
            {
                var decSeteIDs = Converter.ToDecimalCollection(siteIDs);
                return MapperFacade.SiteMapper.GetBizList(QPContext.EFContext.SiteSet.Where(n => decSeteIDs.Contains(n.Id)).ToList());
            }

            return Enumerable.Empty<Site>();
        }

        /// <summary>
        /// Возвращает количество статей во всех контентах сайта
        /// </summary>
        /// <param name="siteId"></param>
        /// <returns></returns>
        internal static int GetSiteArticleCount(int siteId)
        {
            using (var scope = new QPConnectionScope())
            {
                return Common.GetSiteArticleCount(siteId, scope.DbConnection);
            }
        }

        /// <summary>
        /// Возвращает количество контентов сайта
        /// </summary>
        /// <param name="siteId"></param>
        /// <returns></returns>
        internal static int GetSiteContentCount(int siteId)
        {
            using (var scope = new QPConnectionScope())
            {
                return Common.GetSiteContentCount(siteId, scope.DbConnection);
            }
        }

        internal static IEnumerable<ListItem> GetSimpleList(IEnumerable<int> siteIDs)
        {
            return GetList(siteIDs).Select(s => new ListItem { Value = s.Id.ToString(), Text = s.Name });
        }

        internal static int GetSiteVirtualContentCount(int siteId)
        {
            using (var scope = new QPConnectionScope())
            {
                return Common.GetSiteVirtualContentCount(scope.DbConnection, siteId);
            }
        }

        internal static int GetSiteRealContentCount(int siteId)
        {
            using (var scope = new QPConnectionScope())
            {
                return Common.GetSiteRealContentCount(siteId, scope.DbConnection);
            }
        }

        internal static int GetSiteContentLinkCount(int siteId)
        {
            using (var scope = new QPConnectionScope())
            {
                return Common.GetSiteContentLinkCount(siteId, scope.DbConnection);
            }
        }

        internal static string CopyFolders(int sourceSiteId, int destinationSiteId)
        {
            using (var scope = new QPConnectionScope())
            {
                var rows = Common.CopyFolders(sourceSiteId, destinationSiteId, scope.DbConnection);
                return MultistepActionHelper.GetXmlFromDataRows(rows, "folder");
            }
        }

        internal static void CopyFolderAccess(string relationsBetweenFoldersXml)
        {
            using (var scope = new QPConnectionScope())
            {
                Common.CopyFolderAccess(scope.DbConnection, relationsBetweenFoldersXml);
            }
        }

        internal static void CopySiteSettings(int sourceSiteId, int destinationSiteId)
        {
            using (var scope = new QPConnectionScope())
            {
                Common.CopyWorkflow(sourceSiteId, destinationSiteId, scope.DbConnection);
                Common.CopySiteAccessRules(sourceSiteId, destinationSiteId, scope.DbConnection);
                Common.CopyActionSiteBind(sourceSiteId, destinationSiteId, scope.DbConnection);
                Common.CopyWorkflowRules(sourceSiteId, destinationSiteId, scope.DbConnection);
                Common.CopyCommandSiteBind(sourceSiteId, destinationSiteId, scope.DbConnection);
                Common.CopyStyleSiteBind(sourceSiteId, destinationSiteId, scope.DbConnection);
            }
        }

        public static List<QpPluginFieldValue> GetPluginValues(int siteId)
        {
            var actualValues = QPContext.EFContext.PluginFieldValueSet
                .Where(n => n.SiteId == siteId)
                .ToDictionary(k => (int)k.PluginFieldId, n => n.Value);

            var pluginDict = QpPluginRepository.GetQpFieldPluginDict();

            var pluginFieldValues = QpPluginRepository.GetPluginFields(QpPluginRelationType.Site)
                .Select(n => new QpPluginFieldValue()
                {
                    Field = n,
                    Plugin = pluginDict[n.Id],
                    Value = actualValues.TryGetValue(n.Id, out var result) ? result : String.Empty
                }).ToList();

            return pluginFieldValues;
        }

        private static void UpdatePluginValues(IEnumerable<QpPluginFieldValue> fieldValues, int siteId)
        {
            var actualFieldIds = QPContext.EFContext
                .PluginFieldValueSet.Where(n => n.SiteId == siteId)
                    .ToDictionary(n => (int)n.PluginFieldId, m => (int)m.Id);

            var entities = QPContext.EFContext;
            foreach (var fieldValue in fieldValues)
            {
                var dalValue = new PluginFieldValueDAL()
                {
                    SiteId = siteId,
                    Value = string.IsNullOrEmpty(fieldValue.Value) ? null : fieldValue.Value,
                    PluginFieldId = fieldValue.Field.Id,
                    Id = actualFieldIds.TryGetValue(fieldValue.Field.Id, out var result) ? result : 0
                };
                entities.Entry(dalValue).State = dalValue.Id == 0 ? EntityState.Added : EntityState.Modified;
            }
            entities.SaveChanges();
        }

    }
}
