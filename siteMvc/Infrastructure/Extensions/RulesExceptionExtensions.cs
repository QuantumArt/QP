using System.Globalization;
using System.Linq;
using System.Web.Mvc;
using Quantumart.QP8.BLL;

namespace Quantumart.QP8.WebMvc.Infrastructure.Extensions
{
    public static class RulesExceptionExtensions
    {
        public static void Extend(this RulesException ex, ModelStateDictionary modelState, string prefix = null)
        {
            prefix = string.IsNullOrEmpty(prefix) ? string.Empty : prefix + ".";
            var criticalErrors = ex.Errors.Where(n => n.Critical).ToList();
            foreach (var error in criticalErrors)
            {
                CopyError(modelState, prefix, error);
            }

            if (!criticalErrors.Any())
            {
                foreach (var error in ex.Errors.Where(n => !n.Critical))
                {
                    CopyError(modelState, prefix, error);
                }
            }
        }

        private static void CopyError(ModelStateDictionary modelState, string prefix, RuleViolation propertyError)
        {
            var key = !string.IsNullOrEmpty(propertyError.PropertyName) ? propertyError.PropertyName : ExpressionHelper.GetExpressionText(propertyError.Property);
            if (key.StartsWith(Field.Prefix))
            {
                // динамические поля - без префикса, но надо установить Value для Telerik
                modelState.SetModelValue(key, new ValueProviderResult(propertyError.PropertyValue, propertyError.PropertyValue, CultureInfo.InvariantCulture));
            }
            else
            {
                key = prefix + key;
            }

            modelState.AddModelError(key, propertyError.Message);
        }
    }
}
