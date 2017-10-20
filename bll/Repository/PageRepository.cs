using System.Collections.Generic;
using System.Data;
using System.Linq;
using Quantumart.QP8.BLL.Facades;
using Quantumart.QP8.BLL.Helpers;
using Quantumart.QP8.BLL.ListItems;
using Quantumart.QP8.DAL;
using Quantumart.QP8.Utils;

namespace Quantumart.QP8.BLL.Repository
{
	class PageRepository
	{
		internal static IEnumerable<PageListItem> ListPages(ListCommand cmd, int templateId, out int totalRecords)
		{
			using (var scope = new QPConnectionScope())
			{
				var rows = Common.GetPagesByTemplateId(scope.DbConnection, templateId, cmd.SortExpression, out totalRecords, cmd.StartRecord, cmd.PageSize);
				return MapperFacade.PageRowMapper.GetBizList(rows.ToList());
			}
		}

		internal static Page GetPagePropertiesById(int id)
		{
			return MapperFacade.PageMapper.GetBizObject(QPContext.EFContext.PageSet.Include("PageTemplate.Site").Include("LastModifiedByUser")
				.SingleOrDefault(g => g.Id == id)
			);
		}

		internal static Page UpdatePageProperties(Page page) => DefaultRepository.Update<Page, PageDAL>(page);

	    internal static Page SavePageProperties(Page page) => DefaultRepository.Save<Page, PageDAL>(page);

	    internal static void DeletePage(int id)
		{
			DefaultRepository.Delete<PageDAL>(id);
		}

		internal static IEnumerable<PageListItem> ListSitePages(ListCommand listCommand, int parentId, out int totalRecords)
		{
			using (var scope = new QPConnectionScope())
			{
				var rows = Common.GetPagesBySiteId(scope.DbConnection, parentId, listCommand.SortExpression, out totalRecords, listCommand.StartRecord, listCommand.PageSize);
				return MapperFacade.PageRowMapper.GetBizList(rows.ToList());
			}
		}

		/// <summary>
		/// Создает копию страницы
		/// </summary>
		/// <param name="page"></param>
		internal static int CopyPage(Page page, int currentUserId)
		{
			var oldId = page.Id;
			page.Id = 0;			
			page.LockedBy = 0;
			var newPage = SavePageProperties(page);
			var newId = newPage.Id;
			var helper = new PageCopyHelper(oldId, newId);
			helper.Proceed();
			return newId;
		}

		internal static bool NameExists(Page page)
		{
			return QPContext.EFContext.PageSet.Include("PageTemplate")
				.Any(n => n.Name == page.Name && n.Id != page.Id && n.PageTemplate.SiteId == page.PageTemplate.SiteId);
		}

		internal static bool FolderExists(Page page)
		{
			return QPContext.EFContext.PageSet.Include("PageTemplate")
				.Any(n => n.Folder == page.Folder && n.Id != page.Id && n.PageTemplate.Id == page.PageTemplate.Id);
		}

		internal static bool FileNameExists(Page page)
		{
			using (var scope = new QPConnectionScope())
			{
				var pathToCheck = page.PageTemplate.Site.LiveDirectory + page.PageTemplate.TemplateFolder + page.Folder + page.FileName;
				return Common.PageFileNameExists(scope.DbConnection, pathToCheck, page.PageTemplate.SiteId);
			}
		}

		internal static IEnumerable<Page> GetList(IEnumerable<int> IDs)
		{
			IEnumerable<decimal> decIDs = Converter.ToDecimalCollection(IDs).Distinct().ToArray();
			return MapperFacade.PageMapper
				.GetBizList(QPContext.EFContext.PageSet
					.Where(f => decIDs.Contains(f.Id))
					.ToList()
				);		
		}

        internal static string GetRelationsBetweenPages(string relationsBetweenTemplates)
        {
            using (var scope = new QPConnectionScope())
            {
                var rows = Common.GetRelationsBetweenPages(scope.DbConnection, relationsBetweenTemplates);
                return MultistepActionHelper.GetXmlFromDataRows(rows, "page");
            }
        }

        internal static IEnumerable<DataRow> CopySiteTemplatePages(int sourceSiteId, int destinationSiteId, string relationsBetweenTemplates)
        {
            using (var scope = new QPConnectionScope())
            {
                return Common.CopySiteTemplatePages(scope.DbConnection, sourceSiteId, destinationSiteId, relationsBetweenTemplates);
            }
        }
	}
}
