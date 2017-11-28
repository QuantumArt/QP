using System.Collections.Generic;
using Quantumart.QP8.BLL.ListItems;
using Quantumart.QP8.BLL.Services.DTO;
using Quantumart.QP8.WebMvc.ViewModels.Abstract;

namespace Quantumart.QP8.WebMvc.ViewModels.PageTemplate
{
    public class PageSelectableListViewModel : ListViewModel
    {
        public List<PageListItem> Data { get; set; }

        public string ParentName { get; set; }

        public string GroupName { get; set; }

        public PageSelectableListViewModel(PageInitListResult result, string tabId, int parentId, int[] ids)
        {
            ParentEntityId = parentId;
            TabId = tabId;
            ParentName = result.ParentName;
            SelectedIDs = ids;
            AutoGenerateLink = false;
            ShowAddNewItemButton = !IsWindow;
        }

        public sealed override string EntityTypeCode => Constants.EntityTypeCode.Page;

        public override string ActionCode => Constants.ActionCode.SelectPageForObjectForm;

        public virtual string GetDataAction => "_SelectPages";
    }
}
