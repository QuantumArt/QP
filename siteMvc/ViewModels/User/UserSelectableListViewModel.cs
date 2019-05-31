using System.Collections.Generic;
using Quantumart.QP8.BLL.ListItems;
using Quantumart.QP8.WebMvc.ViewModels.Abstract;

namespace Quantumart.QP8.WebMvc.ViewModels.User
{
    public class UserSelectableListViewModel : ListViewModel
    {
        public IEnumerable<UserListItem> Data { get; set; }

        public string GettingDataActionName => IsMultiple ? "_MultipleSelect" : "_Select";

        public static UserSelectableListViewModel Create(string tabId, int parentId, int[] ids, bool isMultiple)
        {
            var model = Create<UserSelectableListViewModel>(tabId, parentId);
            model.SelectedIDs = ids;
            model.AutoGenerateLink = false;
            model.IsMultiple = isMultiple;
            model.TitleFieldName = "Login";
            return model;
        }

        public bool IsMultiple { get; private set; }

        public override string EntityTypeCode => Constants.EntityTypeCode.User;

        public override string ActionCode => IsMultiple ? Constants.ActionCode.MultipleSelectUser : Constants.ActionCode.SelectUser;
    }
}
