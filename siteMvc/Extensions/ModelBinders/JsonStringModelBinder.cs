using System;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace Quantumart.QP8.WebMvc.Extensions.ModelBinders
{
    /// <summary>
    /// Custom Model Binder для десериализации из строки с JSON
    /// </summary>
    public class JsonStringModelBinder<T> : IModelBinder
    {
        public object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {
            var key = bindingContext.ModelName;
            var val = bindingContext.ValueProvider.GetValue(key);
            if (!string.IsNullOrEmpty(val?.AttemptedValue))
            {
                bindingContext.ModelState.SetModelValue(key, val);

                string incomingString;
                var value = val.RawValue as string[];
                if (value != null)
                {
                    incomingString = value[0];
                }
                else if (val.RawValue is string)
                {
                    incomingString = (string)val.RawValue;
                }
                else
                {
                    throw new InvalidCastException(key + " is not string[] or string");
                }

                try
                {
                    return new JavaScriptSerializer().Deserialize<T>(incomingString);
                }
                catch (Exception exp)
                {
                    bindingContext.ModelState.AddModelError(key, String.Format("{2} is not valid.{1}{0}", exp.Message, Environment.NewLine, key));
                    return null;
                }
            }

            return null;
        }
    }
}
