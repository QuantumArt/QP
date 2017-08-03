namespace Quantumart.QP8.DAL
{
    public partial class ArticleDAL
    {
        public ArticleDAL()
        {
            Name = string.Empty;
            AliasForTree = string.Empty;
            ParentId = null;
            HasChildren = false;
        }

        public string Name { get; set; }

        public string AliasForTree { get; set; }

        public decimal? ParentId { get; set; }

        public bool HasChildren { get; set; }
    }
}
