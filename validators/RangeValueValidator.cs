using Microsoft.Practices.EnterpriseLibrary.Validation;
using Microsoft.Practices.EnterpriseLibrary.Validation.Validators;

namespace Quantumart.QP8.Validators
{
    public class RangeValueValidator : RangeValidator<int>
    {
        public RangeValueValidator(int lowerbound, int upperBound)
           : base(lowerbound, RangeBoundaryType.Inclusive, upperBound, RangeBoundaryType.Inclusive)
        { }

        public RangeValueValidator(int lowerbound, RangeBoundaryType lowerInclude, int upperBound, RangeBoundaryType upperInclude)
            : base( lowerbound, lowerInclude, upperBound, upperInclude)
        { }

        protected override void DoValidate(int objectToValidate, object currentTarget, string key, ValidationResults validationResults)
        {
                base.DoValidate(objectToValidate, currentTarget, key, validationResults);
        }
        
    }
}
