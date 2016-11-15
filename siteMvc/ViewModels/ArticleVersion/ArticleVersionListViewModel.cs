namespace Quantumart.QP8.WebMvc.ViewModels.ArticleVersion
{
    public class ArticleVersionListViewModel : ListViewModel
    {
        public static ArticleVersionListViewModel Create(string tabId, int parentId)
        {
            var model = Create<ArticleVersionListViewModel>(tabId, parentId);
            model.ShowAddNewItemButton = !model.IsWindow;
            return model;
        }

        public override string EntityTypeCode => Constants.EntityTypeCode.ArticleVersion;

        public override string ActionCode => Constants.ActionCode.ArticleVersions;
    }
}
