using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using Quantumart.QP8.BLL.Repository.ArticleRepositories;
using Quantumart.QP8.BLL.Repository.ContentRepositories;
using Quantumart.QP8.BLL.Repository.FieldRepositories;
using Quantumart.QP8.BLL.Services.DTO;
using Quantumart.QP8.DAL.DTO;

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
        private readonly IList<ArticleLinkSearchParameter> _linkedFilters;
        private readonly IList<ArticleContextQueryParam> _contextQuery;
        private readonly ICollection<DbParameter> _filterSqlParams;
        private readonly int[] _extensionContentIds;
        private readonly ArticleFullTextSearchParameter _ftsOptions;
        private IList<DbParameter> _sqlParameters;

        public ArticleFtsProcessor(int parentEntityId, string commonFilter, string filterQuery, IList<ArticleLinkSearchParameter> linkedFilters, IList<ArticleContextQueryParam> contextQuery, ICollection<DbParameter> filterSqlParams, int[] extensionContentIds, ArticleFullTextSearchParameter ftsOptions, IList<DbParameter> sqlParameters)
        {
            _parentEntityId = parentEntityId;
            _commonFilter = commonFilter;
            _filterQuery = filterQuery;
            _linkedFilters = linkedFilters;
            _contextQuery = contextQuery;
            _filterSqlParams = filterSqlParams;
            _extensionContentIds = extensionContentIds;
            _ftsOptions = ftsOptions;
            _sqlParameters = sqlParameters;
        }

        public IList<EntityTreeItem> Process()
        {
            var treeField = FieldRepository.GetById(ContentRepository.GetTreeFieldId(_parentEntityId));
            return ArticleRepository.GetArticlesTreeForFtsResult(_commonFilter, treeField, _filterQuery, _linkedFilters, _contextQuery, _filterSqlParams, _extensionContentIds, _ftsOptions, _sqlParameters).ToList();
        }
    }
}
