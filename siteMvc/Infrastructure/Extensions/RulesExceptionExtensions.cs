using System.Globalization;
using System.Linq;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Quantumart.QP8.BLL;

namespace Quantumart.QP8.WebMvc.Infrastructure.Extensions
{
    public static class RulesExceptionExtensions
    {
        public static void Extend(this RulesException ex, ModelStateDictionary modelState, ModelExpressionProvider provider, string prefix = null)
        {
            prefix = string.IsNullOrEmpty(prefix) ? string.Empty : prefix + ".";
            var criticalErrors = ex.Errors.Where(n => n.Critical).ToList();
            foreach (var error in criticalErrors)
            {
                CopyError(modelState, prefix, error, provider);
            }

            if (!criticalErrors.Any())
            {
                foreach (var error in ex.Errors.Where(n => !n.Critical))
                {
                    CopyError(modelState, prefix, error, provider);
                }
            }
        }

        private static void CopyError(ModelStateDictionary modelState, string prefix, RuleViolation propertyError, ModelExpressionProvider provider)
        {
            var key = !string.IsNullOrEmpty(propertyError.PropertyName) ? propertyError.PropertyName : RulesException.GetPropertyName(propertyError.Property);
            if (key.StartsWith(Field.Prefix))
            {
                // динамические поля - без префикса, но надо установить Value для Telerik
                modelState.SetModelValue(key, new ValueProviderResult(propertyError.PropertyValue, CultureInfo.InvariantCulture));
            }
            else
            {
                key = prefix + key;
            }

            modelState.AddModelError(key, propertyError.Message);
        }
    }
}
