using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Quantumart.QP8;
using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Repository;
using Quantumart.QP8.Constants;
using Quantumart.QP8.BLL.SharedLogic;
using Quantumart.QP8.BLL.Repository.Articles;
using Quantumart.QP8.BLL.Helpers;

namespace Quantumart.QP8.BLL.Services
{
    public class ContextMenuService
    {
        /// <summary>
        /// Возвращает контекстное меню по его идентификатору
        /// </summary>
        /// <param name="menuId">идентификатор меню</param>
        /// <returns>контекстное меню</returns>
        public static ContextMenu GetById(int menuId, bool loadItems = false)
        {
            return ContextMenuRepository.GetById(menuId, loadItems);
        }

        /// <summary>
        /// Возвращает контекстное меню по его коду
        /// </summary>
        /// <param name="menuCode">код меню</param>
        /// <returns>контекстное меню</returns>
        public static ContextMenu GetByCode(string menuCode, bool loadItems = false)
        {
            if (String.IsNullOrWhiteSpace(menuCode))
                return null;
            else
                return ContextMenuRepository.GetByCode(menuCode, loadItems);
        }

        /// <summary>
        /// Возвращает список контекстных меню
        /// </summary>
        /// <returns>список контекстных меню</returns>
        public static List<ContextMenu> GetList()
        {
            return ContextMenuRepository.GetList();
        }

		public static IEnumerable<BackendActionStatus> GetStatusesList(string menuCode, int entityId, int parentEntityId, bool? boundToExternal)
        {
            IEnumerable<BackendActionStatus> result = ContextMenuRepository.GetStatusesList(menuCode, entityId);
			result = result.ResolveStatusesForCustomActions(menuCode, entityId, parentEntityId);
			if (menuCode == EntityTypeCode.ContentGroup)
				result = result.ResolveStatusesForContentGroup(entityId);
			else if (menuCode == EntityTypeCode.SiteFolder)
				result = result.ResolveStatusesForSiteFolder(entityId);
			else if (menuCode == EntityTypeCode.ContentFolder)
				result = result.ResolveStatusesForContentFolder(entityId);
			else if (menuCode == EntityTypeCode.Article)
				result = result.ResolveStatusesForArticle(entityId, boundToExternal);
			else if (menuCode == EntityTypeCode.Field)
				result = result.ResolveStatusesForField(entityId);
			else if  (menuCode == EntityTypeCode.ArticleVersion)
				result = result.ResolveStatusesForArticleVersion(entityId, boundToExternal);
			return result;
        }


    }
}
