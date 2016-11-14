using Quantumart.QP8.WebMvc.ViewModels.EntityPermissions;

namespace Quantumart.QP8.WebMvc.Extensions.ModelBinders
{
    public class PermissionViewModelBinder : QpModelBinder
    {
        protected override void OnModelUpdated(System.Web.Mvc.ControllerContext controllerContext, System.Web.Mvc.ModelBindingContext bindingContext)
        {
            var model = bindingContext.Model as PermissionViewModel;
            model.DoCustomBinding();
            base.OnModelUpdated(controllerContext, bindingContext);
        }
    }
}
