using System.Collections.Generic;
using Quantumart.QP8.BLL;
using Quantumart.QP8.Constants;
using Quantumart.QP8.Resources;
using Quantumart.QP8.WebMvc.ViewModels.Interfaces;

namespace Quantumart.QP8.WebMvc.ViewModels.ActionPermissions
{
    public class ActionPermissionsTreeViewModel : ViewModel, IUserAndGroupSearchBlockViewModel
    {
        public ActionPermissionsTreeViewModel()
        {
            IsViewChangable = false;
        }

        public static ActionPermissionsTreeViewModel Create(string tabId)
        {
            return Create<ActionPermissionsTreeViewModel>(tabId, 0);
        }

        public override MainComponentType MainComponentType => MainComponentType.ActionPermissionView;

        public override string MainComponentId => UniqueId("ActionPermissionView");

        public override string EntityTypeCode => Constants.EntityTypeCode.CustomerCode;

        public override string ActionCode => Constants.ActionCode.ActionPermissionTree;

        public IEnumerable<ListItem> GetMemberTypes()
        {
            return new[]
            {
				new ListItem(EntityPermission.GroupMemberType, EntityPermissionStrings.Group, "GroupMemberPanel"),
				new ListItem(EntityPermission.UserMemberType, EntityPermissionStrings.User, "UserMemberPanel")
            };
        }

		public int MemberType { get { return EntityPermission.GroupMemberType; } }
    }
}
