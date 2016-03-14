using Quantumart.QP8.WebMvc.ViewModels.Notification;

namespace Quantumart.QP8.WebMvc.Extensions.ModelBinders
{
	public class NotificationViewModelBinder : QpModelBinder
	{
		protected override void OnModelUpdated(System.Web.Mvc.ControllerContext controllerContext, System.Web.Mvc.ModelBindingContext bindingContext)
		{
			NotificationViewModel model = bindingContext.Model as NotificationViewModel;
			model.DoCustomBinding();
			base.OnModelUpdated(controllerContext, bindingContext);
		}
	}
}