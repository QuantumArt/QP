using System;
using Microsoft.AspNetCore.Mvc.ModelBinding;
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

            if (context.Metadata.ModelType == typeof(QPCheckedItem))
            {
                return new QpCheckedItemModelBinder();
            }

            return null;
        }
    }
}
