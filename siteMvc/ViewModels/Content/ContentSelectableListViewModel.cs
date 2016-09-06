using System.Collections.Generic;
using Quantumart.QP8.BLL.ListItems;
using Quantumart.QP8.BLL.Services.DTO;

namespace Quantumart.QP8.WebMvc.ViewModels.Content
{
    public class ContentSelectableListViewModel : ListViewModel
    {
        public List<ContentListItem> Data { get; set; }

        public string ParentName { get; set; }

        public string GroupName { get; set; }

        public bool IsMultiple { get; set; }

        public ContentSelectableListViewModel(ContentInitListResult result, string tabId, int parentId, int[] ids)
        {
            ParentEntityId = parentId;
            TabId = tabId;
            ParentName = result.ParentName;
            SelectedIDs = ids;
            AutoGenerateLink = false;
            ShowAddNewItemButton = !IsWindow;
        }

        public sealed override string EntityTypeCode => Constants.EntityTypeCode.Content;

        public override bool AllowMultipleEntitySelection
        {
            get
            {
                return IsMultiple;
            }
            set
            {
            }
        }

        public override string ActionCode => IsMultiple ? Constants.ActionCode.MultipleSelectContent : Constants.ActionCode.SelectContent;

        public virtual string GetDataAction => IsMultiple ? "_MultipleSelect" : "_Select";
    }
}
