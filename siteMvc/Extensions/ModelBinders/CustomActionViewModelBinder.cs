using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Web.Mvc;
using Quantumart.QP8.Utils;
using Quantumart.QP8.WebMvc.ViewModels.CustomAction;

namespace Quantumart.QP8.WebMvc.Extensions.ModelBinders
{
    public class CustomActionViewModelBinder : QpModelBinder
    {
        protected override void BindProperty(System.Web.Mvc.ControllerContext controllerContext, System.Web.Mvc.ModelBindingContext bindingContext, System.ComponentModel.PropertyDescriptor propertyDescriptor)
        {
            var model = bindingContext.Model as CustomActionViewModel;
            Expression<Func<IEnumerable<int>>> siteIDs = () => model.SelectedSiteIDs;
            Expression<Func<IEnumerable<int>>> contentIDs = () => model.SelectedContentIDs;

            if (propertyDescriptor.Name == siteIDs.GetPropertyName())
            {
                var value = controllerContext.HttpContext.Request[GetModelPropertyName(bindingContext, siteIDs)];
                model.SelectedSiteIDs = Converter.ToIdArray(value);
            }
            else if (propertyDescriptor.Name == contentIDs.GetPropertyName())
            {
                var value = controllerContext.HttpContext.Request[GetModelPropertyName(bindingContext, contentIDs)];
                model.SelectedContentIDs = Converter.ToIdArray(value);
            }
            else
            {
                base.BindProperty(controllerContext, bindingContext, propertyDescriptor);
            }
        }

        protected override void OnModelUpdated(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {
            var model = bindingContext.Model as CustomActionViewModel;
            model.DoCustomBinding();
            base.OnModelUpdated(controllerContext, bindingContext);
        }
    }
}
