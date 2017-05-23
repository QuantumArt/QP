namespace Quantumart.QP8.DAL.DTO
{
    public class ArticleRelationSecurityParameter
    {
        public bool IsManyToMany { get; set; }

        public bool IsClassifier { get; set; }

        public int[] AllowedContentIds { get; set; }

        public int FieldId { get; set; }

        public string FieldName { get; set; }

        public int? LinkId { get; set; }

        public int RelatedContentId { get; set; }
    }
}
