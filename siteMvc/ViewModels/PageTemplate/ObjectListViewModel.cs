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
    public class ObjectListViewModel : ListViewModel
    {
        public bool IsTemplateObject { get; set; }//else it`s page object

        public IEnumerable<ObjectListItem> Data { get; set; }

        public string GettingDataActionName
        {
            get
            {
                return IsTemplateObject ? "_IndexTemplateObjects" : "_IndexPageObjects";
            }
        }

        public static ObjectListViewModel Create(ObjectInitListResult result, string tabId, int parentId, bool isTemplateObject)
        {
            var model = ViewModel.Create<ObjectListViewModel>(tabId, parentId);
            model.IsTemplateObject = isTemplateObject;
			model.ShowAddNewItemButton = result.IsAddNewAccessable && !model.IsWindow;			
            return model;
        }

        public override string EntityTypeCode
        {
            get { return IsTemplateObject ? C.EntityTypeCode.TemplateObject : C.EntityTypeCode.PageObject; }
        }

        public override string ActionCode
        {
			get { return IsTemplateObject ? C.ActionCode.TemplateObjects : C.ActionCode.PageObjects; }
        }

        public override string AddNewItemActionCode
        {
            get
            {
                return IsTemplateObject ? C.ActionCode.AddNewTemplateObject : C.ActionCode.AddNewPageObject;
            }
        }

        public override string AddNewItemText
        {
            get
            {
                return TemplateStrings.AddNewObject;
            }
        }
    }
}