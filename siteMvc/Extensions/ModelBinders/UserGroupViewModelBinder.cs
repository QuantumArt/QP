using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Web.Mvc;
using Quantumart.QP8.Utils;
using Quantumart.QP8.WebMvc.ViewModels.UserGroup;

namespace Quantumart.QP8.WebMvc.Extensions.ModelBinders
{
    public class UserGroupViewModelBinder : QpModelBinder
    {
        protected override void BindProperty(ControllerContext controllerContext, ModelBindingContext bindingContext, PropertyDescriptor propertyDescriptor)
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

        protected override void OnModelUpdated(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {
            var model = bindingContext.Model as UserGroupViewModel;
            model.DoCustomBinding();
            base.OnModelUpdated(controllerContext, bindingContext);
        }
    }
}
