using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Quantumart.QP8.Utils;
using Quantumart.QP8.WebMvc.ViewModels.Field;

namespace Quantumart.QP8.WebMvc.Extensions.ModelBinders
{
    public class FieldViewModelBinder : QpModelBinder
    {
        protected override void BindProperty(System.Web.Mvc.ControllerContext controllerContext, System.Web.Mvc.ModelBindingContext bindingContext, System.ComponentModel.PropertyDescriptor propertyDescriptor)
        {
            var model = bindingContext.Model as FieldViewModel;
            Expression<Func<IEnumerable<int>>> p = () => model.DefaultArticleIds;
            if (propertyDescriptor.Name == p.GetPropertyName())
            {
                var value = controllerContext.HttpContext.Request[GetModelPropertyName(bindingContext, p)];
                model.DefaultArticleIds = Converter.ToIdArray(value);
            }
            else
            {
                base.BindProperty(controllerContext, bindingContext, propertyDescriptor);
            }
        }

        protected override void OnModelUpdated(System.Web.Mvc.ControllerContext controllerContext, System.Web.Mvc.ModelBindingContext bindingContext)
        {
            base.OnModelUpdated(controllerContext, bindingContext);
            var model = bindingContext.Model as FieldViewModel;
            model.DoCustomBinding();
            model.Update();
        }
    }
}
