using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Quantumart.QP8.DAL
{
    public partial class ArticleDAL
    {
        [NotMapped]
        public string Name { get; set; } = String.Empty;

        [NotMapped]
        public string AliasForTree { get; set; } = String.Empty;

        [NotMapped]
        public decimal? ParentId { get; set; }

        [NotMapped]
        public bool HasChildren { get; set; }
    }
}
