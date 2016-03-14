using System;
using System.Collections.Generic;
using System.Text;

namespace Quantumart.QP8.Validators.Constants
{
	/// <summary>
	/// Типы валидации (валидаторов)
	/// </summary>
	public static class ValidationType
	{
		public const string None = "";
		public const string Required = "required";
		public const string StringLength = "stringLength";
		public const string ContainsCharacters = "containsCharacters";
		public const string Regex = "regex";
		public const string TypeConversion = "typeConversion";
		public const string Range = "range";
		public const string ValueComparison = "valueComparison";
		public const string PropertyComparison = "propertyComparison";
	}
}
