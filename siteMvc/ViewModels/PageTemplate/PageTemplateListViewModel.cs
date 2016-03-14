using Quantumart.QP8.BLL.ListItems;
using Quantumart.QP8.BLL.Services.DTO;
using Quantumart.QP8.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using C = Quantumart.QP8.Constants;

namespace Quantumart.QP8.WebMvc.ViewModels.PageTemplate
{
    public class PageTemplateListViewModel : ListViewModel
    {
        public IEnumerable<PageTemplateListItem> Data { get; set; }

        public string GettingDataActionName
        {
            get
            {
                return "_IndexTemplates";
            }
        }

        public static PageTemplateListViewModel Create(PageTemplateInitListResult result, string tabId, int parentId)
        {
            var model = ViewModel.Create<PageTemplateListViewModel>(tabId, parentId);
			model.ShowAddNewItemButton = result.IsAddNewAccessable && !model.IsWindow;
			return model;
        }

        public override string EntityTypeCode
        {
            get { return C.EntityTypeCode.PageTemplate; }
        }

        public override string ActionCode
        {
            get { return C.ActionCode.Templates; }
        }

        public override string AddNewItemActionCode
        {
            get
            {
                return C.ActionCode.AddNewPageTemplate;
            }
        }

        public override string AddNewItemText
        {
            get
            {
                return TemplateStrings.AddNewTemplate;
            }
        }		
    }
}