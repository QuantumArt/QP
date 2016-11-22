using System.Collections.Generic;
using Quantumart.QP8.BLL.ListItems;
using Quantumart.QP8.BLL.Services.DTO;
using Quantumart.QP8.Resources;

namespace Quantumart.QP8.WebMvc.ViewModels
{
    public class ContentListViewModel : ListViewModel
    {
        public List<ContentListItem> Data { get; set; }

        public string ParentName { get; set; }

        public string GroupName { get; set; }

        public static ContentListViewModel Create(ContentInitListResult result, string tabId, int parentId)
        {
            var model = Create<ContentListViewModel>(tabId, parentId);
            model.AllowMultipleEntitySelection = false;
            model.ParentName = result.ParentName;
            model.IsVirtual = result.IsVirtual;
            model.ShowAddNewItemButton = result.IsAddNewAccessable && !model.IsWindow;
            return model;
        }

        public override string EntityTypeCode => IsVirtual ? Constants.EntityTypeCode.VirtualContent : Constants.EntityTypeCode.Content;

        public override string ActionCode => !IsVirtual ? Constants.ActionCode.Contents : Constants.ActionCode.VirtualContents;

        public override string ContextMenuCode => IsVirtual ? Constants.EntityTypeCode.VirtualContent : Constants.EntityTypeCode.Content;

        public string GetDataAction => IsVirtual ? "_VirtualIndex" : "_Index";

        public override string AddNewItemText => ContentStrings.Link_AddNewContent;

        public override string AddNewItemActionCode => Constants.ActionCode.AddNewContent;
    }
}
