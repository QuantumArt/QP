using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Quantumart.QP8.DAL;
using Quantumart.QP8.BLL.Mappers;

namespace Quantumart.QP8.BLL.Repository
{
	internal static class AssembleRepository
	{
		/// <summary>
		/// Возвращает список ID шаблонов сайта
		/// </summary>
		/// <param name="SiteId"></param>
		/// <returns></returns>
		internal static IEnumerable<int> GetSiteTemplatesId(int siteId)
		{
			using (var scope = new QPConnectionScope())
			{
				return Common.AssembleAction_GetSiteTemplatesId(siteId, scope.DbConnection);
			}
		}

		/// <summary>
		/// Возвращает список ID страниц сайта
		/// </summary>
		internal static PageInfo[] GetSitePages(int siteId)
		{
			using (var scope = new QPConnectionScope())
			{
				var rows = Common.AssembleAction_GetSitePages(siteId, scope.DbConnection);
                return MappersRepository.DataRowMapper.Map<PageInfo>(rows);
            }
		}

		internal static int UpdatePageStatus(int pageId, int userId)
		{
			using (var scope = new QPConnectionScope())
			{
				return Common.AssembleAction_UpdatePageStatus(pageId, userId, scope.DbConnection);
			}
		}

		/// <summary>
		/// Возвращает список ID уведомлений сайта
		/// </summary>
		internal static IEnumerable<int> GetSiteFormatId(int siteId)
		{
			using (var scope = new QPConnectionScope())
			{
				return Common.AssembleAction_GetSiteFormatIds(siteId, scope.DbConnection);
			}
		}

		/// <summary>
		/// Возвращает список ID уведомлений шаблона
		/// </summary>
		internal static IEnumerable<int> GetTemplateFormatId(int templateId)
		{
			using (var scope = new QPConnectionScope())
			{
				return Common.AssembleAction_GetTemplateFormatIds(templateId, scope.DbConnection);
			}
		}

		internal static PageInfo[] GetTemplatePages(int templateId)
		{
			using (var scope = new QPConnectionScope())
			{
				var rows = Common.AssembleAction_TemplatePages(templateId, scope.DbConnection);
                return MappersRepository.DataRowMapper.Map<PageInfo>(rows);
            }
		}
	}

    public class PageInfo
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Template { get; set; }
    }
}
