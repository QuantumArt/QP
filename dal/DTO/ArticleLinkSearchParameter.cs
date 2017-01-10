namespace Quantumart.QP8.DAL.DTO
{
    public class ArticleLinkSearchParameter
    {
        public int LinkId { get; set; }

        public int[] Ids { get; set; }

        public bool IsManyToMany { get; set; }

        public bool IsNull { get; set; }

        public bool Inverse { get; set; }

        public string FieldName { get; set; }

        public int ContentId { get; set; }

        public int ExstensionContentId { get; set; }

        public int ReferenceFieldId { get; set; }

        public bool UnionAll { get; set; }
    }
}
