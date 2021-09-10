using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using QP8.Plugins.Contract;
using Quantumart.QP8.BLL;
using Quantumart.QP8.Constants;
using Quantumart.QP8.Utils;

namespace Quantumart.QP8.WebMvc.Extensions.ModelBinders
{
    public class PluginFieldValuesModelBinder : IModelBinder
    {
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            if (bindingContext.Model is IEnumerable<QpPluginFieldValue> model)
            {
                UpdateFieldValues(bindingContext, model);
            }
            return Task.CompletedTask;
        }
        internal static void UpdateFieldValues(ModelBindingContext bindingContext, IEnumerable<QpPluginFieldValue> fieldValues)
        {
            foreach (var pair in fieldValues)
            {
                string value;
                if (pair.Field.ValueType == QpPluginValueType.Bool)
                {
                    value = Converter.ToInt32(GetBooleanValue(bindingContext, pair.FormName)).ToString();
                }
                else
                {
                    value = GetValue(bindingContext, pair.FormName);
                }

                pair.Value = value;
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
