using System;
using System.Globalization;
using System.Text.RegularExpressions;
using Microsoft.Practices.EnterpriseLibrary.Common;
using Microsoft.Practices.EnterpriseLibrary.Validation;
using Microsoft.Practices.EnterpriseLibrary.Validation.Validators;

namespace Quantumart.QP8.Validators
{
    /// <summary>
    /// Performs validation on strings by matching them to a <see cref="Regex" /> (ignore empty string).
    /// </summary>
    public class FormatValidator : ValueValidator<string>
    {
        private readonly RegexOptions _options;
        private readonly string _dependPropertyName;
        private readonly bool _inverse;
        /// <summary>
        ///     <para>Initializes a new instance of the <see cref="FormatValidator" /> class with a regex pattern.</para>
        /// </summary>
        /// <param name="pattern">The pattern to match.</param>
        public FormatValidator(string pattern)
            : this(pattern, RegexOptions.None)
        {
        }

        /// <summary>
        ///     <para>Initializes a new instance of the <see cref="FormatValidator" /> class with a regex pattern.</para>
        /// </summary>
        /// <param name="patternResourceName">The resource name containing the pattern for the regular expression.</param>
        /// <param name="patternResourceType">The type containing the resource for the regular expression.</param>
        public FormatValidator(string patternResourceName, Type patternResourceType)
            : this(patternResourceName, patternResourceType, RegexOptions.None)
        {
        }

        /// <summary>
        ///     <para>Initializes a new instance of the <see cref="FormatValidator" /> class with a regex pattern.</para>
        /// </summary>
        /// <param name="pattern">The pattern to match.</param>
        /// <param name="negated">True if the validator must negate the result of the validation.</param>
        public FormatValidator(string pattern, bool negated)
            : this(pattern, RegexOptions.None, negated)
        {
        }

        /// <summary>
        ///     <para>Initializes a new instance of the <see cref="FormatValidator" /> class with a regex pattern.</para>
        /// </summary>
        /// <param name="patternResourceName">The resource name containing the pattern for the regular expression.</param>
        /// <param name="patternResourceType">The type containing the resource for the regular expression.</param>
        /// <param name="negated">True if the validator must negate the result of the validation.</param>
        public FormatValidator(string patternResourceName, Type patternResourceType, bool negated)
            : this(patternResourceName, patternResourceType, RegexOptions.None, negated)
        {
        }

        /// <summary>
        ///     <para>
        ///     Initializes a new instance of the <see cref="FormatValidator" /> class with a regex pattern and
        ///     matching options.
        ///     </para>
        /// </summary>
        /// <param name="pattern">The pattern to match.</param>
        /// <param name="options">The <see cref="RegexOptions" /> to use when matching.</param>
        public FormatValidator(string pattern, RegexOptions options)
            : this(pattern, options, null)
        {
        }

        /// <summary>
        ///     <para>Initializes a new instance of the <see cref="FormatValidator" /> class with a regex pattern.</para>
        /// </summary>
        /// <param name="patternResourceName">The resource name containing the pattern for the regular expression.</param>
        /// <param name="patternResourceType">The type containing the resource for the regular expression.</param>
        /// <param name="options">The <see cref="RegexOptions" /> to use when matching.</param>
        public FormatValidator(string patternResourceName, Type patternResourceType, RegexOptions options)
            : this(patternResourceName, patternResourceType, options, null)
        {
        }

        /// <summary>
        ///     <para>
        ///     Initializes a new instance of the <see cref="FormatValidator" /> class with a regex pattern and
        ///     matching options.
        ///     </para>
        /// </summary>
        /// <param name="pattern">The pattern to match.</param>
        /// <param name="options">The <see cref="RegexOptions" /> to use when matching.</param>
        /// <param name="negated">True if the validator must negate the result of the validation.</param>
        public FormatValidator(string pattern, RegexOptions options, bool negated)
            : this(pattern, options, null, negated)
        {
        }

        /// <summary>
        ///     <para>
        ///     Initializes a new instance of the <see cref="FormatValidator" /> class with a regex pattern and
        ///     matching options.
        ///     </para>
        /// </summary>
        /// <param name="patternResourceName">The resource name containing the pattern for the regular expression.</param>
        /// <param name="patternResourceType">The type containing the resource for the regular expression.</param>
        /// <param name="options">The <see cref="RegexOptions" /> to use when matching.</param>
        /// <param name="negated">True if the validator must negate the result of the validation.</param>
        public FormatValidator(string patternResourceName, Type patternResourceType, RegexOptions options, bool negated)
            : this(patternResourceName, patternResourceType, options, null, negated)
        {
        }

        /// <summary>
        ///     <para>
        ///     Initializes a new instance of the <see cref="FormatValidator" /> class with a regex pattern
        ///     and a failure message template.
        ///     </para>
        /// </summary>
        /// <param name="pattern">The pattern to match.</param>
        /// <param name="messageTemplate">The message template.</param>
        public FormatValidator(string pattern, string messageTemplate)
            : this(pattern, RegexOptions.None, messageTemplate)
        {
        }

        /// <summary>
        ///     <para>
        ///     Initializes a new instance of the <see cref="FormatValidator" /> class with a regex pattern
        ///     and a failure message template.
        ///     </para>
        /// </summary>
        /// <param name="patternResourceName">The resource name containing the pattern for the regular expression.</param>
        /// <param name="patternResourceType">The type containing the resource for the regular expression.</param>
        /// <param name="messageTemplate">The message template.</param>
        public FormatValidator(string patternResourceName, Type patternResourceType, string messageTemplate)
            : this(patternResourceName, patternResourceType, RegexOptions.None, messageTemplate)
        {
        }

        /// <summary>
        ///     <para>
        ///     Initializes a new instance of the <see cref="FormatValidator" /> class with a regex pattern
        ///     and a failure message template.
        ///     </para>
        /// </summary>
        /// <param name="pattern">The pattern to match.</param>
        /// <param name="messageTemplate">The message template.</param>
        /// <param name="negated">True if the validator must negate the result of the validation.</param>
        public FormatValidator(string pattern, string messageTemplate, bool negated)
            : this(pattern, RegexOptions.None, messageTemplate, negated)
        {
        }

        /// <summary>
        ///     <para>
        ///     Initializes a new instance of the <see cref="FormatValidator" /> class with a regex pattern
        ///     and a failure message template.
        ///     </para>
        /// </summary>
        /// <param name="patternResourceName">The resource name containing the pattern for the regular expression.</param>
        /// <param name="patternResourceType">The type containing the resource for the regular expression.</param>
        /// <param name="messageTemplate">The message template.</param>
        /// <param name="negated">True if the validator must negate the result of the validation.</param>
        public FormatValidator(string patternResourceName, Type patternResourceType, string messageTemplate, bool negated)
            : this(patternResourceName, patternResourceType, RegexOptions.None, messageTemplate, negated)
        {
        }

        /// <summary>
        ///     <para>
        ///     Initializes a new instance of the <see cref="FormatValidator" /> class with a regex pattern,
        ///     matching options and a failure message template.
        ///     </para>
        /// </summary>
        /// <param name="pattern">The pattern to match.</param>
        /// <param name="options">The <see cref="RegexOptions" /> to use when matching.</param>
        /// <param name="messageTemplate">The message template.</param>
        public FormatValidator(string pattern, RegexOptions options, string messageTemplate)
            : this(pattern, options, messageTemplate, false)
        {
        }

        /// <summary>
        ///     <para>
        ///     Initializes a new instance of the <see cref="FormatValidator" /> class with a regex pattern,
        ///     matching options and a failure message template.
        ///     </para>
        /// </summary>
        /// <param name="patternResourceName">The resource name containing the pattern for the regular expression.</param>
        /// <param name="patternResourceType">The type containing the resource for the regular expression.</param>
        /// <param name="options">The <see cref="RegexOptions" /> to use when matching.</param>
        /// <param name="messageTemplate">The message template.</param>
        public FormatValidator(string patternResourceName, Type patternResourceType, RegexOptions options, string messageTemplate)
            : this(patternResourceName, patternResourceType, options, messageTemplate, false)
        {
        }

        /// <summary>
        ///     <para>
        ///     Initializes a new instance of the <see cref="FormatValidator" /> class with a regex pattern,
        ///     matching options and a failure message template.
        ///     </para>
        /// </summary>
        /// <param name="pattern">The pattern to match.</param>
        /// <param name="options">The <see cref="RegexOptions" /> to use when matching.</param>
        /// <param name="messageTemplate">The message template.</param>
        /// <param name="negated">True if the validator must negate the result of the validation.</param>
        public FormatValidator(string pattern, RegexOptions options, string messageTemplate, bool negated)
            : this(pattern, null, null, options, messageTemplate, negated)
        {
        }

        /// <summary>
        ///     <para>
        ///     Initializes a new instance of the <see cref="FormatValidator" /> class with a regex pattern,
        ///     matching options and a failure message template.
        ///     </para>
        /// </summary>
        /// <param name="options">The <see cref="RegexOptions" /> to use when matching.</param>
        /// <param name="messageTemplate">The message template.</param>
        /// <param name="negated">True if the validator must negate the result of the validation.</param>
        /// <param name="patternResourceName">The resource name containing the pattern for the regular expression.</param>
        /// <param name="patternResourceType">The type containing the resource for the regular expression.</param>
        public FormatValidator(string patternResourceName, Type patternResourceType, RegexOptions options, string messageTemplate, bool negated)
            : this(null, patternResourceName, patternResourceType, options, messageTemplate, negated)
        {
        }

        /// <summary>
        ///     <para>
        ///     Initializes a new instance of the <see cref="FormatValidator" /> class with a regex pattern,
        ///     matching options and a failure message template.
        ///     </para>
        /// </summary>
        /// <param name="pattern">The pattern to match.</param>
        /// <param name="options">The <see cref="RegexOptions" /> to use when matching.</param>
        /// <param name="messageTemplate">The message template.</param>
        /// <param name="negated">True if the validator must negate the result of the validation.</param>
        /// <param name="propertyName">The name of the property which validator depends on.</param>
        /// <param name="inverse">True if the validator must inverse the propertyName value.</param>
        /// <param name="patternResourceName">The resource name containing the pattern for the regular expression.</param>
        /// <param name="patternResourceType">The type containing the resource for the regular expression.</param>
        public FormatValidator(string pattern, string patternResourceName, Type patternResourceType, RegexOptions options, string messageTemplate, bool negated, string propertyName = null, bool inverse = false)
            : base(messageTemplate, null, negated)
        {
            ValidatorArgumentsValidatorHelper.ValidateRegexIgnoresEmptyStringValidator(pattern, patternResourceName, patternResourceType);

            Pattern = pattern;
            _options = options;
            _dependPropertyName = propertyName;
            _inverse = inverse;
            PatternResourceName = patternResourceName;
            PatternResourceType = patternResourceType;
        }

        /// <summary>
        /// Validates by comparing <paramref name="objectToValidate" /> by matching it to the pattern
        /// specified for the validator.
        /// </summary>
        /// <param name="objectToValidate">The object to validate.</param>
        /// <param name="currentTarget">The object on the behalf of which the validation is performed.</param>
        /// <param name="key">The key that identifies the source of <paramref name="objectToValidate" />.</param>
        /// <param name="validationResults">The validation results to which the outcome of the validation should be stored.</param>
        /// <remarks>
        /// <see langword="null" /> is considered a failed validation.
        /// </remarks>
        protected override void DoValidate(string objectToValidate, object currentTarget, string key, ValidationResults validationResults)
        {
            bool isContinue = ValidatorArgumentsValidatorHelper.CheckIsNeedtoValidate(_dependPropertyName, _inverse, currentTarget);
          
            if (isContinue)
            {
                var logError = Negated;
                if (!string.IsNullOrWhiteSpace(objectToValidate))
                {
                    var pattern = GetPattern();
                    if (Options != null)
                    {
                        var regex = new Regex(pattern, (RegexOptions)Options);
                        logError = !regex.IsMatch(objectToValidate);
                    }
                }

                if (logError != Negated)
                {
                    LogValidationResult(validationResults, GetMessage(objectToValidate, key), currentTarget, key);
                }
            }
        }

        /// <summary>
        /// Gets the message representing a failed validation.
        /// </summary>
        /// <param name="objectToValidate">The object for which validation was performed.</param>
        /// <param name="key">The key representing the value being validated for <paramref name="objectToValidate" />.</param>
        /// <returns>The message representing the validation failure.</returns>
        protected override string GetMessage(object objectToValidate, string key) => string.Format(CultureInfo.CurrentCulture, MessageTemplate, objectToValidate, key, Tag, Pattern, _options);

        /// <summary>
        /// Gets the pattern used for building the regular expression.
        /// </summary>
        /// <returns>The regular expression pattern.</returns>
        public string GetPattern() => !string.IsNullOrEmpty(Pattern)
            ? Pattern
            : ResourceStringLoader.LoadString(PatternResourceType.FullName, PatternResourceName, PatternResourceType.Assembly);

        /// <summary>
        /// Gets the Default Message Template when the validator is non negated.
        /// </summary>
        protected override string DefaultNonNegatedMessageTemplate => Resources.RegexIgnoresEmptyStringValidatorNonNegatedDefaultMessageTemplate;

        /// <summary>
        /// Gets the Default Message Template when the validator is negated.
        /// </summary>
        protected override string DefaultNegatedMessageTemplate => Resources.RegexIgnoresEmptyStringValidatorNegatedDefaultMessageTemplate;

        /// <summary>
        /// Regular expression pattern used.
        /// </summary>
        public string Pattern { get; }

        /// <summary>
        /// Any regex options specified.
        /// </summary>
        public RegexOptions? Options => _options;

        /// <summary>
        /// Resource name used to load regex pattern.
        /// </summary>
        public string PatternResourceName { get; }

        /// <summary>
        /// Resource type used to look up regex pattern.
        /// </summary>
        public Type PatternResourceType { get; }
    }
}
