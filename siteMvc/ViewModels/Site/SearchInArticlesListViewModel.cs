namespace Quantumart.QP8.WebMvc.ViewModels
{
    public class SearchInArticlesListViewModel : ListViewModel
    {
        public int SiteId { get; set; }

        public string Query { get; set; }

        public static SearchInArticlesListViewModel Create(int id, string tabId, int parentId)
        {
            var model = Create<SearchInArticlesListViewModel>(tabId, parentId);
            model.SiteId = id;
            model.ShowAddNewItemButton = !model.IsWindow;
            model.AutoLoad = false;
            return model;
        }

        public override string EntityTypeCode => Constants.EntityTypeCode.Article;

        public override string ActionCode => Constants.ActionCode.SearchInArticles;

        public override bool AllowMultipleEntitySelection => false;

        public override string ContextMenuCode => string.Empty;

        public override bool LinkOpenNewTab => true;

        public string DataBindingControllerName => "Site";

        public string DataBindingActionName => "_SearchInArticles";

        public string SeachBlockElementId => UniqueId("SearchInArticlesBlock");

        public string SearchTextBoxElementId => UniqueId("SearchInArticlesTextBox");

        public string SearchButtonElementId => UniqueId("SearchInArticlesSearchButton");

        public string SearchLayoutFormElementId => UniqueId("SearchLayoutForm");
    }
}
