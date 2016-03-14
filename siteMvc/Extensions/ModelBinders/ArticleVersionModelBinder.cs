using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Quantumart.QP8.BLL;
using Quantumart.QP8.Constants;
using System.Web.Mvc;
using System.Globalization;
using Quantumart.QP8.WebMvc.ViewModels;

namespace Quantumart.QP8.WebMvc.Extensions.ModelBinders
{
    public class ArticleVersionViewModelBinder : QpModelBinder
    {
        protected override void OnModelUpdated(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {            
			ArticleVersionViewModel model = (bindingContext.Model as ArticleVersionViewModel);
			ArticleVersion version = model.Data;
			Article article = model.Data.Article;
			article.FieldValues = version.GetFieldValues(true);
			ArticleViewModelBinder.UpdateFieldValues(bindingContext, article.FieldValues, false);
			article.UpdateAggregatedCollection();

			foreach (var aggArticle in article.AggregatedArticles)
			{
				aggArticle.FieldValues = version.GetAggregatedFieldValues(aggArticle, true).ToList();
				ArticleViewModelBinder.UpdateFieldValues(bindingContext, aggArticle.FieldValues, true);
			}
           
            base.OnModelUpdated(controllerContext, bindingContext);
        }
	}

}
