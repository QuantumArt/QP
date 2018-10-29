using System;

namespace Quantumart.QP8.Validators
{
    internal static class ValidatorArgumentsValidatorHelper
    {
        internal static void ValidateRegexIgnoresEmptyStringValidator(string pattern, string patternResourceName, Type patternResourceType)
        {
            if (pattern == null)
            {
                if (patternResourceName == null || patternResourceType == null)
                {
                    throw new ArgumentNullException(nameof(pattern));
                }
            }
        }

        internal static bool CheckIsNeedtoValidate(string propertyName, bool inverse, object current)
        {
            if (propertyName != null)
            {
                var property = ValidationReflectionHelper.GetProperty(current.GetType(), propertyName, false);
                bool result;
                bool.TryParse(property.GetValue(current).ToString(), out result);
                if (inverse)
                {
                    return !result;
                }
                return result;
            }

            return true;
        }
    }
}
