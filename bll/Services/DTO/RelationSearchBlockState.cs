using System.Collections.Generic;

namespace Quantumart.QP8.BLL.Services.DTO
{
    /// <summary>
    /// Состояние блока поиска по полю типа Relation
    /// </summary>
    public class RelationSearchBlockState
    {
        public ArticleFieldSearchType SearchType { get; set; }
        public int FieldId { get; set; }
        public int ReferenceFieldId { get; set; }
        public int ContentId { get; set; }
        public string FieldName { get; set; }
        public string FieldGroup { get; set; }
        public string FieldColumnName { get; set; }
        public IEnumerable<EntityListItem> SelectedEntities { get; set; }
    }
}
