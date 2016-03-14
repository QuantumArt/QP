using Quantumart.QP8.WebMvc.ViewModels;
using System.Web.Mvc;

namespace Quantumart.QP8.WebMvc.Extensions.ModelBinders
{
	public class DbViewModelBinder : QpModelBinder
	{
		protected override void OnModelUpdated(ControllerContext controllerContext, ModelBindingContext bindingContext)
		{
			DbViewModel model = bindingContext.Model as DbViewModel;
			model.DoCustomBinding();
			base.OnModelUpdated(controllerContext, bindingContext);
		}
	}
}