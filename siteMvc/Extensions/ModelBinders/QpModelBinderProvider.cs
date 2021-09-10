using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Quantumart.QP8.BLL;
using Quantumart.QP8.WebMvc.Extensions.Helpers;
using Quantumart.QP8.WebMvc.ViewModels.Article;
using Quantumart.QP8.WebMvc.ViewModels.ArticleVersion;

namespace Quantumart.QP8.WebMvc.Extensions.ModelBinders
{
    public class QpModelBinderProvider : IModelBinderProvider
    {
        public IModelBinder GetBinder(ModelBinderProviderContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (context.Metadata.ModelType == typeof(List<FieldValue>))
            {
                 return new FieldValuesModelBinder();
            }

            if (context.Metadata.ModelType == typeof(IEnumerable<QpPluginFieldValue>))
            {
                return new PluginFieldValuesModelBinder();
            }

            if (context.Metadata.ModelType == typeof(Guid?))
            {
                return new GuidModelBinder();
            }

            if (context.Metadata.ModelType == typeof(QPCheckedItem))
            {
                return new QpCheckedItemModelBinder();
            }

            return null;
        }
    }
}
