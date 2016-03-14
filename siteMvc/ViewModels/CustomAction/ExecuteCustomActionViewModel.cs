using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Quantumart.QP8.BLL.Services;
using Quantumart.QP8.Constants;

namespace Quantumart.QP8.WebMvc.ViewModels.CustomAction
{
	public class ExecuteCustomActionViewModel : ViewModel
	{		
		public static ExecuteCustomActionViewModel Create(string tabId, int parentId, int[] IDs, BLL.CustomAction customAction)
		{
			ExecuteCustomActionViewModel model = ViewModel.Create<ExecuteCustomActionViewModel>(tabId, parentId);			 			
			model.CustomAction = customAction;			
			return model;
		}

		public BLL.CustomAction CustomAction {get; private set;}

		public string IFrameElementId { get { return UniqueId("caframe"); } }


		public override ExpandoObject MainComponentParameters
		{
			get
			{
				dynamic result = base.MainComponentParameters;
				result.actionBaseUrl = CustomAction.FullUrl;
				result.iframeElementId = IFrameElementId;				
				return result;
			}
		}

		public override MainComponentType MainComponentType
		{
			get { return MainComponentType.CustomActionHost; }
		}

		public override string MainComponentId
		{
			get { return UniqueId("CustomActionHost"); }
		}

		public override string EntityTypeCode
		{
			get { return CustomAction.Action.EntityType.Code; }
		}
		
		public override string ActionCode
		{
			get { return CustomAction.Action.Code; }
		}
	}
}