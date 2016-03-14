using System;
using System.Text;

namespace Quantumart.QP8.Validators
{
	internal static class ValidatorArgumentsValidatorHelper
	{
		internal static void ValidateRegexIgnoresEmptyStringValidator(string pattern, string patternResourceName, Type patternResourceType)
		{
			if (null == pattern && (patternResourceName == null || patternResourceType == null))
			{
				throw new ArgumentNullException("pattern");
			}
			else if (pattern == null && patternResourceName == null)
			{
				throw new ArgumentNullException("patternResourceName");
			}
			else if (pattern == null && patternResourceType == null)
			{
				throw new ArgumentNullException("patternResourceType");
			}
		}
	}
}
