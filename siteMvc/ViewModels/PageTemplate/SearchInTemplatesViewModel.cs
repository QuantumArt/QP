namespace Quantumart.QP8.WebMvc.ViewModels.PageTemplate
{
    public sealed class SearchInTemplatesViewModel : ListViewModel
    {
        public static SearchInTemplatesViewModel Create(string tabId, int parentId) => Create<SearchInTemplatesViewModel>(tabId, parentId);

        public override string EntityTypeCode => Constants.EntityTypeCode.PageTemplate;

        public override string ActionCode => Constants.ActionCode.SearchInTemplates;

        public string GridElementId => UniqueId("Grid");

        public string FilterElementId => UniqueId("Filter");

        public override string ContextMenuCode => string.Empty;

        public override bool LinkOpenNewTab => true;

        public override string ActionCodeForLink => Constants.ActionCode.PageTemplateProperties;
    }
}
