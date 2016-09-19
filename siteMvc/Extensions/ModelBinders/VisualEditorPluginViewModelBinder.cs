using System.Diagnostics.CodeAnalysis;
using System.Web.Mvc;
using Quantumart.QP8.WebMvc.ViewModels.VisualEditor;

namespace Quantumart.QP8.WebMvc.Extensions.ModelBinders
{
    public class VisualEditorPluginViewModelBinder : QpModelBinder
    {
        [SuppressMessage("ReSharper", "PossibleNullReferenceException")]
        protected override void OnModelUpdated(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {
            var model = bindingContext.Model as VisualEditorPluginViewModel;
            model.DoCustomBinding();
            base.OnModelUpdated(controllerContext, bindingContext);
        }
    }
}
