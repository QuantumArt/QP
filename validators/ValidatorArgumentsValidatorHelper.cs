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
    }
}
