using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Quantumart.QP8.BLL.Services;
using C = Quantumart.QP8.Constants;
using Quantumart.QP8.BLL;
using Quantumart.QP8.Validators;
using Quantumart.QP8.Resources;

namespace Quantumart.QP8.WebMvc.ViewModels
{
	public class UserGroupViewModel : EntityViewModel
	{
		IUserGroupService service;

		public new UserGroup Data
		{
			get
			{
				return (UserGroup)EntityData;
			}
			set
			{
				EntityData = value;
			}
		}

		public static UserGroupViewModel Create(UserGroup group, string tabId, int parentId, IUserGroupService service)
		{
			UserGroupViewModel model = EntityViewModel.Create<UserGroupViewModel>(group, tabId, parentId);
			model.service = service;
			model.Init();
			return model;
		}

		#region overrides
		public override string EntityTypeCode
		{
			get { return C.EntityTypeCode.UserGroup; }
		}

		public override string ActionCode
		{
			get { return IsNew ? C.ActionCode.AddNewUserGroup : C.ActionCode.UserGroupProperties; }
		} 

		#endregion


		private void Init()
		{
			if (IsNew)
			{
				Data.Users = Enumerable.Empty<User>();
			}

			BindedUserIDs = Data.Users.Select(u => u.Id).ToArray();
			ParentGroupId = Data.ParentGroup != null ? Data.ParentGroup.Id : 0;
		}

		internal void DoCustomBinding()
		{
			Data.Users = BindedUserIDs.Any() ? service.GetUsers(BindedUserIDs) : Enumerable.Empty<User>();
			Data.ParentGroup = ParentGroupId.HasValue ? service.Read(ParentGroupId.Value) : null;
		}

		#region properties
		[LocalizedDisplayName("BindedUserIDs", NameResourceType = typeof(UserGroupStrings))]
		public IEnumerable<int> BindedUserIDs { get; set; }

		[LocalizedDisplayName("ParentGroup", NameResourceType = typeof(UserGroupStrings))]
		public int? ParentGroupId { get; set; }
		#endregion

		#region List Items Gettrs
		public IEnumerable<ListItem> BindedUserListItems
		{
			get
			{
				return Data.Users
					.Select(u => new ListItem(u.Id.ToString(), u.Name))
					.ToArray();
			}
		}

		public IEnumerable<ListItem> GetGroupList()
		{
			return new[] {new ListItem("", UserGroupStrings.SelectParentGroup)}
				.Concat(service.GetAllGroups()
					.Where(g => g.Id != Data.Id && !g.UseParallelWorkflow)
					.Select(g => new ListItem(g.Id.ToString(), g.Name))
				).ToArray();
		}
		#endregion
	}
}