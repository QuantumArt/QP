using Quantumart.QP8.BLL.Facades;
using Quantumart.QP8.BLL.Processors.TreeProcessors;
using Quantumart.QP8.BLL.Repository.ArticleRepositories;
using Quantumart.QP8.BLL.Repository.ArticleRepositories.SearchParsers;
using Quantumart.QP8.BLL.Services.DTO;
using Quantumart.QP8.Constants;
using Quantumart.QP8.DAL;
using Quantumart.QP8.DAL.DTO;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using Quantumart.QP8.BLL.Repository.ContentRepositories;

namespace Quantumart.QP8.BLL.Factories
{
    internal static class ArticleTreeFactory
    {
        /// <summary>
        /// Фабрика для запроса на отображение дерева в QP
        /// </summary>
        public static ITreeProcessor Create(ChildListQuery query)
        {
            using (var scope = new QPConnectionScope())
            {
                if (query.EntityTypeCode == EntityTypeCode.Article || query.EntityTypeCode == EntityTypeCode.VirtualArticle)
                {
                    var contentId = query.ParentEntityId;
                    var content = ContentRepository.GetById(contentId);
                    var searchQueryParams = query.SearchQueryParams;
                    var articleContextQueryParams = query.ContextQueryParams;

                    var sqlFilterParameters = new List<DbParameter>();
                    var filters = MapperFacade.CustomFilterMapper.GetDalList(query?.Filter?.ToList());
                    var customFilter = CommonCustomFilters.GetFilterQuery(
                        scope.DbConnection,
                        sqlFilterParameters,
                        scope.CurrentDbType,
                        EntityTypeCode.Article,
                        query.ParentEntityId,
                        filters.ToArray(),
                        content.UseNativeEfTypes
                    );

                    var sqlHostFilterParameters = new List<DbParameter>();
                    var hostFilters = MapperFacade.CustomFilterMapper.GetDalList(query?.HostFilter?.ToList());
                    var hostFilter = CommonCustomFilters.GetFilterQuery(
                        scope.DbConnection,
                        sqlHostFilterParameters,
                        scope.CurrentDbType,
                        EntityTypeCode.Article,
                        query.ParentEntityId,
                        hostFilters.ToArray(),
                        content.UseNativeEfTypes
                    );

                    customFilter = ArticleRepository.FillFullTextSearchParams(
                        contentId,
                        customFilter,
                        searchQueryParams,
                        query.Parser,
                        out var ftsOptions,
                        out var extensionContentIds,
                        out var _
                    );

                    var filterSqlParams = new List<DbParameter>();
                    var sourceQuery = new ArticleFilterSearchQueryParser().GetFilter(searchQueryParams, filterSqlParams);
                    var linkedFilters = (ArticleRepository.GetLinkSearchParameter(searchQueryParams) ?? new ArticleLinkSearchParameter[0]).ToList();
                    var hasFtsSearchParams = !string.IsNullOrEmpty(ftsOptions.QueryString) && !(ftsOptions.HasError.HasValue && ftsOptions.HasError.Value);
                    var hasFilterSearchParams = !string.IsNullOrEmpty(sourceQuery) || linkedFilters.Any();
                    var combinedFilter = string.IsNullOrWhiteSpace(sourceQuery)
                        ? hostFilter
                        : string.IsNullOrWhiteSpace(hostFilter)
                            ? sourceQuery
                            : $"({hostFilter} AND {sourceQuery})";

                    var isCustom = string.IsNullOrWhiteSpace(combinedFilter);
                    var filterForSmpl = isCustom ? customFilter : combinedFilter;
                    var sqlSmplFilterParameters = isCustom ? sqlFilterParameters : sqlHostFilterParameters;
                    filterSqlParams.AddRange(sqlSmplFilterParameters);

                    return hasFtsSearchParams || hasFilterSearchParams
                        ? new ArticleFtsProcessor(contentId, customFilter, combinedFilter, linkedFilters, articleContextQueryParams, filterSqlParams, extensionContentIds, ftsOptions)
                        : new ArticleSimpleProcessor(contentId, query.EntityId, filterForSmpl, query.EntityTypeCode, query.SelectItemIDs, sqlSmplFilterParameters) as ITreeProcessor;
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
