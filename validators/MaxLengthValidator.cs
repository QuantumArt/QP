using Microsoft.Practices.EnterpriseLibrary.Validation;
using Microsoft.Practices.EnterpriseLibrary.Validation.Validators;

namespace Quantumart.QP8.Validators
{
    public class MaxLengthValidator : StringLengthValidator
    {
        public MaxLengthValidator(int upperBound)
            : base(0, RangeBoundaryType.Inclusive, upperBound, RangeBoundaryType.Inclusive, false)
        { }

        protected override void DoValidate(string objectToValidate, object currentTarget, string key, ValidationResults validationResults)
        {
            if (objectToValidate != null)
            {
                base.DoValidate(objectToValidate, currentTarget, key, validationResults);
            }
        }
    }
}
