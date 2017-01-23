using Quantumart.QP8.BLL.Services.DTO;
using Quantumart.QP8.Resources;
using Quantumart.QP8.WebMvc.ViewModels.Abstract;

namespace Quantumart.QP8.WebMvc.ViewModels
{
    public class UserGroupTreeViewModel : ListViewModel
    {
        public static UserGroupTreeViewModel Create(UserGroupInitTreeResult result, string tabId, int parentId)
        {
            var model = ViewModel.Create<UserGroupTreeViewModel>(tabId, parentId);
            model.IsTree = true;
            model.AllowMultipleEntitySelection = false;
            model.ShowAddNewItemButton = result.IsAddNewAccessable && !model.IsWindow;
            return model;
        }

        public override string EntityTypeCode => Constants.EntityTypeCode.UserGroup;

        public override string ActionCode => Constants.ActionCode.UserGroups;

        public override string AddNewItemActionCode => Constants.ActionCode.AddNewUserGroup;

        public override string AddNewItemText => UserGroupStrings.AddNewGroup;
    }
}
