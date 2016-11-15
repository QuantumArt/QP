namespace Quantumart.QP8.BLL.Services.API.Models
{
    public class RelationData
    {
        public int ArticleId { get; set; }

        public int ContentId { get; set; }

        public int FieldId { get; set; }

        public string FieldName { get; set; }

        public string FielValue { get; set; }

        public int RefContentId { get; set; }

        public int? RefFieldId { get; set; }

        public int? LinkId { get; set; }

        public override string ToString()
        {
            return new
            {
                ArticleId,
                ContentId,
                FieldId,
                FieldName,
                FielValue,
                RefContentId,
                RefFieldId,
                LinkId
            }.ToString();
        }
    }
}
