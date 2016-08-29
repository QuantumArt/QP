using C = Quantumart.QP8.Constants;

namespace Quantumart.QP8.WebMvc.ViewModels.Article
{
    public class ArticleStatusHistoryListViewModel : ListViewModel
    {
        public static ArticleStatusHistoryListViewModel Create(string tabId, int parentId)
        {
            var model = ViewModel.Create<ArticleStatusHistoryListViewModel>(tabId, parentId);
            return model;
        }

        public string DataBindingActionName => "_StatusHistoryList";

        public string DataBindingControllerName => "Article";

        public override string ActionCode => C.ActionCode.ArticleStatus;

        public override string EntityTypeCode => C.EntityTypeCode.Article;
    }
}