using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Quantumart.QP8.WebMvc.Extensions.Helpers;

namespace Quantumart.QP8.WebMvc.Extensions.ModelBinders
{
	public class QPCheckedItemListModelBinder : DefaultModelBinder
	{
		public override object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
		{
			IList<QPCheckedItem> result = base.BindModel(controllerContext, bindingContext) as IList<QPCheckedItem>;
			if (result != null)
				return result.Where(i => i != null).ToList();
			else
				return null;
		}
	}
}