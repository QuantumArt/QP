using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using B = Quantumart.QP8.BLL;
using C = Quantumart.QP8.Constants;
using Quantumart.QP8.Resources;
using Quantumart.QP8.BLL.ListItems;
using Quantumart.QP8.BLL.Services.DTO;

namespace Quantumart.QP8.WebMvc.ViewModels.CustomAction
{
	public class CustomActionListViewModel : ListViewModel
	{
		public IEnumerable<CustomActionListItem> Data { get; set; }

		public string GettingDataActionName
		{
			get
			{
				return "_Index";
			}
		}

		#region creation

		public static CustomActionListViewModel Create(CustomActionInitListResult initList, string tabId, int parentId)
		{
			var model = ViewModel.Create<CustomActionListViewModel>(tabId, parentId);
			model.ShowAddNewItemButton = initList.IsAddNewAccessable && !model.IsWindow;
			return model;
		}

		#endregion
		
		#region overrides
		public override string EntityTypeCode
		{
			get { return C.EntityTypeCode.CustomAction; }
		}

		public override string ActionCode
		{
			get { return C.ActionCode.CustomActions; }
		}

		public override string AddNewItemActionCode
		{
			get
			{
				return C.ActionCode.AddNewCustomAction;
			}
		}

		public override string AddNewItemText
		{
			get
			{
				return CustomActionStrings.AddNewCustomAction;
			}
		} 
		#endregion
	}
}