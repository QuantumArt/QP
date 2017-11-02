using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Web.Mvc;
using Quantumart.QP8.Utils;
using Quantumart.QP8.WebMvc.ViewModels.Workflow;

namespace Quantumart.QP8.WebMvc.Extensions.ModelBinders
{
    public class WorkflowModelBinder : QpModelBinder
    {
        protected override void BindProperty(ControllerContext controllerContext, ModelBindingContext bindingContext, PropertyDescriptor propertyDescriptor)
        {
            var model = bindingContext.Model as WorkflowViewModel;
            Expression<Func<IEnumerable<int>>> p = () => model.ActiveContentsIds;
            if (propertyDescriptor.Name == p.GetPropertyName())
            {
                var value = controllerContext.HttpContext.Request[GetModelPropertyName(bindingContext, p)];
                model.ActiveContentsIds = Converter.ToIdArray(value);
            }
            else
            {
                base.BindProperty(controllerContext, bindingContext, propertyDescriptor);
            }
        }

        protected override void OnModelUpdated(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {
            var model = bindingContext.Model as WorkflowViewModel;
            model.DoCustomBinding();
            base.OnModelUpdated(controllerContext, bindingContext);
        }
    }
}
