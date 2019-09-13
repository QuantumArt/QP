using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Newtonsoft.Json;

namespace Quantumart.QP8.BLL.Helpers
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

                bindingContext.Result = ModelBindingResult.Success(typeof(T) == typeof(IList<ArticleSearchQueryParam>)
                    ? JsonConvert.DeserializeObject<T>(incomingString, new JsonQueryParamConverter())
                    : JsonConvert.DeserializeObject<T>(incomingString));

            return Task.CompletedTask;
        }

    }
}
