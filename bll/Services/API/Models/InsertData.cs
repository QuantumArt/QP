namespace Quantumart.QP8.BLL.Services.API.Models
{
    public class InsertData
    {
        public int OriginalArticleId { get; set; }

        public int CreatedArticleId { get; set; }

        public int ContentId { get; set; }

        public override string ToString()
        {
            return new
            {
                OriginalArticleId,
                CreatedArticleId,
                ContentId

            }.ToString();
        }
    }
}
