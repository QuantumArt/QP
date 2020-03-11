using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Quantumart.QP8.WebMvc.Infrastructure.Helpers;

namespace Quantumart.QP8.WebMvc.Extensions.ModelBinders
{
    public class GuidModelBinder : IModelBinder
    {
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            var key = bindingContext.ModelName;
            var val = bindingContext.ValueProvider.GetValue(key).FirstValue;
            Guid? result = string.IsNullOrWhiteSpace(val) ? Guid.NewGuid() : GuidHelpers.GetGuidOrDefault(val);
            bindingContext.Result = ModelBindingResult.Success(result);
            return Task.CompletedTask;
        }
    }
}
