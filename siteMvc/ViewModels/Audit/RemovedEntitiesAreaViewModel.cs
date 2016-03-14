using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Quantumart.QP8.WebMvc.Extensions.Helpers;

namespace Quantumart.QP8.WebMvc.ViewModels.Audit
{
	public sealed class RemovedEntitiesAreaViewModel : AreaViewModel
	{		
		#region creation

		public static RemovedEntitiesAreaViewModel Create(string tabId, int parentId)
		{
			RemovedEntitiesAreaViewModel model = ViewModel.Create<RemovedEntitiesAreaViewModel>(tabId, parentId);						
			return model;
		}

		#endregion

		public override string EntityTypeCode
		{
			get {return Constants.EntityTypeCode.CustomerCode; }			
		}

		public override string ActionCode
		{
			get { return Constants.ActionCode.RemovedEntities; }
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