using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using C = Quantumart.QP8.Constants;
using Quantumart.QP8.Resources;
using Quantumart.QP8.BLL.Services.DTO;

namespace Quantumart.QP8.WebMvc.ViewModels
{
	public class UserListViewModel : ListViewModel
	{
		public IEnumerable<UserListItem> Data { get; set; }

		public string GettingDataActionName
		{
			get
			{
				return "_Index";
			}
		}

		public static UserListViewModel Create(UserInitListResult result, string tabId, int parentId)
		{
			var model = ViewModel.Create<UserListViewModel>(tabId, parentId);
			model.ShowAddNewItemButton = result.IsAddNewAccessable && !model.IsWindow;
			return model;
		}

		public override string EntityTypeCode
		{
			get { return C.EntityTypeCode.User; }
		}

		public override string ActionCode
		{
			get { return C.ActionCode.Users; }
		}

		public override string AddNewItemActionCode
		{
			get
			{
				return C.ActionCode.AddNewUser;
			}
		}

		public override string AddNewItemText
		{
			get
			{
				return UserStrings.AddNewUser;
			}
		}
	}
}