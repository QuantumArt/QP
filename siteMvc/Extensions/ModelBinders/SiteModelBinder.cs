using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Quantumart.QP8.BLL;
using System.Web.Mvc;

namespace Quantumart.QP8.WebMvc.Extensions.ModelBinders
{
    public class SiteModelBinder : QpModelBinder
    {
        protected override void OnModelUpdated(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {
            Site site = (bindingContext.Model as Site);
            site.DoCustomBinding();
            base.OnModelUpdated(controllerContext, bindingContext);
        }
    }
}
