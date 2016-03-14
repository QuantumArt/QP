using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Practices.EnterpriseLibrary.Validation;
using Microsoft.Practices.EnterpriseLibrary.Validation.Validators;
using Quantumart.QP8;
using Quantumart.QP8.BLL.Resources;
using Quantumart.QP8.BLL.Validators;
using Quantumart.QP8.Validators;

namespace Quantumart.QP8.BLL.Metadata
{
    public class UserMetadata
    {
        [RequiredValidator(MessageTemplateResourceName = "FirstNameNotEntered", MessageTemplateResourceType = typeof(UserStrings))]
        [StringLengthValidator(255, MessageTemplateResourceName = "FirstNameMaxLengthExceeded", MessageTemplateResourceType = typeof(UserStrings))]
        [FormatValidator(Constants.RegularExpressions.InvalidName, Negated = true, MessageTemplateResourceName = "FirstNameInvalidFormat", MessageTemplateResourceType = typeof(UserStrings))]
        public string FirstName
        {
            get;
            set;
        }
        [RequiredValidator(MessageTemplateResourceName = "LastNameNotEntered", MessageTemplateResourceType = typeof(UserStrings))]
        [StringLengthValidator(255, MessageTemplateResourceName = "LastNameMaxLengthExceeded", MessageTemplateResourceType = typeof(UserStrings))]
        [FormatValidator(Constants.RegularExpressions.InvalidName, Negated = true, MessageTemplateResourceName = "LastNameInvalidFormat", MessageTemplateResourceType = typeof(UserStrings))]
        public string LastName
        {
            get;
            set;
        }

        [RequiredValidator(MessageTemplateResourceName = "EmailNotEntered", MessageTemplateResourceType = typeof(UserStrings))]
        [StringLengthValidator(255, MessageTemplateResourceName = "EmailMaxLengthExceeded", MessageTemplateResourceType = typeof(UserStrings))]
        [FormatValidator(Constants.RegularExpressions.Email, Negated = true, MessageTemplateResourceName = "EmailInvalidFormat", MessageTemplateResourceType = typeof(UserStrings))]
        public string Email
        {
            get;
            set;
        }
    }
}
