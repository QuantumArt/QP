using Quantumart.QP8.WebMvc.ViewModels.Abstract;

namespace Quantumart.QP8.WebMvc.ViewModels.HomePage
{
    public class ArticleBaseListViewModel : ListViewModel
    {
        public static ArticleBaseListViewModel Create(int id, string tabId, int parentId)
        {
            var model = Create<ArticleBaseListViewModel>(tabId, parentId);
            return model;
        }

        public override bool AllowMultipleEntitySelection => true;

        public override bool LinkOpenNewTab => true;

        public override string EntityTypeCode => Constants.EntityTypeCode.Article;

        public override string ActionCode => Constants.ActionCode.LockedArticles;

        public override bool IsListDynamic => true;

        public string DataBindingControllerName => "Home";

        public string DataBindingActionName { get; set; }
    }
}
