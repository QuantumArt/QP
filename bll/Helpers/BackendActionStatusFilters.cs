using System.Collections.Generic;
using System.Linq;
using Quantumart.QP8.BLL.Repository;
using Quantumart.QP8.BLL.Repository.ArticleRepositories;
using Quantumart.QP8.BLL.Repository.ContentRepositories;
using Quantumart.QP8.BLL.Repository.FieldRepositories;
using Quantumart.QP8.BLL.Services;
using Quantumart.QP8.BLL.SharedLogic;
using Quantumart.QP8.Constants;

namespace Quantumart.QP8.BLL.Helpers
{
    public static class BackendActionStatusFilters
    {
        public static IEnumerable<BackendActionStatus> ResolveStatusesForArticle(this IEnumerable<BackendActionStatus> statuses, int entityId, bool? boundToExternal)
        {
            var excluded = new List<BackendActionStatus>();
            var article = ArticleRepository.GetById(entityId);

            if (!article.Content.HasTreeField)
            {
                excluded.AddRange(statuses.Where(s => s.Code == ActionCode.AddNewChildArticle || s.Code == ActionCode.SelectChildArticles || s.Code == ActionCode.UnselectChildArticles));
            }

            if (!article.IsArticleChangingActionsAllowed(boundToExternal))
            {
                excluded.AddRange(statuses.Where(s => ActionCode.ArticleNonChangingActionCodes.Contains(s.Code)));
            }

            foreach (var excludedItem in excluded)
            {
                excludedItem.Visible = false;
            }

            return statuses;
        }

        public static IEnumerable<BackendActionStatus> ResolveStatusesForArticleVersion(this IEnumerable<BackendActionStatus> statuses, int entityId, bool? boundToExternal)
        {
            var version = ArticleVersionRepository.GetById(entityId);
            if (!version.Article.IsArticleChangingActionsAllowed(boundToExternal))
            {
                var excluded = statuses.Where(s => ActionCode.ArticleNonChangingActionCodes.Contains(s.Code));
                foreach (var excludedItem in excluded)
                {
                    excludedItem.Visible = false;
                }
            }

            return statuses;
        }

        /// <summary>
        /// Устанавливает соответствующий статус элементам меню связанным с Custom Action
        /// </summary>
        public static IEnumerable<BackendActionStatus> ResolveStatusesForCustomActions(this IEnumerable<BackendActionStatus> statuses, string menuCode, int entityId, int parentEntityId)
        {
            var menu = ContextMenuRepository.GetByCode(menuCode);
            if (menu == null)
            {
                return Enumerable.Empty<BackendActionStatus>();
            }

            if (menu.EntityType == null)
            {
                return statuses;
            }

            return CustomActionResolver.ResolveStatus(menu.EntityType.Code, entityId, parentEntityId, statuses.ToArray());
        }

        /// <summary>
        /// Делает невидимыми некоторые элементы контекстного меню для корневого Site Folder
        /// </summary>
        public static IEnumerable<BackendActionStatus> ResolveStatusesForSiteFolder(this IEnumerable<BackendActionStatus> statuses, int entityId)
        {
            var folder = SiteFolderService.GetById(entityId);
            if (!folder.ParentId.HasValue)
            {
                var codes = new[] { ActionCode.SiteFolderProperties, ActionCode.RemoveSiteFolder };
                foreach (var status in statuses)
                {
                    if (codes.Contains(status.Code))
                    {
                        status.Visible = false;
                    }
                }
            }

            return statuses;
        }

        public static IEnumerable<BackendActionStatus> ResolveStatusesForContentGroup(this IEnumerable<BackendActionStatus> statuses, int entityId)
        {
            var group = ContentRepository.GetGroupById(entityId);
            if (group.IsDefault)
            {
                var status = statuses.SingleOrDefault(n => n.Code == ActionCode.ContentGroupProperties);
                status.Visible = false;
            }

            return statuses;
        }

        /// <summary>
        /// Делает невидимыми некоторые элементы контекстного меню для корневого Content Folder
        /// </summary>
        public static IEnumerable<BackendActionStatus> ResolveStatusesForContentFolder(this IEnumerable<BackendActionStatus> statuses, int entityId)
        {
            var folder = ContentFolderService.GetById(entityId);
            if (!folder.ParentId.HasValue)
            {
                var codes = new[] { ActionCode.ContentFolderProperties, ActionCode.RemoveContentFolder };
                foreach (var status in statuses)
                {
                    if (codes.Contains(status.Code))
                    {
                        status.Visible = false;
                    }
                }
            }

            return statuses;
        }

        public static IEnumerable<BackendActionStatus> ResolveStatusesForField(this IEnumerable<BackendActionStatus> statuses, int entityId)
        {
            var field = FieldRepository.GetById(entityId);
            if (field != null)
            {
                if (field.TypeId != FieldTypeCodes.DynamicImage)
                {
                    var dynStatus = statuses.SingleOrDefault(s => s.Code == ActionCode.RecreateDynamicImages);
                    if (dynStatus != null)
                    {
                        dynStatus.Visible = false;
                    }
                }

                if (field.TypeId == FieldTypeCodes.DynamicImage || field.TypeId == FieldTypeCodes.M2ORelation)
                {
                    var applyStatus = statuses.SingleOrDefault(s => s.Code == ActionCode.ApplyFieldDefaultValue);
                    if (applyStatus != null)
                    {
                        applyStatus.Visible = false;
                    }
                }
            }

            return statuses;
        }
    }
}
