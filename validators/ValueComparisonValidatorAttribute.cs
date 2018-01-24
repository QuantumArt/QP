using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Microsoft.Practices.EnterpriseLibrary.Validation;
using Microsoft.Practices.EnterpriseLibrary.Validation.Validators;

namespace Quantumart.QP8.Validators
{
    /// <summary>
    /// Представляет реализацию <see cref="ValueComparisonValidator" /> в виде атрибута
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1019", Justification = "Fields are used internally")]
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Method | AttributeTargets.Parameter, AllowMultiple = true, Inherited = false)]
    public sealed class ValueComparisonValidatorAttribute : ValueValidatorAttribute
    {
        // значение, которое нужно сравнить со значением проверяемого объекта
        private readonly object _valueToCompare;

        // оператор, который определяет как будут сравниваться значения
        private readonly ComparisonOperator _comparisonOperator;

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="ValueComparisonValidatorAttribute" />
        /// </summary>
        /// <param name="comparisonOperator">оператор, который определяет как будут сравниваться значения</param>
        /// <param name="valueToCompare">значение, которое нужно сравнить со значением проверяемого объекта</param>
        public ValueComparisonValidatorAttribute(object valueToCompare, ComparisonOperator comparisonOperator)
        {
            _valueToCompare = valueToCompare;
            _comparisonOperator = comparisonOperator;
        }

        /// <summary>
        /// Создает <see cref="ValueComparisonValidatorAttribute" /> описанный атрибутом объект
        /// </summary>
        /// <param name="targetType">тип объекта, который должен быть проверен валидатором</param>
        /// <remarks>
        /// Этот метод не может быть вызван на данном классе. Вызывайте
        /// <see cref="ValueComparisonValidatorAttribute.DoCreateValidator(Type, Type, MemberValueAccessBuilder, ValidatorFactory)" />.
        /// </remarks>
        protected override Validator DoCreateValidator(Type targetType) => new ValueComparisonValidator(_valueToCompare, _comparisonOperator, Negated);

        /// <summary>
        /// Устанавливает значение, которое определяет валидность объекта
        /// </summary>
        /// <param name="value">значение, которое определяет валидность объекта</param>
        /// <returns>Возвращает <see langword="true" />, если значение валидно; иначе, возвращает <see langword="false" />.</returns>
        /// <exception cref="NotSupportedException">при вызове на атрибуте, у которого набор правил не равен Null</exception>
        public override bool IsValid(object value)
        {
            if (!string.IsNullOrEmpty(Ruleset))
            {
                return true;
            }

            throw new NotSupportedException(string.Format(CultureInfo.CurrentCulture, Resources.ExceptionValidationAttributeNotSupported, GetType().Name));
        }
    }
}
