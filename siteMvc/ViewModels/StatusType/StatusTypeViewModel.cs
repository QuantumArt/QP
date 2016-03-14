using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Quantumart.QP8.BLL;
using Quantumart.QP8.Resources;
using Quantumart.QP8.Validators;
using C = Quantumart.QP8.Constants;

namespace Quantumart.QP8.WebMvc.ViewModels.StatusType
{
	public class StatusTypeViewModel : EntityViewModel
	{
		public override string EntityTypeCode
		{
			get { return C.EntityTypeCode.StatusType; }
		}	

		public override string ActionCode
		{
			get 
			{ 
				if (this.IsNew)
                    return C.ActionCode.AddNewStatusType;
                else                
					return C.ActionCode.StatusTypeProperties; 
			}
		}		

		public new Quantumart.QP8.BLL.StatusType Data
		{
			get
			{
				return (Quantumart.QP8.BLL.StatusType)EntityData;
			}
			set
			{
				EntityData = value;
			}
		}

		public static StatusTypeViewModel Create(Quantumart.QP8.BLL.StatusType status, string tabId, int parentId)
		{
			var model = EntityViewModel.Create<StatusTypeViewModel>(status, tabId, parentId);
			return model;
		}

		public override void Validate(ModelStateDictionary modelState)
		{
			base.Validate(modelState);
		}
	}
}