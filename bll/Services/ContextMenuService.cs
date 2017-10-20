using System.Collections.Generic;
using Quantumart.QP8.BLL.Helpers;
using Quantumart.QP8.BLL.Repository;
using Quantumart.QP8.Constants;

namespace Quantumart.QP8.BLL.Services
{
    public class ContextMenuService
    {
        /// <summary>
        /// Возвращает контекстное меню по его идентификатору
        /// </summary>
        public static ContextMenu GetById(int menuId, bool loadItems = false) => ContextMenuRepository.GetById(menuId, loadItems);

        /// <summary>
        /// Возвращает контекстное меню по его коду
        /// </summary>
        public static ContextMenu GetByCode(string menuCode, bool loadItems = false)
        {
            if (string.IsNullOrWhiteSpace(menuCode))
            {
                return null;
            }

            return ContextMenuRepository.GetByCode(menuCode, loadItems);
        }

        /// <summary>
        /// Возвращает список контекстных меню
        /// </summary>
        public static List<ContextMenu> GetList() => ContextMenuRepository.GetList();

        public static IEnumerable<BackendActionStatus> GetStatusesList(string menuCode, int entityId, int parentEntityId, bool? boundToExternal)
        {
            var result = ContextMenuRepository.GetStatusesList(menuCode, entityId);
            result = result.ResolveStatusesForCustomActions(menuCode, entityId, parentEntityId);
            switch (menuCode)
            {
                case EntityTypeCode.ContentGroup:
                    result = result.ResolveStatusesForContentGroup(entityId);
                    break;
                case EntityTypeCode.SiteFolder:
                    result = result.ResolveStatusesForSiteFolder(entityId);
                    break;
                case EntityTypeCode.ContentFolder:
                    result = result.ResolveStatusesForContentFolder(entityId);
                    break;
                case EntityTypeCode.Article:
                    result = result.ResolveStatusesForArticle(entityId, boundToExternal);
                    break;
                case EntityTypeCode.Field:
                    result = result.ResolveStatusesForField(entityId);
                    break;
                case EntityTypeCode.ArticleVersion:
                    result = result.ResolveStatusesForArticleVersion(entityId, boundToExternal);
                    break;
            }

            return result;
        }
    }
}
