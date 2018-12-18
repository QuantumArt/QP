using Microsoft.Practices.EnterpriseLibrary.Validation;
using Microsoft.Practices.EnterpriseLibrary.Validation.Validators;

namespace Quantumart.QP8.Validators
{
    public class MaxLengthValidator : StringLengthValidator
    {
        protected readonly string _dependPropertyName = null;
        protected readonly bool _inverse = false;
        public MaxLengthValidator(int upperBound)
            : base(0, RangeBoundaryType.Inclusive, upperBound, RangeBoundaryType.Inclusive, false)
        { }

        public MaxLengthValidator(int upperBound, string dependPropertyName, bool inverse = false)
         : base(0, RangeBoundaryType.Inclusive, upperBound, RangeBoundaryType.Inclusive, false)
        {
            _dependPropertyName = dependPropertyName;
            _inverse = inverse;
        }


        protected override void DoValidate(string objectToValidate, object currentTarget, string key, ValidationResults validationResults)
        {
            bool isContinue = ValidatorArgumentsValidatorHelper.CheckIsNeedtoValidate(_dependPropertyName, _inverse, currentTarget);

            if (isContinue)
            {
                if (objectToValidate != null)
                {
                    base.DoValidate(objectToValidate, currentTarget, key, validationResults);
                }
            }
        }
    }
}
