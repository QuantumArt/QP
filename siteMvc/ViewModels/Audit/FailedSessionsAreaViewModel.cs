using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Quantumart.QP8.WebMvc.Extensions.Helpers;

namespace Quantumart.QP8.WebMvc.ViewModels.Audit
{
	public sealed class FailedSessionsAreaViewModel : AreaViewModel
	{
		#region creation

		public static FailedSessionsAreaViewModel Create(string tabId, int parentId)
		{
			FailedSessionsAreaViewModel model = ViewModel.Create<FailedSessionsAreaViewModel>(tabId, parentId);
			return model;
		}

		#endregion

		public override string EntityTypeCode
		{
			get { return Constants.EntityTypeCode.CustomerCode; }
		}

		public override string ActionCode
		{
			get { return Constants.ActionCode.FailedSession; }
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