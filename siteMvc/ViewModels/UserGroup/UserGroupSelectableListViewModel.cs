using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using C = Quantumart.QP8.Constants;
using Quantumart.QP8.Resources;
using Quantumart.QP8.BLL.Services.DTO;

namespace Quantumart.QP8.WebMvc.ViewModels
{
	public class UserGroupSelectableListViewModel : ListViewModel
	{
		public IEnumerable<UserGroupListItem> Data { get; set; }
		

		public string GettingDataActionName
		{
			get
			{
				return "_Select";
			}
		}

		public static UserGroupSelectableListViewModel Create(string tabId, int parentId, int[] IDs)
		{
			UserGroupSelectableListViewModel model = ViewModel.Create<UserGroupSelectableListViewModel>(tabId, parentId);
			model.SelectedIDs = IDs;
			model.AutoGenerateLink = false;
			model.ShowAddNewItemButton = !model.IsWindow;
			return model;
		}
		

		public override string EntityTypeCode
		{
			get { return C.EntityTypeCode.UserGroup; }
		}

		public override string ActionCode
		{
			get { return C.ActionCode.SelectUserGroup; }
		}

	}
}