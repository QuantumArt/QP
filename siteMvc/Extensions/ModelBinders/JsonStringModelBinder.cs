using System;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using Newtonsoft.Json;

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
            if (string.IsNullOrEmpty(val?.AttemptedValue))
            {
                return null;
            }

            bindingContext.ModelState.SetModelValue(key, val);

            string incomingString;
            switch (val.RawValue)
            {
                case string[] value:
                    incomingString = value[0];
                    break;
                case string _:
                    incomingString = (string)val.RawValue;
                    break;
                default:
                    throw new InvalidCastException($"{key} is not string[] or string");
            }

            try
            {
                var x = JsonConvert.DeserializeObject<T>(incomingString);
                var y = new JavaScriptSerializer().Deserialize<T>(incomingString);

                return JsonConvert.DeserializeObject<T>(incomingString);
            }
            catch (Exception exp)
            {
                bindingContext.ModelState.AddModelError(key, string.Format($"{key} is not valid.{Environment.NewLine}{exp.Message}"));
                return null;
            }
        }
    }
}
