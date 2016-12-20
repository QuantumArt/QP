using System;
using System.Globalization;
using System.Web.Mvc;
using Quantumart.QP8.WebMvc.Extensions.Helpers;

namespace Quantumart.QP8.WebMvc.Extensions.ModelBinders
{
    public class QpCheckedItemModelBinder : DefaultModelBinder
    {
        public override object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {
            var key = bindingContext.ModelName;
            var val = bindingContext.ValueProvider.GetValue(key);
            if (!string.IsNullOrEmpty(val?.AttemptedValue))
            {
                var values = (string[])val.RawValue;
                try
                {
                    bindingContext.ModelState.Remove(key);
                    if (values.Length == 2)
                    {
                        bindingContext.ModelState.SetModelValue(key, new ValueProviderResult(new[] { "true" }, "true", CultureInfo.InvariantCulture));
                        return new QPCheckedItem { Value = values[0] };
                    }

                    bindingContext.ModelState.SetModelValue(key, val);
                    return null;
                }
                catch (Exception exp)
                {
                    bindingContext.ModelState.AddModelError(key, string.Format("{2} is not valid.{1}{0}", exp.Message, Environment.NewLine, key));
                    return null;
                }
            }

            return null;
        }
    }
}
