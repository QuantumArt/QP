using System.Collections.Generic;
using Quantumart.QP8.BLL.ListItems;
using Quantumart.QP8.BLL.Services.DTO;
using Quantumart.QP8.Resources;

namespace Quantumart.QP8.WebMvc.ViewModels.PageTemplate
{
    public class PageListViewModel : ListViewModel
    {
        public IEnumerable<PageListItem> Data { get; set; }

        public string GettingDataActionName => "_IndexPages";

        public static PageListViewModel Create(PageInitListResult result, string tabId, int parentId)
        {
            var model = Create<PageListViewModel>(tabId, parentId);
            model.ShowAddNewItemButton = result.IsAddNewAccessable && !model.IsWindow;
            return model;
        }

        public override string EntityTypeCode => Constants.EntityTypeCode.Page;

        public override string ActionCode => Constants.ActionCode.Pages;

        public override string AddNewItemActionCode => Constants.ActionCode.AddNewPage;

        public override string AddNewItemText => TemplateStrings.AddNewPage;
    }
}
