using System.Web.Mvc;
using Quantumart.QP8.WebMvc.ViewModels.Notification;

namespace Quantumart.QP8.WebMvc.Extensions.ModelBinders
{
    public class NotificationViewModelBinder : QpModelBinder
    {
        protected override void OnModelUpdated(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {
            var model = bindingContext.Model as NotificationViewModel;
            model.DoCustomBinding();
            base.OnModelUpdated(controllerContext, bindingContext);
        }
    }
}
