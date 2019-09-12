using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Quantumart.QP8.BLL;
using Quantumart.QP8.Constants;
using Quantumart.QP8.Utils;

namespace Quantumart.QP8.WebMvc.Extensions.ModelBinders
{
    public class FieldValuesModelBinder : IModelBinder
    {
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            if (bindingContext.Model is List<FieldValue> model)
            {
                UpdateFieldValues(bindingContext, model);
                var article = model.First().Article;
                article.UpdateAggregatedCollection();
                foreach (var aggArticle in article.AggregatedArticles)
                {
                    UpdateFieldValues(bindingContext, aggArticle.FieldValues);
                }
            }
            return Task.CompletedTask;
        }
        internal static void UpdateFieldValues(ModelBindingContext bindingContext, IEnumerable<FieldValue> fieldValues)
        {
            foreach (var pair in fieldValues)
            {
                if (pair.Field.IsReadOnly || pair.Field.Aggregated)
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
            return value.Values.ToString().Trim();
        }

        private static string GetBooleanValue(ModelBindingContext bindingContext, string fieldName)
        {
            var value = GetValue(bindingContext, fieldName);
            return string.IsNullOrEmpty(value) ? false.ToString() : value.Split(',')[0];
        }
    }
}
