using C = Quantumart.QP8.Constants;


namespace Quantumart.QP8.WebMvc.ViewModels.ArticleVersion
{
    public class ArticleVersionListViewModel : ListViewModel
    {
        #region creation

        public static ArticleVersionListViewModel Create(string tabId, int parentId)
        {
            var model = ViewModel.Create<ArticleVersionListViewModel>(tabId, parentId);
            model.ShowAddNewItemButton = !model.IsWindow;
            return model;
        }

        #endregion

        #region read-only members

        public override string EntityTypeCode => C.EntityTypeCode.ArticleVersion;

        public override string ActionCode => C.ActionCode.ArticleVersions;

        #endregion
    }
}