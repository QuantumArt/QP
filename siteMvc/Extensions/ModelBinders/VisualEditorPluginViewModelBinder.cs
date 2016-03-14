using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Quantumart.QP8.WebMvc.ViewModels.VisualEditor;

namespace Quantumart.QP8.WebMvc.Extensions.ModelBinders
{
	public class VisualEditorPluginViewModelBinder : QpModelBinder
	{
		protected override void OnModelUpdated(System.Web.Mvc.ControllerContext controllerContext, System.Web.Mvc.ModelBindingContext bindingContext)
		{
			VisualEditorPluginViewModel model = bindingContext.Model as VisualEditorPluginViewModel;
			model.DoCustomBinding();
			base.OnModelUpdated(controllerContext, bindingContext);
		}
	}
}