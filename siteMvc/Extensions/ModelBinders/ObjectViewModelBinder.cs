using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Web.Mvc;
using Quantumart.QP8.Utils;
using Quantumart.QP8.WebMvc.ViewModels.PageTemplate;

namespace Quantumart.QP8.WebMvc.Extensions.ModelBinders
{
    public class ObjectViewModelBinder : QpModelBinder
    {
        protected override void BindProperty(ControllerContext controllerContext, ModelBindingContext bindingContext, PropertyDescriptor propertyDescriptor)
        {
            var model = bindingContext.Model as ObjectViewModel;
            Expression<Func<IEnumerable<int>>> p = () => model.ActiveStatusTypeIds;
            if (propertyDescriptor.Name == p.GetPropertyName())
            {
                var value = controllerContext.HttpContext.Request[GetModelPropertyName(bindingContext, p)];
                model.ActiveStatusTypeIds = Converter.ToIdArray(value);
            }
            else
            {
                base.BindProperty(controllerContext, bindingContext, propertyDescriptor);
            }
        }

        protected override void OnModelUpdated(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {
            var model = bindingContext.Model as ObjectViewModel;
            model.DoCustomBinding();
            base.OnModelUpdated(controllerContext, bindingContext);
        }
    }
}
