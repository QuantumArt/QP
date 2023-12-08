using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using Quantumart.QP8.BLL.Repository;
using Quantumart.QP8.BLL.Repository.ArticleRepositories;
using Quantumart.QP8.BLL.Repository.ContentRepositories;
using Quantumart.QP8.BLL.Repository.FieldRepositories;
using Quantumart.QP8.BLL.Services.DTO;

namespace Quantumart.QP8.BLL.Processors.TreeProcessors
{
    /// <summary>
    /// Рекурсивно загружает дерево и если необходимо, - дочерние элементы
    /// </summary>
    internal class ArticleSimpleProcessor : ITreeProcessor
    {
        private readonly int _contentId;
        private readonly int? _entityId;
        private readonly string _filter;
        private readonly string _entityTypeCode;
        private readonly string _selectedIdsString;
        private IList<DbParameter> _sqlParameters;

        public ArticleSimpleProcessor(int contentId, int? entityId, string filter, string entityTypeCode, string selectedIdsString, IList<DbParameter> sqlParameters)
        {
            _contentId = contentId;
            _entityId = entityId;
            _filter = filter;
            _entityTypeCode = entityTypeCode;
            _selectedIdsString = selectedIdsString;
            _sqlParameters = sqlParameters;
        }

        public IList<EntityTreeItem> Process()
        {
            var treeField = FieldRepository.GetById(ContentRepository.GetTreeFieldId(_contentId));
            return GetChildArticles(_entityId, _filter, EntityObjectRepository.GetTreeIdsToLoad(_entityTypeCode, treeField.ContentId, _selectedIdsString), treeField, _sqlParameters);
        }

        private static IList<EntityTreeItem> GetChildArticles(int? parentArticleId, string filter, ICollection<int> selectedIds, Field treeField, IList<DbParameter> sqlParameters)
        {
            var treeRows = ArticleRepository.GetArticleTreeForParentResult(parentArticleId, filter, treeField, sqlParameters).ToList();
            foreach (var row in treeRows.Where(n => selectedIds.Contains(int.Parse(n.Id))))
            {
                row.Children = GetChildArticles(int.Parse(row.Id), filter, selectedIds, treeField, sqlParameters);
            }

            return treeRows;
        }
    }
}
