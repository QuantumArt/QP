using System.Collections.Generic;
using Quantumart.QP8.BLL.ListItems;
using Quantumart.QP8.BLL.Services.DTO;
using Quantumart.QP8.WebMvc.ViewModels.Abstract;

namespace Quantumart.QP8.WebMvc.ViewModels.PageTemplate
{
    public class ObjectFormatVersionListViewModel : ListViewModel
    {
        private bool IsTemplateObjectFormats { get; set; }

        public IEnumerable<ObjectFormatVersionListItem> Data { get; set; }

        public string GettingDataActionName => IsTemplateObjectFormats ? "_IndexTemplateObjectFormatVersions" : "_IndexPageObjectFormatVersions";

        public override string EntityTypeCode => IsTemplateObjectFormats ? Constants.EntityTypeCode.TemplateObjectFormatVersion : Constants.EntityTypeCode.PageObjectFormatVersion;

        public override string ActionCode => IsTemplateObjectFormats ? Constants.ActionCode.TemplateObjectFormatVersions : Constants.ActionCode.PageObjectFormatVersions;

        public override string AddNewItemText => string.Empty;

        public override string AddNewItemActionCode => IsTemplateObjectFormats ? Constants.ActionCode.AddNewTemplateObjectFormatVersion : Constants.ActionCode.AddNewPageObjectFormatVersion;

        public static ObjectFormatVersionListViewModel Create(InitListResult result, string tabId, int parentId, bool isTemplateObjectFormats)
        {
            var model = Create<ObjectFormatVersionListViewModel>(tabId, parentId);
            model.IsTemplateObjectFormats = isTemplateObjectFormats;
            return model;
        }
    }
}
