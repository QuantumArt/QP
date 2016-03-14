using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Quantumart.QP8.Constants;
using Quantumart.QP8.BLL;
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
			ActionPermissionsTreeViewModel model = ViewModel.Create<ActionPermissionsTreeViewModel>(tabId, 0);
			return model;
		}

		public override MainComponentType MainComponentType { get { return MainComponentType.ActionPermissionView; } }

		public override string MainComponentId { get { return UniqueId("ActionPermissionView"); } }

		public override string EntityTypeCode { get { return Constants.EntityTypeCode.CustomerCode; } }

		public override string ActionCode { get { return Constants.ActionCode.ActionPermissionTree; } }


		#region IUserAndGroupSearchBlockViewModel Members

		public IEnumerable<ListItem> GetMemberTypes()
		{
			return new[]
			{
				new ListItem(EntityPermission.GROUP_MEMBER_TYPE, EntityPermissionStrings.Group, "GroupMemberPanel"),
				new ListItem(EntityPermission.USER_MEMBER_TYPE, EntityPermissionStrings.User, "UserMemberPanel")			
			};
		}

		public int MemberType { get { return EntityPermission.GROUP_MEMBER_TYPE; } }

		#endregion
	}
}