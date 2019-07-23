using System.Collections.Generic;
using Quantumart.QP8.BLL.ListItems;
using Quantumart.QP8.WebMvc.ViewModels.Abstract;

namespace Quantumart.QP8.WebMvc.ViewModels.UserGroup
{
    public class UserGroupSelectableListViewModel : ListViewModel
    {
        public IEnumerable<UserGroupListItem> Data { get; set; }

        public string GettingDataActionName => "_Select";

        public static UserGroupSelectableListViewModel Create(string tabId, int parentId, int[] ids)
        {
            var model = Create<UserGroupSelectableListViewModel>(tabId, parentId);
            model.SelectedIDs = ids;
            model.AutoGenerateLink = false;
            model.ShowAddNewItemButton = !model.IsWindow;
            model.AllowMultipleEntitySelection = false;
            model.AllowGlobalSelection = false;
            return model;
        }

        public override string EntityTypeCode => Constants.EntityTypeCode.UserGroup;

        public override string ActionCode => Constants.ActionCode.SelectUserGroup;
    }
}
