using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Practices.EnterpriseLibrary.Validation;
using Microsoft.Practices.EnterpriseLibrary.Validation.Validators;
using Quantumart.QP8.BLL.Resources;
using Quantumart.QP8.BLL.Validators;
using Quantumart.QP8.Validators;
using Quantumart.QP8.Constants;

namespace Quantumart.QP8.BLL
{
    public class NamedEntityObject : EntityObject
    {
        /// <summary>
        /// название сущности
        /// </summary>
        [NotNullValidator(MessageTemplateResourceName = "NameNotEntered", MessageTemplateResourceType = typeof(SiteStrings))]
        [MaxLengthValidator(255, MessageTemplateResourceName = "NameMaxLengthExceeded", MessageTemplateResourceType = typeof(SiteStrings))]
        [FormatValidator(Constants.RegularExpressions.InvalidName, Negated = true, MessageTemplateResourceName = "NameInvalidFormat", MessageTemplateResourceType = typeof(SiteStrings))]
        [CustomValidator(typeof(SiteValidators), "ValidateSiteNameUniqueness", MessageTemplateResourceName = "NameNonUnique", MessageTemplateResourceType = typeof(SiteStrings))]
        [LocalizedDisplayName("Name", NameResourceType = typeof(SiteStrings))]
        [Editor(EditorType.Textbox)]
        public string Name
        {
            get;
            set;
        }

        /// <summary>
        /// описание сущности
        /// </summary>
        [MaxLengthValidator(512, MessageTemplateResourceName = "DescriptionMaxLengthExceeded", MessageTemplateResourceType = typeof(SiteStrings))]
        [LocalizedDisplayName("Description", NameResourceType = typeof(SiteStrings))]
        [Editor(EditorType.TextArea)]
        public string Description
        {
            get;
            set;
        }
    }
}
