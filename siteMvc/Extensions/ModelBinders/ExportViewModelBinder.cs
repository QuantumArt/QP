using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Web.Mvc;
using Quantumart.QP8.Utils;
using Quantumart.QP8.WebMvc.ViewModels;
using Quantumart.QP8.WebMvc.ViewModels.MultistepSettings;

namespace Quantumart.QP8.WebMvc.Extensions.ModelBinders
{
    public class ExportViewModelBinder : QpModelBinder
    {
        protected override void BindProperty(ControllerContext controllerContext, ModelBindingContext bindingContext, PropertyDescriptor propertyDescriptor)
        {
            var model = bindingContext.Model as ExportViewModel;
            Expression<Func<IEnumerable<int>>> fieldsToExpand = () => model.FieldsToExpand;
            Expression<Func<IEnumerable<int>>> customFields = () => model.CustomFields;

            if (propertyDescriptor.Name == fieldsToExpand.GetPropertyName())
            {
                var value = controllerContext.HttpContext.Request[GetModelPropertyName(bindingContext, fieldsToExpand)];
                model.FieldsToExpand = Converter.ToIdArray(value);
            }
            else if (propertyDescriptor.Name == customFields.GetPropertyName())
            {
                var value = controllerContext.HttpContext.Request[GetModelPropertyName(bindingContext, customFields)];
                model.CustomFields = Converter.ToIdArray(value);
            }
            else
            {
                base.BindProperty(controllerContext, bindingContext, propertyDescriptor);
            }
        }
    }
}
