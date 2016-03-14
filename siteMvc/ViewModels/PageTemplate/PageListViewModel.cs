using Quantumart.QP8.BLL.ListItems;
using Quantumart.QP8.BLL.Services.DTO;
using Quantumart.QP8.Resources;
using System.Collections.Generic;
using C = Quantumart.QP8.Constants;

namespace Quantumart.QP8.WebMvc.ViewModels.PageTemplate
{
    public class PageListViewModel : ListViewModel
    {
        public IEnumerable<PageListItem> Data { get; set; }

        public string GettingDataActionName
        {
            get
            {
                return "_IndexPages";
            }
        }

        public static PageListViewModel Create(PageInitListResult result, string tabId, int parentId)
        {
            var model = ViewModel.Create<PageListViewModel>(tabId, parentId);
			model.ShowAddNewItemButton = result.IsAddNewAccessable && !model.IsWindow;
			return model;
        }

        public override string EntityTypeCode
        {
            get { return C.EntityTypeCode.Page; }
        }

        public override string ActionCode
        {
            get { return C.ActionCode.Pages; }
        }

        public override string AddNewItemActionCode
        {
            get
            {
                return C.ActionCode.AddNewPage;
            }
        }

        public override string AddNewItemText
        {
            get
            {
                return TemplateStrings.AddNewPage;
            }
        }
    }
}