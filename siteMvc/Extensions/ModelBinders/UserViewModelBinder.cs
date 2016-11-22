using System.Web.Mvc;
using Quantumart.QP8.WebMvc.ViewModels.User;

namespace Quantumart.QP8.WebMvc.Extensions.ModelBinders
{
    public class UserViewModelBinder : QpModelBinder
    {
        protected override void OnModelUpdated(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {
            var model = bindingContext.Model as UserViewModel;
            model.DoCustomBinding();
            base.OnModelUpdated(controllerContext, bindingContext);
        }
    }
}
