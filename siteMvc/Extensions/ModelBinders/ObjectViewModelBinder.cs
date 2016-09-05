using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using Quantumart.QP8.Utils;
using Quantumart.QP8.WebMvc.ViewModels.PageTemplate;

namespace Quantumart.QP8.WebMvc.Extensions.ModelBinders
{
    public class ObjectViewModelBinder : QpModelBinder
    {
        [SuppressMessage("ReSharper", "PossibleNullReferenceException")]
        protected override void BindProperty(System.Web.Mvc.ControllerContext controllerContext, System.Web.Mvc.ModelBindingContext bindingContext, System.ComponentModel.PropertyDescriptor propertyDescriptor)
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

        [SuppressMessage("ReSharper", "PossibleNullReferenceException")]
        protected override void OnModelUpdated(System.Web.Mvc.ControllerContext controllerContext, System.Web.Mvc.ModelBindingContext bindingContext)
        {
            var model = bindingContext.Model as ObjectViewModel;
            model.DoCustomBinding();
            base.OnModelUpdated(controllerContext, bindingContext);
        }
    }
}
