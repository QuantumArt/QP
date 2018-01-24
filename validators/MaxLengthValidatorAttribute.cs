using System;
using Microsoft.Practices.EnterpriseLibrary.Validation;
using Microsoft.Practices.EnterpriseLibrary.Validation.Validators;

namespace Quantumart.QP8.Validators
{
    public class MaxLengthValidatorAttribute : ValueValidatorAttribute
    {
        public int UpperBound { get; set; }

        public MaxLengthValidatorAttribute(int upperBound)
        {
            UpperBound = upperBound;
        }

        protected override Validator DoCreateValidator(Type targetType) => new MaxLengthValidator(UpperBound);
    }
}
