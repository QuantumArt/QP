using Quantumart.QP8.WebMvc.ViewModels.VisualEditor;
using System.Web.Mvc;

namespace Quantumart.QP8.WebMvc.Extensions.ModelBinders
{
    public class VisualEditorPluginViewModelBinder : QpModelBinder
    {
        protected override void OnModelUpdated(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {
            var model = bindingContext.Model as VisualEditorPluginViewModel;
            model.DoCustomBinding();
            base.OnModelUpdated(controllerContext, bindingContext);
        }
    }
}
