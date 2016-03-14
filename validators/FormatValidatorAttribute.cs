using System;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Practices.EnterpriseLibrary.Validation;
using Microsoft.Practices.EnterpriseLibrary.Validation.Validators;
using Microsoft.Practices.EnterpriseLibrary.Validation.Properties;

namespace Quantumart.QP8.Validators
{
	/// <summary>
	/// Represents a <see cref="FormatValidator"/>.
	/// </summary>
	[AttributeUsage(AttributeTargets.Property
	| AttributeTargets.Field
	| AttributeTargets.Method
	| AttributeTargets.Parameter,
	AllowMultiple = true,
	Inherited = false)]
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1019",
		Justification = "Fields are used internally")]
	public sealed class FormatValidatorAttribute : ValueValidatorAttribute
	{
		private string pattern;
		private RegexOptions options;
		private string patternResourceName;
		private Type patternResourceType;

		/// <summary>
		/// <para>Initializes a new instance of the <see cref="FormatValidatorAttribute"/> class with a regex pattern.</para>
		/// </summary>
		/// <param name="pattern">The pattern to match.</param>
		public FormatValidatorAttribute(string pattern)
			: this(pattern, RegexOptions.None)
		{ }

		/// <summary>
		/// <para>Initializes a new instance of the <see cref="FormatValidatorAttribute"/> class with a regex pattern.</para>
		/// </summary>
		/// <param name="patternResourceName">The resource name containing the pattern for the regular expression.</param>
		/// <param name="patternResourceType">The type containing the resource for the regular expression.</param>
		public FormatValidatorAttribute(string patternResourceName, Type patternResourceType)
			: this(patternResourceName, patternResourceType, RegexOptions.None)
		{ }

		/// <summary>
		/// <para>Initializes a new instance of the <see cref="FormatValidatorAttribute"/> class with a regex pattern and 
		/// matching options.</para>
		/// </summary>
		/// <param name="pattern">The pattern to match.</param>
		/// <param name="options">The <see cref="RegexOptions"/> to use when matching.</param>
		public FormatValidatorAttribute(string pattern, RegexOptions options)
			: this(pattern, null, null, options)
		{ }

		/// <summary>
		/// <para>Initializes a new instance of the <see cref="FormatValidatorAttribute"/> class with a regex pattern.</para>
		/// </summary>
		/// <param name="patternResourceName">The resource name containing the pattern for the regular expression.</param>
		/// <param name="patternResourceType">The type containing the resource for the regular expression.</param>
		/// <param name="options">The <see cref="RegexOptions"/> to use when matching.</param>
		public FormatValidatorAttribute(string patternResourceName, Type patternResourceType, RegexOptions options)
			: this(null, patternResourceName, patternResourceType, options)
		{ }

		/// <summary>
		/// <para>Initializes a new instance of the <see cref="FormatValidatorAttribute"/> class with a regex pattern, 
		/// matching options and a failure message template.</para>
		/// </summary>
		/// <param name="pattern">The pattern to match.</param>
		/// <param name="patternResourceName">The resource name containing the pattern for the regular expression.</param>
		/// <param name="patternResourceType">The type containing the resource for the regular expression.</param>
		/// <param name="options">The <see cref="RegexOptions"/> to use when matching.</param>
		internal FormatValidatorAttribute(string pattern, string patternResourceName, Type patternResourceType, RegexOptions options)
		{
			ValidatorArgumentsValidatorHelper.ValidateRegexIgnoresEmptyStringValidator(pattern, patternResourceName, patternResourceType);

			this.pattern = pattern;
			this.options = options;
			this.patternResourceName = patternResourceName;
			this.patternResourceType = patternResourceType;
		}

		/// <summary>
		/// Creates the <see cref="FormatValidator"/> described by the attribute object.
		/// </summary>
		/// <param name="targetType">The type of object that will be validated by the validator.</param>
		/// <remarks>This operation must be overriden by subclasses.</remarks>
		/// <returns>The created <see cref="FormatValidator"/>.</returns>
		protected override Validator DoCreateValidator(Type targetType)
		{
			return new FormatValidator(this.pattern,
				this.patternResourceName,
				this.patternResourceType,
				this.options,
				this.MessageTemplate,
				Negated);
		}
	}
}
