using System;

namespace Quantumart.QP8.DAL
{
    public partial class ArticleDAL
    {
        public string Name { get; set; } = String.Empty;

        public string AliasForTree { get; set; } = String.Empty;

        public decimal? ParentId { get; set; }

        public bool HasChildren { get; set; }
    }
}
