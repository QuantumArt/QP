using System.Collections.Generic;
using Quantumart.QP8.BLL.ListItems;
using Quantumart.QP8.BLL.Services.DTO;
using Quantumart.QP8.Resources;

namespace Quantumart.QP8.WebMvc.ViewModels.PageTemplate
{
    public class ObjectListViewModel : ListViewModel
    {
        public bool IsTemplateObject { get; set; }

        public IEnumerable<ObjectListItem> Data { get; set; }

        public string GettingDataActionName => IsTemplateObject ? "_IndexTemplateObjects" : "_IndexPageObjects";

        public static ObjectListViewModel Create(ObjectInitListResult result, string tabId, int parentId, bool isTemplateObject)
        {
            var model = Create<ObjectListViewModel>(tabId, parentId);
            model.IsTemplateObject = isTemplateObject;
            model.ShowAddNewItemButton = result.IsAddNewAccessable && !model.IsWindow;
            return model;
        }

        public override string EntityTypeCode => IsTemplateObject ? Constants.EntityTypeCode.TemplateObject : Constants.EntityTypeCode.PageObject;

        public override string ActionCode => IsTemplateObject ? Constants.ActionCode.TemplateObjects : Constants.ActionCode.PageObjects;

        public override string AddNewItemActionCode => IsTemplateObject ? Constants.ActionCode.AddNewTemplateObject : Constants.ActionCode.AddNewPageObject;

        public override string AddNewItemText => TemplateStrings.AddNewObject;
    }
}
