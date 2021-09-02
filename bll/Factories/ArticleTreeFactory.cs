using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using Quantumart.QP8.BLL.Processors.TreeProcessors;
using Quantumart.QP8.BLL.Repository.ArticleRepositories;
using Quantumart.QP8.BLL.Repository.ArticleRepositories.SearchParsers;
using Quantumart.QP8.BLL.Services.DTO;
using Quantumart.QP8.Constants;
using Quantumart.QP8.DAL.DTO;

namespace Quantumart.QP8.BLL.Factories
{
    internal static class ArticleTreeFactory
    {
        /// <summary>
        /// Фабрика для запроса на отображение дерева в QP
        /// </summary>
        internal static ITreeProcessor Create(ChildListQuery query)
        {
            using (new QPConnectionScope())
            {
                if (query.EntityTypeCode == EntityTypeCode.Article || query.EntityTypeCode == EntityTypeCode.VirtualArticle)
                {
                    var contentId = query.ParentEntityId;
                    var searchQueryParams = query.SearchQueryParams;
                    var articleContextQueryParams = query.ContextQueryParams;

                    query.Filter = ArticleRepository.FillFullTextSearchParams(contentId, query.Filter, searchQueryParams , query.Parser, out var ftsOptions, out var extensionContentIds, out var _);

                    var filterSqlParams = new List<DbParameter>();
                    var sourceQuery = new ArticleFilterSearchQueryParser().GetFilter(searchQueryParams, filterSqlParams);
                    var linkedFilters = (ArticleRepository.GetLinkSearchParameter(searchQueryParams) ?? new ArticleLinkSearchParameter[0]).ToList();
                    var hasFtsSearchParams = !string.IsNullOrEmpty(ftsOptions.QueryString) && !(ftsOptions.HasError.HasValue && ftsOptions.HasError.Value);
                    var hasFilterSearchParams = !string.IsNullOrEmpty(sourceQuery) || linkedFilters.Any();
                    var combinedFilter = string.IsNullOrWhiteSpace(sourceQuery)
                        ? query.HostFilter
                        : string.IsNullOrWhiteSpace(query.HostFilter)
                            ? sourceQuery
                            : $"({query.HostFilter} AND {sourceQuery})";

                    var filterForSmpl = string.IsNullOrWhiteSpace(combinedFilter) ? query.Filter : combinedFilter;

                    return hasFtsSearchParams || hasFilterSearchParams
                        ? new ArticleFtsProcessor(contentId, query.Filter, combinedFilter, linkedFilters, articleContextQueryParams, filterSqlParams, extensionContentIds, ftsOptions)
                        : new ArticleSimpleProcessor(contentId, query.EntityId, filterForSmpl, query.EntityTypeCode, query.SelectItemIDs) as ITreeProcessor;
                }

                if (query.EntityTypeCode == EntityTypeCode.SiteFolder || query.EntityTypeCode == EntityTypeCode.ContentFolder)
                {
                    return new SiteFolderProcessor(query.ParentEntityId, query.EntityTypeCode, query.ReturnSelf, query.EntityId);
                }

                if (query.EntityTypeCode == EntityTypeCode.UserGroup)
                {
                    return new UserGroupProcessor(query.EntityTypeCode, query.EntityId);
                }
            }

            throw new NotImplementedException(query.EntityTypeCode);
        }
    }
}
