using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Practices.EnterpriseLibrary.Validation.Validators;
using Microsoft.Practices.EnterpriseLibrary.Validation;

namespace Quantumart.QP8.Validators
{
    public class MaxLengthValidator : StringLengthValidator
    {

        public MaxLengthValidator(int upperBound) : base(0, RangeBoundaryType.Inclusive, upperBound, RangeBoundaryType.Inclusive, false) { }
      
        protected override void DoValidate(string objectToValidate, object currentTarget, string key, ValidationResults validationResults)
        {
            if (objectToValidate != null)
            {
                base.DoValidate(objectToValidate, currentTarget, key, validationResults);
            }

        }
    }
}
