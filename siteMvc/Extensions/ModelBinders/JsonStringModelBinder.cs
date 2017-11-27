using System;
using System.Web.Mvc;
using Newtonsoft.Json;
using Quantumart.QP8.BLL;
using Quantumart.QP8.WebMvc.Infrastructure.Converters;

namespace Quantumart.QP8.WebMvc.Extensions.ModelBinders
{
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
                return typeof(T).IsAssignableFrom(typeof(ArticleContextQueryParam))
                    ? JsonConvert.DeserializeObject<T>(incomingString, new JsonQueryParamConverter())
                    : JsonConvert.DeserializeObject<T>(incomingString);
            }
            catch (Exception exp)
            {
                bindingContext.ModelState.AddModelError(key, string.Format($"{key} is not valid.{Environment.NewLine}{exp.Message}"));
                return null;
            }
        }
    }
}
