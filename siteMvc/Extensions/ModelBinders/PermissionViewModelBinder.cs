using System.Web.Mvc;
using Quantumart.QP8.WebMvc.ViewModels.EntityPermissions;

namespace Quantumart.QP8.WebMvc.Extensions.ModelBinders
{
    public class PermissionViewModelBinder : QpModelBinder
    {
        protected override void OnModelUpdated(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {
            var model = bindingContext.Model as PermissionViewModel;
            model.DoCustomBinding();
            base.OnModelUpdated(controllerContext, bindingContext);
        }
    }
}
