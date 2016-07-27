using System;
using System.Collections.Generic;
using System.Web.Mvc;
using Quantumart.QP8.BLL;
using Quantumart.QP8.Constants;
using Quantumart.QP8.WebMvc.ViewModels.Article;

namespace Quantumart.QP8.WebMvc.Extensions.ModelBinders
{
	public class ArticleViewModelBinder : QpModelBinder
	{
		protected override void OnModelUpdated(ControllerContext controllerContext, ModelBindingContext bindingContext)
		{
			ArticleViewModel model = bindingContext.Model as ArticleViewModel;
			model.DoCustomBinding();
			UpdateArticle(model.Data, controllerContext, bindingContext);
			base.OnModelUpdated(controllerContext, bindingContext);
		}

		protected void UpdateArticle(Article item, ControllerContext controllerContext, ModelBindingContext bindingContext)
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
		}

		internal static void UpdateArticleWithVariations(ModelBindingContext bindingContext, Article article)
		{
			UpdateArticle(bindingContext, article, false, false);
			foreach (var varItem in article.VariationListItems)
			{
				var newValues = varItem.FieldValues;
				if (String.IsNullOrEmpty(varItem.Context))
				{
					article.UpdateFieldValuesWithAggregated(newValues);
				}
				else
				{
					Article varArticle = article.GetVariationByContext(varItem.Context);
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
					item.Delayed = false;
			}

			item.Visible = item.Schedule.IsVisible || item.Delayed;
		}

		internal static void UpdateFieldValues(ModelBindingContext bindingContext, IEnumerable<FieldValue> fieldValues, bool isArticleAggregated)
		{
			foreach (FieldValue pair in fieldValues)
			{
				if (pair.Field.IsReadOnly || isArticleAggregated && (pair.Field.IsClassifier || pair.Field.Aggregated))
					continue;

				string value;
				if (pair.Field.ExactType == FieldExactTypes.Boolean)
				{
					value = Utils.Converter.ToInt32(GetBooleanValue(bindingContext, pair.Field.FormName)).ToString();
				}
				else
				{
					value = GetValue(bindingContext, pair.Field.FormName);

					if (pair.Field.IsDateTime && !pair.Field.Required && Boolean.Parse(GetBooleanValue(bindingContext, pair.Field.FormCheckboxName)))
					{
						value = String.Empty;
					}
				}
				pair.UpdateValue(value);
			}
		}


		private static string GetBooleanValue(ModelBindingContext bindingContext, string fieldName)
		{
			string value = GetValue(bindingContext, fieldName);
			return String.IsNullOrEmpty(value) ? false.ToString() : value.Split(',')[0].ToString();
		}

		private static string GetValue(ModelBindingContext bindingContext, string fieldName)
		{
			ValueProviderResult value = bindingContext.ValueProvider.GetValue(fieldName);
			return (value != null) ? value.AttemptedValue.Trim() : String.Empty;
		}

	}
}