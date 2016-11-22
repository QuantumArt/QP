using System;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Web.Mvc;
using Quantumart.QP8.Resources;
using Quantumart.QP8.Utils;

namespace Quantumart.QP8.WebMvc.Extensions.ModelBinders
{
    public class QpModelBinder : DefaultModelBinder
    {
        protected override void SetProperty(ControllerContext controllerContext, ModelBindingContext bindingContext, PropertyDescriptor propertyDescriptor, object value)
        {
            if (propertyDescriptor.PropertyType == typeof(string))
            {
                var stringValue = (string)value;
                stringValue = !string.IsNullOrEmpty(stringValue) ? stringValue.Trim() : stringValue;
                value = string.IsNullOrEmpty(stringValue) ? null : stringValue;
            }

            base.SetProperty(controllerContext, bindingContext, propertyDescriptor, value);
        }

        public override object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {
            var val = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);
            if (val != null)
            {
                var attemptedValue = val.AttemptedValue;
                bindingContext.ModelState.SetModelValue(bindingContext.ModelName, val);

                if (!ValidateFormat(bindingContext.ModelType, attemptedValue))
                {
                    bindingContext.ModelState.AddModelError(bindingContext.ModelName, string.Format(ArticleStrings.InvalidDateTimeFormat, bindingContext.ModelMetadata.DisplayName));
                    return bindingContext.Model;
                }

                if (bindingContext.ModelType == typeof(TimeSpan) || bindingContext.ModelType == typeof(TimeSpan?))
                {
                    var dt = Converter.ToNullableDateTime(attemptedValue);
                    if (dt.HasValue)
                    {
                        return dt.Value.TimeOfDay;
                    }

                    if (bindingContext.ModelType == typeof(TimeSpan?))
                    {
                        return null;
                    }
                }
            }

            return base.BindModel(controllerContext, bindingContext);
        }

        protected bool ValidateFormat(Type type, string attemptedValue)
        {
            if (type == typeof(DateTime) && !Converter.CanParse<DateTime>(attemptedValue)
                || type == typeof(DateTime?) && !string.IsNullOrEmpty(attemptedValue) && !Converter.CanParse<DateTime>(attemptedValue)
                || type == typeof(TimeSpan) && !Converter.CanParse<TimeSpan>(attemptedValue)
                || type == typeof(TimeSpan?) && !string.IsNullOrEmpty(attemptedValue) && !Converter.CanParse<TimeSpan>(attemptedValue)
            )
            {
                return false;
            }

            return type != typeof(int) || Converter.CanParse<int>(attemptedValue);
        }

        protected string GetModelPropertyName<T>(ModelBindingContext bindingContext, Expression<Func<T>> e)
        {
            return string.IsNullOrWhiteSpace(bindingContext.ModelName) ? e.GetPropertyName() : string.Concat(bindingContext.ModelName, ".", e.GetPropertyName());
        }
    }
}
