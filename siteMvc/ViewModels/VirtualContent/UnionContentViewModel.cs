using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using B = Quantumart.QP8.BLL;
using Quantumart.QP8.Resources;
using C = Quantumart.QP8.Constants;
using Quantumart.QP8.BLL.Services.DTO;
using Quantumart.QP8.WebMvc.ViewModels.Content;


namespace Quantumart.QP8.WebMvc.ViewModels.VirtualContent
{
	public class UnionContentViewModel : ContentSelectableListViewModel
	{
		public UnionContentViewModel(ContentInitListResult result, string tabId, int parentId, int[] IDs) : base(result, tabId, parentId, IDs) { }

		#region read-only members		

		public override string ActionCode
		{
			get
			{
				return C.ActionCode.MultipleSelectContentForUnion;
			}
		}
		

		public override string GetDataAction
		{
			get
			{
				return "_MultipleSelectForUnion";
			}
		}
		
		#endregion

	}
}

