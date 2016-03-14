using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using C = Quantumart.QP8.Constants;
using Quantumart.QP8.Resources;
using Quantumart.QP8.BLL.Services.DTO;

namespace Quantumart.QP8.WebMvc.ViewModels
{
	public class UserSelectableListViewModel : ListViewModel
	{
		public IEnumerable<UserListItem> Data { get; set; }
		

		public string GettingDataActionName
		{
			get
			{
				return IsMultiple ? "_MultipleSelect" : "_Select";
			}
		}

		public static UserSelectableListViewModel Create(string tabId, int parentId, int[] IDs, bool isMultiple)
		{
			UserSelectableListViewModel model = ViewModel.Create<UserSelectableListViewModel>(tabId, parentId);
			model.SelectedIDs = IDs;			
			model.AutoGenerateLink = false;
			model.IsMultiple = isMultiple;
			model.TitleFieldName = "Login";
			return model;
		}

		public bool IsMultiple { get; private set; }

		public override string EntityTypeCode
		{
			get { return C.EntityTypeCode.User; }
		}

		public override string ActionCode
		{
			get { return IsMultiple ? C.ActionCode.MultipleSelectUser : C.ActionCode.SelectUser; }
		}

	}
}