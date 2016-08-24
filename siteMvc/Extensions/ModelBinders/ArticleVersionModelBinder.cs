using System.Linq;
using System.Web.Mvc;
using Quantumart.QP8.WebMvc.ViewModels.ArticleVersion;

namespace Quantumart.QP8.WebMvc.Extensions.ModelBinders
{
    public class ArticleVersionViewModelBinder : QpModelBinder
    {
        protected override void OnModelUpdated(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {            
            var model = (bindingContext.Model as ArticleVersionViewModel);
            if (model != null)
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
            base.OnModelUpdated(controllerContext, bindingContext);
        }
    }

}
