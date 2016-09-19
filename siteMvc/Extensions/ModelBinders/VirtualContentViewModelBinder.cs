using System.Diagnostics.CodeAnalysis;
using Quantumart.QP8.WebMvc.ViewModels.VirtualContent;

namespace Quantumart.QP8.WebMvc.Extensions.ModelBinders
{
    public class VirtualContentViewModelBinder : QpModelBinder
    {
        [SuppressMessage("ReSharper", "PossibleNullReferenceException")]
        protected override void OnModelUpdated(System.Web.Mvc.ControllerContext controllerContext, System.Web.Mvc.ModelBindingContext bindingContext)
        {
            base.OnModelUpdated(controllerContext, bindingContext);
            var model = bindingContext.Model as VirtualContentViewModel;
            model.Update();
        }
    }
}
