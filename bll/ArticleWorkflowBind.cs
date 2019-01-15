namespace Quantumart.QP8.BLL
{
    public class ArticleWorkflowBind : WorkflowBind
    {
        public ArticleWorkflowBind()
        {
        }

        public static ArticleWorkflowBind Create(Article article)
        {
            var result = new ArticleWorkflowBind();
            result.SetArticle(article);
            return result;
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
