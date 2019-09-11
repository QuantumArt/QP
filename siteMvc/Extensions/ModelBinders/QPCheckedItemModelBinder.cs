using System;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore.Internal;
using Quantumart.QP8.WebMvc.Extensions.Helpers;

namespace Quantumart.QP8.WebMvc.Extensions.ModelBinders
{
    public class QpCheckedItemModelBinder : IModelBinder
    {
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            var key = bindingContext.ModelName;
            var val = bindingContext.ValueProvider.GetValue(key).Values;
            if (!String.IsNullOrEmpty(val))
            {
                var values = (string[])val;
                QPCheckedItem model = null;
                if (values.Length == 2)
                {
                    model = new QPCheckedItem { Value = values[0] };
                }

                bindingContext.Result = ModelBindingResult.Success(model);
                return Task.CompletedTask;
            }

            return Task.CompletedTask;
        }
    }
}
