namespace Quantumart.QP8.BLL.Services.DTO
{
    public class ArticleInitTreeResult : ArticleResultBase
    {
        public string Filter { get; set; }

        public bool AutoCheckChildren { get; set; }

        public ArticleInitTreeResult(Content content, bool isMultipleSelection)
        {
            IsVirtual = content.IsVirtual;
            ContentName = content.Name;
            IsUpdatable = content.IsUpdatable;
            Filter = content.SelfRelationFieldFilter;
            AutoCheckChildren = isMultipleSelection && content.TreeField != null ? content.TreeField.AutoCheckChildren : false;
        }
    }
}
