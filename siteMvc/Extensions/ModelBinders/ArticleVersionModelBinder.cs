using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Quantumart.QP8.WebMvc.ViewModels.Article;
using Quantumart.QP8.WebMvc.ViewModels.ArticleVersion;

namespace Quantumart.QP8.WebMvc.Extensions.ModelBinders
{
    public class ArticleVersionViewModelBinder : IModelBinder
    {

        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            if (bindingContext.Model is ArticleVersionViewModel model)
            {
                var version = model.Data;
                var article = model.Data.Article;
                article.FieldValues = version.LoadFieldValues(true);
                ArticleViewModelBinder.UpdateFieldValues(bindingContext, article.FieldValues, false);
                article.UpdateAggregatedCollection();

                foreach (var aggArticle in article.AggregatedArticles)
                {
                    aggArticle.FieldValues = version.GetAggregatedArticle(aggArticle.ContentId).FieldValues;
                    ArticleViewModelBinder.UpdateFieldValues(bindingContext, aggArticle.FieldValues, true);
                }
            }
            return Task.CompletedTask;
        }
    }
}
