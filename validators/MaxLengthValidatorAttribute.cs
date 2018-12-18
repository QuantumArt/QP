using System;
using Microsoft.Practices.EnterpriseLibrary.Validation;
using Microsoft.Practices.EnterpriseLibrary.Validation.Validators;

namespace Quantumart.QP8.Validators
{
    public class MaxLengthValidatorAttribute : ValueValidatorAttribute
    {
        public int UpperBound { get; set; }
        protected readonly string _dependPropertyName = null;
        protected readonly bool _inverse = false;

        public MaxLengthValidatorAttribute(int upperBound)
        {
            UpperBound = upperBound;
        }

        public MaxLengthValidatorAttribute(int upperbound, string dependPropertyName, bool inverse = false)
        {
            UpperBound = upperbound;
            _dependPropertyName = dependPropertyName;
            _inverse = inverse;
        }
        protected override Validator DoCreateValidator(Type targetType)
        {
            return new MaxLengthValidator(UpperBound, _dependPropertyName, _inverse);
        }
    }
}
