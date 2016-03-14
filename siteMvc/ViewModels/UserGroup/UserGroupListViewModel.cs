using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using C = Quantumart.QP8.Constants;
using Quantumart.QP8.Resources;
using Quantumart.QP8.BLL.Services.DTO;

namespace Quantumart.QP8.WebMvc.ViewModels
{
	public class UserGroupListViewModel : ListViewModel
	{
		public IEnumerable<UserGroupListItem> Data { get; set; }

		public string GettingDataActionName
		{
			get
			{
				return "_Index";
			}
		}

		public static UserGroupListViewModel Create(UserGroupInitListResult result, string tabId, int parentId)
		{
			var model = ViewModel.Create<UserGroupListViewModel>(tabId, parentId);
			model.ShowAddNewItemButton = result.IsAddNewAccessable && !model.IsWindow;
			return model;
		}

		public override string EntityTypeCode
		{
			get { return C.EntityTypeCode.UserGroup; }
		}

		public override string ActionCode
		{
			get { return C.ActionCode.UserGroups; }
		}

		public override string AddNewItemActionCode
		{
			get
			{
				return C.ActionCode.AddNewUserGroup;
			}
		}

		public override string AddNewItemText
		{
			get
			{
				return UserGroupStrings.AddNewGroup;
			}
		}
	}
}