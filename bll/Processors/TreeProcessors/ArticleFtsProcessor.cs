using Quantumart.QP8.BLL.Repository;
using Quantumart.QP8.BLL.Repository.Articles;
using Quantumart.QP8.BLL.Services.DTO;
using Quantumart.QP8.DAL.DTO;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;

namespace Quantumart.QP8.BLL.Processors.TreeProcessors
{
    /// <summary>
    /// Загружает дерево используя полнотекстовый поиск
    /// </summary>
    internal class ArticleFtsProcessor : ITreeProcessor
    {
        private readonly int _parentEntityId;
        private readonly string _commonFilter;
        private readonly string _filterQuery;
        private readonly IEnumerable<ArticleLinkSearchParameter> _linkedFilters;
        private readonly IEnumerable<ArticleContextQueryParam> _contextQuery;
        private readonly ICollection<SqlParameter> _filterSqlParams;
        private readonly int[] _extensionContentIds;
        private readonly ArticleFullTextSearchParameter _ftsOptions;

        public ArticleFtsProcessor(int parentEntityId, string commonFilter, string filterQuery, IEnumerable<ArticleLinkSearchParameter> linkedFilters, IEnumerable<ArticleContextQueryParam> contextQuery, ICollection<SqlParameter> filterSqlParams, int[] extensionContentIds, ArticleFullTextSearchParameter ftsOptions)
        {
            _parentEntityId = parentEntityId;
            _commonFilter = commonFilter;
            _filterQuery = filterQuery;
            _linkedFilters = linkedFilters;
            _contextQuery = contextQuery;
            _filterSqlParams = filterSqlParams;
            _extensionContentIds = extensionContentIds;
            _ftsOptions = ftsOptions;
        }

        public IList<EntityTreeItem> Process()
        {
            var treeField = FieldRepository.GetById(ContentRepository.GetTreeFieldId(_parentEntityId));
            return ArticleRepository.GetArticlesTreeForFtsResult(_commonFilter, treeField, _filterQuery, _linkedFilters, _contextQuery, _filterSqlParams, _extensionContentIds, _ftsOptions).ToList();
        }
    }
}
