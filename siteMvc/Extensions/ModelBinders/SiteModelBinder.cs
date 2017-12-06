using System.Web.Mvc;
using Quantumart.QP8.BLL;

namespace Quantumart.QP8.WebMvc.Extensions.ModelBinders
{
    public class SiteModelBinder : QpModelBinder
    {
        protected override void OnModelUpdated(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {
            var site = bindingContext.Model as Site;
            site.DoCustomBinding();
            base.OnModelUpdated(controllerContext, bindingContext);
        }
    }
}
