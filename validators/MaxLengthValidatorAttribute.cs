using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Practices.EnterpriseLibrary.Validation;
using Microsoft.Practices.EnterpriseLibrary.Validation.Validators;
using Microsoft.Practices.EnterpriseLibrary.Validation.Properties;

namespace Quantumart.QP8.Validators
{
    public class MaxLengthValidatorAttribute : ValueValidatorAttribute
    {
        public int UpperBound
        {
            get;
            set;
        }

        public MaxLengthValidatorAttribute(int upperBound)
        {
            UpperBound = upperBound;
        }

        protected override Validator DoCreateValidator(Type targetType)
        {
            return new MaxLengthValidator(UpperBound);
        }
    }
}
