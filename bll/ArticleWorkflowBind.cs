namespace Quantumart.QP8.BLL
{
    public class ArticleWorkflowBind : WorkflowBind
    {
        public ArticleWorkflowBind()
        {
        }

        public ArticleWorkflowBind(Article article)
        {
            SetArticle(article);
        }

        public void SetArticle(Article article)
        {
            Article = article;
            ArticleId = article.Id;
        }

        public Article Article { get; set; }

        public int ArticleId { get; set; }
    }
}
