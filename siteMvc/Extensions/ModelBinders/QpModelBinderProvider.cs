using System;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using QA.Validation.Xaml.Adapters;
using Quantumart.QP8.WebMvc.ViewModels.Article;
using Quantumart.QP8.WebMvc.ViewModels.ArticleVersion;
using Quantumart.QP8.WebMvc.ViewModels.User;

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

            if (context.Metadata.ModelType == typeof(ArticleViewModel))
            {
                 return new ArticleViewModelBinder();
            }

            if (context.Metadata.ModelType == typeof(ArticleVersionViewModel))
            {
                return new ArticleVersionViewModelBinder();
            }

            if (context.Metadata.ModelType == typeof(ArticleVersionViewModel))
            {
                return new ArticleVersionViewModelBinder();
            }

            return null;
        }
    }
}
