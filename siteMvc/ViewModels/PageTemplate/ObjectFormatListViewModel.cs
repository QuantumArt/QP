using System.Collections.Generic;
using Quantumart.QP8.BLL.ListItems;
using Quantumart.QP8.BLL.Services.DTO;
using Quantumart.QP8.Resources;
using Quantumart.QP8.WebMvc.ViewModels.Abstract;

namespace Quantumart.QP8.WebMvc.ViewModels.PageTemplate
{
    public class ObjectFormatListViewModel : ListViewModel
    {
        private bool IsTemplateObjectFormats { get; set; }

        public IEnumerable<ObjectFormatListItem> Data { get; set; }

        public string GettingDataActionName => IsTemplateObjectFormats ? "_IndexTemplateObjectFormats" : "_IndexPageObjectFormats";

        public override string EntityTypeCode => IsTemplateObjectFormats ? Constants.EntityTypeCode.TemplateObjectFormat : Constants.EntityTypeCode.PageObjectFormat;

        public override string ActionCode => IsTemplateObjectFormats ? Constants.ActionCode.TemplateObjectFormats : Constants.ActionCode.PageObjectFormats;

        public override string AddNewItemActionCode => IsTemplateObjectFormats ? Constants.ActionCode.AddNewTemplateObjectFormat : Constants.ActionCode.AddNewPageObjectFormat;

        public override string AddNewItemText => TemplateStrings.AddNewFormat;

        public static ObjectFormatListViewModel Create(FormatInitListResult result, string tabId, int parentId, bool isTemplateObjectFormats)
        {
            var model = Create<ObjectFormatListViewModel>(tabId, parentId);
            model.IsTemplateObjectFormats = isTemplateObjectFormats;
            model.ShowAddNewItemButton = result.IsAddNewAccessable && !model.IsWindow;
            return model;
        }
    }
}
