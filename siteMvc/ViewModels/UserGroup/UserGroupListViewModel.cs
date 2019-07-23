using System.Collections.Generic;
using Quantumart.QP8.BLL.ListItems;
using Quantumart.QP8.BLL.Services.DTO;
using Quantumart.QP8.Resources;
using Quantumart.QP8.WebMvc.ViewModels.Abstract;
using C = Quantumart.QP8.Constants;

namespace Quantumart.QP8.WebMvc.ViewModels.UserGroup
{
    public class UserGroupListViewModel : ListViewModel
    {
        public IEnumerable<UserGroupListItem> Data { get; set; }

        public string GettingDataActionName => "_Index";

        public static UserGroupListViewModel Create(UserGroupInitListResult result, string tabId, int parentId)
        {
            var model = Create<UserGroupListViewModel>(tabId, parentId);
            model.ShowAddNewItemButton = result.IsAddNewAccessable && !model.IsWindow;
            model.AllowMultipleEntitySelection = false;
            return model;
        }

        public override string EntityTypeCode => C.EntityTypeCode.UserGroup;

        public override string ActionCode => C.ActionCode.UserGroups;

        public override string AddNewItemActionCode => C.ActionCode.AddNewUserGroup;

        public override string AddNewItemText => UserGroupStrings.AddNewGroup;
    }
}
