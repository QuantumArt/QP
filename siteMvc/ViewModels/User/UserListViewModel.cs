using System.Collections.Generic;
using Quantumart.QP8.BLL.ListItems;
using Quantumart.QP8.BLL.Services.DTO;
using Quantumart.QP8.Resources;
using Quantumart.QP8.WebMvc.ViewModels.Abstract;

namespace Quantumart.QP8.WebMvc.ViewModels.User
{
    public class UserListViewModel : ListViewModel
    {
        public IEnumerable<UserListItem> Data { get; set; }

        public string GettingDataActionName => "_Index";

        public static UserListViewModel Create(InitListResult result, string tabId, int parentId)
        {
            var model = Create<UserListViewModel>(tabId, parentId);
            model.ShowAddNewItemButton = result.IsAddNewAccessable && !model.IsWindow;
            model.AllowMultipleEntitySelection = false;
            return model;
        }

        public override string EntityTypeCode => Constants.EntityTypeCode.User;

        public override string ActionCode => Constants.ActionCode.Users;

        public override string AddNewItemActionCode => Constants.ActionCode.AddNewUser;

        public override string AddNewItemText => UserStrings.AddNewUser;
    }
}
