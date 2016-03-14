using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Quantumart.QP8.WebMvc.ViewModels.VirtualContent;
using Quantumart.QP8.Utils;
using System.Linq.Expressions;

namespace Quantumart.QP8.WebMvc.Extensions.ModelBinders
{
	public class VirtualContentViewModelBinder : QpModelBinder
	{
		protected override void OnModelUpdated(System.Web.Mvc.ControllerContext controllerContext, System.Web.Mvc.ModelBindingContext bindingContext)
		{
			base.OnModelUpdated(controllerContext, bindingContext);
			VirtualContentViewModel model = bindingContext.Model as VirtualContentViewModel;
			model.Update();			
		}
	}
}