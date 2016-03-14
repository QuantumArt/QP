using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Quantumart.QP8.BLL.Services.DTO;
using Quantumart.QP8.WebMvc.ViewModels.Content;
using C = Quantumart.QP8.Constants;

namespace Quantumart.QP8.WebMvc.ViewModels.CustomAction
{
	public class CustomActionContentViewModel : ContentSelectableListViewModel
	{
		public CustomActionContentViewModel(ContentInitListResult result, string tabId, int parentId, int[] IDs) : base(result, tabId, parentId, IDs) { }

		#region read-only members

		public override string ActionCode
		{
			get
			{
				return C.ActionCode.MultipleSelectContentForCustomAction;
			}
		}

		public override string GetDataAction
		{
			get
			{
				return "_MultipleSelectForCustomAction";
			}
		}

		#endregion
	}
}