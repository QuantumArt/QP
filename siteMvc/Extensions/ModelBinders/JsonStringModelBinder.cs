using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Newtonsoft.Json;
using Quantumart.QP8.BLL;
using Quantumart.QP8.WebMvc.Infrastructure.Converters;

namespace Quantumart.QP8.WebMvc.Extensions.ModelBinders
{
    public class JsonStringModelBinder<T> : IModelBinder
    {
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            var key = bindingContext.ModelName;
            var val = bindingContext.ValueProvider.GetValue(key);
            if (string.IsNullOrEmpty(val.FirstValue))
            {
                return Task.CompletedTask;
            }

            bindingContext.ModelState.SetModelValue(key, val);

            var incomingString = val.FirstValue;

            try
            {
                bindingContext.Result = ModelBindingResult.Success(typeof(T) == typeof(IList<ArticleSearchQueryParam>)
                    ? JsonConvert.DeserializeObject<T>(incomingString, new JsonQueryParamConverter())
                    : JsonConvert.DeserializeObject<T>(incomingString));
            }
            catch (Exception exp)
            {
                bindingContext.ModelState.AddModelError(key, string.Format($"{key} is not valid.{Environment.NewLine}{exp.Message}"));
            }

            return Task.CompletedTask;
        }

    }
}
