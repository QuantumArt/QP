using Quantumart.QP8.BLL;

namespace Quantumart.QP8.ArticleScheduler
{
    internal interface IOperationsLogWriter
    {
        void ShowArticle(Article article);

        void HideArticle(Article article);

        void PublishArticle(Article article);
    }
}
