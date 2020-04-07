using System.ComponentModel.DataAnnotations.Schema;

namespace Quantumart.QP8.DAL.Entities
{
    public partial class ArticleDAL
    {
        [NotMapped]
        public string Name { get; set; } = string.Empty;

        [NotMapped]
        public string AliasForTree { get; set; } = string.Empty;

        [NotMapped]
        public decimal? ParentId { get; set; }

        [NotMapped]
        public bool HasChildren { get; set; }
    }
}
