using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using Quantumart.QP8.Utils;
using Quantumart.QP8.WebMvc.ViewModels;

namespace Quantumart.QP8.WebMvc.Extensions.ModelBinders
{
    public class UserGroupViewModelBinder : QpModelBinder
    {
        [SuppressMessage("ReSharper", "PossibleNullReferenceException")]
        protected override void BindProperty(System.Web.Mvc.ControllerContext controllerContext, System.Web.Mvc.ModelBindingContext bindingContext, System.ComponentModel.PropertyDescriptor propertyDescriptor)
        {
            var model = bindingContext.Model as UserGroupViewModel;
            Expression<Func<IEnumerable<int>>> bindedUserIDs = () => model.BindedUserIDs;

            if (propertyDescriptor.Name == bindedUserIDs.GetPropertyName())
            {
                var value = controllerContext.HttpContext.Request[GetModelPropertyName(bindingContext, bindedUserIDs)];
                model.BindedUserIDs = Converter.ToIdArray(value);
            }
            else
            {
                base.BindProperty(controllerContext, bindingContext, propertyDescriptor);
            }
        }
        [SuppressMessage("ReSharper", "PossibleNullReferenceException")]
        protected override void OnModelUpdated(System.Web.Mvc.ControllerContext controllerContext, System.Web.Mvc.ModelBindingContext bindingContext)
        {
            var model = bindingContext.Model as UserGroupViewModel;
            model.DoCustomBinding();
            base.OnModelUpdated(controllerContext, bindingContext);
        }
    }
}
