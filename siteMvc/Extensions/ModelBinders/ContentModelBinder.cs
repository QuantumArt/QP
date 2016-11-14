using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Web.Mvc;
using Quantumart.QP8.BLL;
using Quantumart.QP8.Utils;

namespace Quantumart.QP8.WebMvc.Extensions.ModelBinders
{
    public class ContentModelBinder : QpModelBinder
    {
        public override object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {
            var content = base.BindModel(controllerContext, bindingContext) as Content;
            if (!content.UseVersionControl)
            {
                bindingContext.ModelState.Remove(GetModelPropertyName(bindingContext, () => content.MaxNumOfStoredVersions));
            }

            return content;
        }

        protected override void BindProperty(ControllerContext controllerContext, ModelBindingContext bindingContext, PropertyDescriptor propertyDescriptor)
        {
            var model = bindingContext.Model as Content;
            Expression<Func<IEnumerable<int>>> p = () => model.UnionSourceContentIDs;
            if (propertyDescriptor.Name == p.GetPropertyName())
            {
                var value = controllerContext.HttpContext.Request[GetModelPropertyName(bindingContext, p)];
                model.UnionSourceContentIDs = Converter.ToIdArray(value);
            }
            else
            {
                base.BindProperty(controllerContext, bindingContext, propertyDescriptor);
            }
        }

        protected override void OnModelUpdated(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {
            var content = bindingContext.Model as Content;
            content.DoCustomBinding();
            base.OnModelUpdated(controllerContext, bindingContext);
        }
    }
}
