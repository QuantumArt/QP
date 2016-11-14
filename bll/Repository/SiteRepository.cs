using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using Quantumart.QP8;
using Quantumart.QP8.BLL.Facades;
using Quantumart.QP8.Utils;
using Quantumart.QP8.BLL.Mappers;
using Quantumart.QP8.DAL;
using Quantumart.QP8.DAL.DTO;
using Quantumart.QP8.BLL.ListItems;
using Quantumart.QP8.BLL.Helpers;
using Quantumart.QP8.Constants;

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
			Site result = GetByIdFromCache(id);
			if (result != null)
				return result;

            return MapperFacade.SiteMapper.GetBizObject(
                QPContext.EFContext.SiteSet
                    .Include("LastModifiedByUser")
                    .Include("LockedByUser")
                    .Where(n => n.Id == (decimal)id)
                    .SingleOrDefault()
            );
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
        /// <param name="sortExpression">настройки сортировки</param>
        /// <param name="beginRowIndex">индекс начальной записи</param>
        /// <param name="pageSize">максимальное количество записей на одной странице</param>
        /// <param name="totalRecords">общее количество записей</param>
        /// <returns>список сайтов</returns>
        internal static ListResult<SiteListItem> GetList(ListCommand cmd, IEnumerable<int> selectedIDs)
        {
            SitePageOptions options = new SitePageOptions()
            {
                SelectedIDs = selectedIDs,
                SortExpression = cmd.SortExpression,
                StartRecord = cmd.StartRecord,
                PageSize = cmd.PageSize,
                UserId = QPContext.CurrentUserId,
                UseSecurity = !QPContext.IsAdmin
            };

            int totalRecords = -1;
            using (var scope = new QPConnectionScope())
            {
                IEnumerable<DataRow> rows = Common.GetSitesPage(scope.DbConnection, options, out totalRecords);
                return new ListResult<SiteListItem> { Data = MapperFacade.SiteListItemRowMapper.GetBizList(rows.ToList()), TotalRecords = totalRecords };
            }
        }

        internal static IEnumerable<Site> GetAll()
        {
            return MapperFacade.SiteMapper.GetBizList(QPContext.EFContext.SiteSet.OrderBy(S => S.Name).ToList());
        }

        /// <summary>
        /// Добавляет новый сайт
        /// </summary>
        /// <param name="site">информация о сайте</param>
        /// <returns>информация о сайте</returns>
        internal static Site Save(Site site)
        {
            DefaultRepository.TurnIdentityInsertOn(EntityTypeCode.Site, site);
            var result = DefaultRepository.Save<Site, SiteDAL>(site);
            DefaultRepository.TurnIdentityInsertOff(EntityTypeCode.Site);
            return result;
        }

        /// <summary>
        /// Обновляет информацию о сайте
        /// </summary>
        /// <param name="site">информация о сайте</param>
        /// <returns>информация о сайте</returns>
        internal static Site Update(Site site)
        {
            return DefaultRepository.Update<Site, SiteDAL>(site);
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
            return QPContext.EFContext.SiteSet
                .Select(n => new PathSecurityInfo { Id = (int)n.Id, Path = n.UploadDir })
                .ToList();
        }

		internal static List<PathInfo> GetPathInfoList()
		{
			return QPContext.EFContext.SiteSet
				.Select(n => new PathInfo { Url = ((n.UseAbsoluteUploadUrl == 1) ? n.UploadUrlPrefix : "http://" + n.Dns) + n.UploadUrl, Path = n.UploadDir })
				.ToList();
		}

        internal static bool ContextClassNameExists(Site site)
        {
            return QPContext.EFContext.ContentSet
                .Any(n => n.AdditionalContextClassName == site.FullyQualifiedContextClassName && n.SiteId == site.Id);
        }

        internal static bool Exists(int id)
        {
            return QPContext.EFContext.SiteSet.Any(n => n.Id == id);
        }

        internal static IEnumerable<Site> GetList(IEnumerable<int> siteIDs)
        {
            if (siteIDs != null && siteIDs.Any())
            {
                IEnumerable<decimal> decSeteIDs = Converter.ToDecimalCollection(siteIDs);
                return MapperFacade.SiteMapper.GetBizList(QPContext.EFContext.SiteSet.Where(n => decSeteIDs.Contains(n.Id)).ToList());
            }
            else
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
            return GetList(siteIDs)
                .Select(s => new ListItem { Value = s.Id.ToString(), Text = s.Name });

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
                IEnumerable<DataRow> rows = Common.CopyFolders(sourceSiteId, destinationSiteId, scope.DbConnection);
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
                //Common.CopyWorkflowAccess(sourceSiteId, destinationSiteId, scope.DbConnection);
                Common.CopyCommandSiteBind(sourceSiteId, destinationSiteId, scope.DbConnection);
                Common.CopyStyleSiteBind(sourceSiteId, destinationSiteId, scope.DbConnection);
            }
        }
    }
}