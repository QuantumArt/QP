using System;
using System.Collections.Generic;
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
    public class ArticleViewModelBinder : IModelBinder
    {

        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            if (bindingContext.Model is ArticleViewModel model)
            {
                model.DoCustomBinding();
                UpdateArticle(model.Data, bindingContext);
            }

            return Task.CompletedTask;
        }
        protected void UpdateArticle(Article item, ModelBindingContext bindingContext)
        {
            if (item.UseVariations)
            {
                UpdateArticleWithVariations(bindingContext, item);
            }
            else
            {
                UpdateArticle(bindingContext, item, false);
                item.UpdateAggregatedCollection();
                foreach (var aggArticle in item.AggregatedArticles)
                {
                    UpdateArticle(bindingContext, aggArticle, true);
                }
            }

            item.UniqueId = GetGuidOrGenerate(bindingContext);
        }

        internal static void UpdateArticleWithVariations(ModelBindingContext bindingContext, Article article)
        {
            UpdateArticle(bindingContext, article, false, false);
            foreach (var varItem in article.VariationListItems)
            {
                var newValues = varItem.FieldValues;
                if (string.IsNullOrEmpty(varItem.Context))
                {
                    article.UpdateFieldValuesWithAggregated(newValues);
                }
                else
                {
                    var varArticle = article.GetVariationByContext(varItem.Context);
                    varArticle.UpdateFieldValuesWithAggregated(newValues);
                    varArticle.UseInVariationUpdate = true;
                }
            }
        }

        internal static void UpdateArticle(ModelBindingContext bindingContext, Article item, bool isArticleAggregated, bool withFieldValues = true)
        {
            if (withFieldValues)
            {
                UpdateFieldValues(bindingContext, item.FieldValues, isArticleAggregated);
            }

            if (!item.Workflow.IsAssigned || !item.Workflow.IsAsync || !item.Workflow.CurrentUserHasWorkflowMaxWeight || item.StatusTypeId == item.Workflow.MaxStatus.Id)
            {
                item.CancelSplit = false;
            }

            if (item.Workflow.IsAssigned)
            {
                if (item.StatusTypeId != item.Workflow.MaxStatus.Id)
                {
                    item.Delayed = false;
                }
            }

            item.Visible = item.Schedule.IsVisible || item.Delayed;
        }

        internal static void UpdateFieldValues(ModelBindingContext bindingContext, IEnumerable<FieldValue> fieldValues, bool isArticleAggregated)
        {
            foreach (var pair in fieldValues)
            {
                if (pair.Field.IsReadOnly || isArticleAggregated && (pair.Field.IsClassifier || pair.Field.Aggregated))
                {
                    continue;
                }

                string value;
                if (pair.Field.ExactType == FieldExactTypes.Boolean)
                {
                    value = Converter.ToInt32(GetBooleanValue(bindingContext, pair.Field.FormName)).ToString();
                }
                else
                {
                    value = GetValue(bindingContext, pair.Field.FormName);
                    if (pair.Field.IsDateTime && !pair.Field.Required && bool.Parse(GetBooleanValue(bindingContext, pair.Field.FormCheckboxName)))
                    {
                        value = string.Empty;
                    }
                }

                pair.UpdateValue(value);
            }
        }

        private static string GetValue(ModelBindingContext bindingContext, string fieldName)
        {
            var value = bindingContext.ValueProvider.GetValue(fieldName);
            return value.FirstValue.Trim();
        }

        private static string GetBooleanValue(ModelBindingContext bindingContext, string fieldName)
        {
            var value = GetValue(bindingContext, fieldName);
            return string.IsNullOrEmpty(value) ? false.ToString() : value.Split(',')[0];
        }

        private static Guid? GetGuidOrGenerate(ModelBindingContext bindingContext)
        {
            Expression<Func<ArticleViewModel, Guid?>> guidExpression = vm => vm.Data.UniqueId;
            var fieldName = ExpressionHelper.GetExpressionText(guidExpression);
            var rawGuid = GetValue(bindingContext, fieldName);
            return string.IsNullOrWhiteSpace(rawGuid) ? Guid.NewGuid() : GuidHelpers.GetGuidOrDefault(rawGuid);
        }


    }
}
