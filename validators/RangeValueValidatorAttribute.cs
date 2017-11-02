using System;
using Microsoft.Practices.EnterpriseLibrary.Validation;
using Microsoft.Practices.EnterpriseLibrary.Validation.Validators;

namespace Quantumart.QP8.Validators
{
    public class RangeValueValidatorAttribute : ValueValidatorAttribute
    {
        public int LowerBound { get; set; }

        public int UpperBound { get; set; }

        public RangeValueValidatorAttribute(int lowerBound, int upperBound)
            : this(lowerBound, RangeBoundaryType.Inclusive, upperBound, RangeBoundaryType.Inclusive)
        {
        }

        public RangeValueValidatorAttribute(int lowerBound, RangeBoundaryType lowerInclude, int upperBound, RangeBoundaryType upperInclude)
        {
            LowerBound = lowerBound;
            UpperBound = upperBound;
        }

        protected override Validator DoCreateValidator(Type targetType) => new RangeValueValidator(LowerBound, UpperBound);
    }
}
