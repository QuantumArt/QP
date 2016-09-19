using System.Collections.Generic;
using Quantumart.QP8.BLL.ListItems;
using Quantumart.QP8.BLL.Services.DTO;
using Quantumart.QP8.Resources;

namespace Quantumart.QP8.WebMvc.ViewModels
{
    public class SiteListViewModel : ListViewModel
    {
        public List<SiteListItem> Data { get; set; }

        public string GettingDataActionName => IsSelect ? "_MultipleSelect" : "_Index";

        public static SiteListViewModel Create(SiteInitListResult result, string tabId, int parentId, bool isSelect = false, int[] ids = null)
        {
            var model = Create<SiteListViewModel>(tabId, parentId);
            model.IsSelect = isSelect;
            if (isSelect)
            {
                model.AutoGenerateLink = false;
            }

            model.SelectedIDs = ids;
            model.ShowAddNewItemButton = result.IsAddNewAccessable && !model.IsWindow;
            return model;
        }

        public override string EntityTypeCode => Constants.EntityTypeCode.Site;

        public override string ActionCode => IsSelect ? Constants.ActionCode.MultipleSelectSites : Constants.ActionCode.Sites;

        public override string AddNewItemText => SiteStrings.Link_AddNewSite;

        public override string AddNewItemActionCode => Constants.ActionCode.AddNewSite;

        public override bool IsReadOnly => base.IsReadOnly || IsSelect;
    }
}
