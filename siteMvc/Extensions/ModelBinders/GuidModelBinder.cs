using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ViewFeatures.Internal;
using Quantumart.QP8.BLL;
using Quantumart.QP8.Constants;
using Quantumart.QP8.Utils;
using Quantumart.QP8.WebMvc.Infrastructure.Helpers;
using Quantumart.QP8.WebMvc.ViewModels.Article;

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
