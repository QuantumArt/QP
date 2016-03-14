using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Quantumart.QP8.BLL;
using System.Globalization;
using Quantumart.QP8.Utils;
using Quantumart.QP8.Resources;
using System.Linq.Expressions;
using System.ComponentModel;

namespace Quantumart.QP8.WebMvc.Extensions.ModelBinders
{
	public class RecurringScheduleModelBinder : QpModelBinder
	{
		public override object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
		{			
			RecurringSchedule item = base.BindModel(controllerContext, bindingContext) as RecurringSchedule;
							
			// Для тех полей который скрыты - удалить сообщения об ошибках форматирования
			if (item.RepetitionNoEnd)
				bindingContext.ModelState.Remove(GetModelPropertyName(bindingContext, () => item.RepetitionEndDate));
			if(item.ShowLimitationType != Constants.ShowLimitationType.EndTime)
				bindingContext.ModelState.Remove(GetModelPropertyName(bindingContext, () => item.ShowEndTime));
			
			return item;				
		}					
	}
}