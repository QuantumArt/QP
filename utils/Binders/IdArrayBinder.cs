using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Quantumart.QP8.Utils.Binders
{
    public class IdArrayBinder : IModelBinder
    {
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            if (bindingContext == null)
            {
                throw new ArgumentNullException(nameof(bindingContext));
            }

            var modelName = bindingContext.ModelName;
            var valueProviderResult = bindingContext.ValueProvider.GetValue(modelName);
            if (valueProviderResult != ValueProviderResult.None)
            {
                bindingContext.ModelState.SetModelValue(modelName, valueProviderResult);
                var idArr = Converter.ToIdArray(valueProviderResult.FirstValue);
                bindingContext.Result = ModelBindingResult.Success(idArr);
            }
            return Task.CompletedTask;
        }
    }
}
