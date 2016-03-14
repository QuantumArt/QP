using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Quantumart.QP8.WebMvc.Extensions.Helpers;

namespace Quantumart.QP8.WebMvc.ViewModels.Audit
{
	public sealed class SucessfullSessionsAreaViewModel : AreaViewModel
	{
		#region creation

		public static SucessfullSessionsAreaViewModel Create(string tabId, int parentId)
		{
			SucessfullSessionsAreaViewModel model = ViewModel.Create<SucessfullSessionsAreaViewModel>(tabId, parentId);
			return model;
		}

		#endregion

		public override string EntityTypeCode
		{
			get { return Constants.EntityTypeCode.CustomerCode; }
		}

		public override string ActionCode
		{
			get { return Constants.ActionCode.SuccessfulSession; }
		}


		public string GridElementId
		{
			get
			{
				return UniqueId("Grid");
			}
		}
	}	
}