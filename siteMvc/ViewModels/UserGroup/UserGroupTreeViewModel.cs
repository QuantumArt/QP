using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using C = Quantumart.QP8.Constants;
using Quantumart.QP8.Resources;
using Quantumart.QP8.BLL.Services.DTO;

namespace Quantumart.QP8.WebMvc.ViewModels
{
	public class UserGroupTreeViewModel : ListViewModel
	{

		public static UserGroupTreeViewModel Create(UserGroupInitTreeResult result, string tabId, int parentId)
		{
			UserGroupTreeViewModel model = ViewModel.Create<UserGroupTreeViewModel>(tabId, parentId);
			model.IsTree = true;
			model.AllowMultipleEntitySelection = false;
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