using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using Quantumart.QP8.BLL.Processors.TreeProcessors;
using Quantumart.QP8.BLL.Repository.ArticleRepositories;
using Quantumart.QP8.BLL.Repository.ArticleRepositories.SearchParsers;
using Quantumart.QP8.Constants;
using Quantumart.QP8.DAL.DTO;

namespace Quantumart.QP8.BLL.Factories
{
    internal static class ArticleTreeFactory
    {
        /// <summary>
        /// Фабрика для запроса на отображение дерева в QP
        /// </summary>
        internal static ITreeProcessor Create(string entityTypeCode, int? parentEntityId, int? entityId, bool returnSelf, string commonFilter, string hostFilter, string selectItemIDs, IList<ArticleSearchQueryParam> searchQuery, IList<ArticleContextQueryParam> contextQuery, ArticleFullTextSearchQueryParser ftsParser)
        {
            using (new QPConnectionScope())
            {
                if (entityTypeCode == EntityTypeCode.Article || entityTypeCode == EntityTypeCode.VirtualArticle)
                {
                    var contentId = parentEntityId.GetValueOrDefault();
                    commonFilter = ArticleRepository.FillFullTextSearchParams(contentId, commonFilter, searchQuery, ftsParser, out var ftsOptions, out var extensionContentIds, out var contentReferences);

                    var filterSqlParams = new List<SqlParameter>();
                    var sourceQuery = new ArticleFilterSearchQueryParser().GetFilter(searchQuery, filterSqlParams);
                    var linkedFilters = (ArticleRepository.GetLinkSearchParameter(searchQuery) ?? new ArticleLinkSearchParameter[0]).ToList();
                    var hasFtsSearchParams = !string.IsNullOrEmpty(ftsOptions.QueryString) && !(ftsOptions.HasError.HasValue && ftsOptions.HasError.Value);
                    var hasFilterSearchParams = !string.IsNullOrEmpty(sourceQuery) || linkedFilters.Any();
                    var combinedFilter = string.IsNullOrWhiteSpace(sourceQuery)
                        ? hostFilter
                        : string.IsNullOrWhiteSpace(hostFilter)
                            ? sourceQuery
                            : $"({hostFilter} AND {sourceQuery})";

                    var filterForSmpl = string.IsNullOrWhiteSpace(combinedFilter) ? commonFilter : combinedFilter;
                    return hasFtsSearchParams || hasFilterSearchParams
                        ? new ArticleFtsProcessor(contentId, commonFilter, combinedFilter, linkedFilters, contextQuery, filterSqlParams, extensionContentIds, ftsOptions)
                        : new ArticleSimpleProcessor(contentId, entityId, filterForSmpl, entityTypeCode, selectItemIDs) as ITreeProcessor;
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
