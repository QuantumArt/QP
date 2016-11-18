
namespace Quantumart.QP8.WebMvc.ViewModels.Article
{
    public class ArticleStatusHistoryListViewModel : ListViewModel
    {
        public static ArticleStatusHistoryListViewModel Create(string tabId, int parentId)
        {
            return Create<ArticleStatusHistoryListViewModel>(tabId, parentId);
        }

        public string DataBindingActionName => "_StatusHistoryList";

        public string DataBindingControllerName => "Article";

        public override string ActionCode => Constants.ActionCode.ArticleStatus;

        public override string EntityTypeCode => Constants.EntityTypeCode.Article;

        public override string ContextMenuCode => null;
    }
}
