using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Quantumart.QP8.WebMvc.ViewModels;

namespace Quantumart.QP8.WebMvc.Extensions.ModelBinders
{
	public class SiteViewModelBinder : QpModelBinder
	{
		protected override void OnModelUpdated(System.Web.Mvc.ControllerContext controllerContext, System.Web.Mvc.ModelBindingContext bindingContext)
		{
			SiteViewModel model = bindingContext.Model as SiteViewModel;
			model.DoCustomBinding();
			base.OnModelUpdated(controllerContext, bindingContext);
		}
	}
}