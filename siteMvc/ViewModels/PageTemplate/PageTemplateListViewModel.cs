using System.Collections.Generic;
using Quantumart.QP8.BLL.ListItems;
using Quantumart.QP8.BLL.Services.DTO;
using Quantumart.QP8.Resources;

namespace Quantumart.QP8.WebMvc.ViewModels.PageTemplate
{
    public class PageTemplateListViewModel : ListViewModel
    {
        public IEnumerable<PageTemplateListItem> Data { get; set; }

        public string GettingDataActionName => "_IndexTemplates";

        public static PageTemplateListViewModel Create(PageTemplateInitListResult result, string tabId, int parentId)
        {
            var model = ViewModel.Create<PageTemplateListViewModel>(tabId, parentId);
            model.ShowAddNewItemButton = result.IsAddNewAccessable && !model.IsWindow;
            return model;
        }

        public override string EntityTypeCode => Constants.EntityTypeCode.PageTemplate;

        public override string ActionCode => Constants.ActionCode.Templates;

        public override string AddNewItemActionCode => Constants.ActionCode.AddNewPageTemplate;

        public override string AddNewItemText => TemplateStrings.AddNewTemplate;
    }
}
