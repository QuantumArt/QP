using Quantumart.QP8.BLL.Processors.TreeProcessors;
using Quantumart.QP8.BLL.Repository.Articles;
using Quantumart.QP8.Constants;
using Quantumart.QP8.DAL.DTO;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;

namespace Quantumart.QP8.BLL.Factories
{
    internal static class ArticleTreeFactory
    {
        /// <summary>
        /// Фабрика для деревьев
        /// </summary>
        /// <returns>список дочерних сущностей</returns>
        internal static ITreeProcessor Create(string entityTypeCode, int? parentEntityId, int? entityId, bool returnSelf, string commonFilter, string selectItemIDs, IEnumerable<ArticleSearchQueryParam> searchQuery, IEnumerable<ArticleContextQueryParam> contextQuery, ArticleFullTextSearchQueryParser ftsParser)
        {
            using (new QPConnectionScope())
            {
                if (entityTypeCode == EntityTypeCode.Article || entityTypeCode == EntityTypeCode.VirtualArticle)
                {
                    var contentId = parentEntityId.GetValueOrDefault();
                    int[] extensionContentIds;
                    ContentReference[] contentReferences;
                    ArticleFullTextSearchParameter ftsOptions;
                    commonFilter = ArticleRepository.FillFullTextSearchParams(contentId, commonFilter, searchQuery, ftsParser, out ftsOptions, out extensionContentIds, out contentReferences);

                    var filterSqlParams = new List<SqlParameter>();
                    var filterQuery = new ArticleFilterSearchQueryParser().GetFilter(searchQuery, filterSqlParams);
                    var linkedFilters = ArticleRepository.GetLinkSearchParameter(searchQuery);

                    var hasFtsSearchParams = !string.IsNullOrEmpty(ftsOptions.QueryString) && !(ftsOptions.HasError.HasValue && ftsOptions.HasError.Value);
                    var hasFilterSearchParams = !string.IsNullOrEmpty(filterQuery) || (linkedFilters != null && linkedFilters.Any());

                    return hasFtsSearchParams || hasFilterSearchParams
                        ? new ArticleFtsProcessor(contentId, commonFilter, filterQuery, linkedFilters, contextQuery, filterSqlParams, extensionContentIds, ftsOptions)
                        : new ArticleSimpleProcessor(contentId, entityId, commonFilter, entityTypeCode, selectItemIDs) as ITreeProcessor;
                }

                if (entityTypeCode == EntityTypeCode.SiteFolder || entityTypeCode == EntityTypeCode.ContentFolder)
                {
                    return new SiteFolderProcessor(parentEntityId, entityTypeCode, returnSelf, entityId);
                }

                if (entityTypeCode == EntityTypeCode.UserGroup)
                {
                    return new UserGroupProcessor(entityTypeCode, entityId);
                }
            }

            throw new NotImplementedException(entityTypeCode);
        }
    }
}
