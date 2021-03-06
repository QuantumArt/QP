﻿using System;
using System.Globalization;
using Microsoft.Practices.EnterpriseLibrary.Validation;
using Microsoft.Practices.EnterpriseLibrary.Validation.Validators;
using Quantumart.QP8.Utils;

namespace Quantumart.QP8.Validators
{
    public class ValueComparisonValidator : ValueValidator
    {
        /// <summary>
        /// Значение, которое нужно сравнить со значением проверямого объекта
        /// </summary>
        public object ValueToCompare { get; }

        /// <summary>
        /// Оператор, который определяет как будут сравниваться значения
        /// </summary>
        public ComparisonOperator ComparisonOperator { get; }

        /// <summary>
        /// Возвращает типовой шаблон сообщения об ошибке, который используется, если валидация не опровергнута
        /// </summary>
        protected override string DefaultNonNegatedMessageTemplate => Resources.ValueComparisonValidatorNonNegatedDefaultMessageTemplate;

        /// <summary>
        /// Возвращает типовой  шаблон сообщения об ошибке, который используется, если валидация опровергнута
        /// </summary>
        protected override string DefaultNegatedMessageTemplate => Resources.ValueComparisonValidatorNegatedDefaultMessageTemplate;

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="ValueComparisonValidator"/>
        /// </summary>
        /// <param name="valueToCompare">значение, которое нужно сравнить со значением проверямого объекта</param>
        /// <param name="comparisonOperator">оператор, который определяет как будут сравниваться значения</param>
        public ValueComparisonValidator(object valueToCompare, ComparisonOperator comparisonOperator)
            : this(valueToCompare, comparisonOperator, null, null)
        { }

        /// <summary>
		/// Инициализирует новый экземпляр класса <see cref="ValueComparisonValidator"/>
        /// </summary>
		/// <param name="valueToCompare">значение, которое нужно сравнить со значением проверямого объекта</param>
		/// <param name="comparisonOperator">оператор, который определяет как будут сравниваться значения</param>
		/// <param name="messageTemplate">шаблон сообщения об ошибке</param>
		/// <param name="tag">тэг, характеризующий назначение валидатора</param>
        public ValueComparisonValidator(object valueToCompare, ComparisonOperator comparisonOperator, string messageTemplate, string tag)
            : this(valueToCompare, comparisonOperator, messageTemplate, tag, false)
        {
            ValueToCompare = valueToCompare;
            ComparisonOperator = comparisonOperator;
        }

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="ValueComparisonValidator"/>
        /// </summary>
        /// <param name="valueToCompare">значение, которое нужно сравнить со значением проверямого объекта</param>
        /// <param name="comparisonOperator">оператор, который определяет как будут сравниваться значения</param>
        /// <param name="negated">принимает значение true, если валидатор должен опровергнуть результат валидации</param>
        public ValueComparisonValidator(object valueToCompare, ComparisonOperator comparisonOperator, bool negated)
            : this(valueToCompare, comparisonOperator, null, null, negated)
        {
            ValueToCompare = valueToCompare;
            ComparisonOperator = comparisonOperator;
        }

        /// <summary>
		/// Инициализирует новый экземпляр класса <see cref="ValueComparisonValidator"/>
        /// </summary>
		/// <param name="valueToCompare">значение, которое нужно сравнить со значением проверямого объекта</param>
		/// <param name="comparisonOperator">оператор, который определяет как будут сравниваться значения</param>
		/// <param name="messageTemplate">шаблон сообщения об ошибке</param>
		/// <param name="negated">принимает значение true, если валидатор должен опровергнуть результат валидации</param>
        public ValueComparisonValidator(object valueToCompare, ComparisonOperator comparisonOperator, string messageTemplate, bool negated)
            : this(valueToCompare, comparisonOperator, messageTemplate, null, negated)
        {
            ValueToCompare = valueToCompare;
            ComparisonOperator = comparisonOperator;
        }

        /// <summary>
		/// Инициализирует новый экземпляр класса <see cref="ValueComparisonValidator"/>
        /// </summary>
		/// <param name="valueToCompare">значение, которое нужно сравнить со значением проверямого объекта</param>
		/// <param name="comparisonOperator">оператор, который определяет как будут сравниваться значения</param>
		/// <param name="messageTemplate">шаблон сообщения об ошибке</param>
		/// <param name="tag">тэг, характеризующий назначение валидатора</param>
		/// <param name="negated">принимает значение true, если валидатор должен опровергнуть результат валидации</param>
		public ValueComparisonValidator(object valueToCompare, ComparisonOperator comparisonOperator, string messageTemplate, string tag, bool negated)
            : base(messageTemplate, tag, negated)
        {
            ValueToCompare = valueToCompare;
            ComparisonOperator = comparisonOperator;
        }

        /// <summary>
		/// Осуществляет валидацию путем сравнения заданного значения со значением указанного свойства объекта
        /// </summary>
		/// <param name="objectToValidate">объект для проверки</param>
		/// <param name="currentTarget">объект, на котором производится валидация</param>
		/// <param name="key">ключ, который идентифицирует источник <paramref name="objectToValidate"/></param>
		/// <param name="validationResults">результаты валидации</param>
        public override void DoValidate(object objectToValidate, object currentTarget, string key, ValidationResults validationResults)
        {
            object valueToCompare;

            try
            {
                valueToCompare = Converter.ChangeType(ValueToCompare, objectToValidate.GetType());
            }
            catch
            {
                LogValidationResult(validationResults, Resources.ValueComparisonValidatorFailureToCastValue, currentTarget, key);

                return;
            }

            var isValid = false;
            if (ComparisonOperator == ComparisonOperator.Equal || ComparisonOperator == ComparisonOperator.NotEqual)
            {
                isValid = objectToValidate.Equals(valueToCompare) ^ ComparisonOperator == ComparisonOperator.NotEqual ^ Negated;
            }
            else
            {
                var comparableObjectToValidate = objectToValidate as IComparable;
                if (comparableObjectToValidate != null && valueToCompare != null && comparableObjectToValidate.GetType() == valueToCompare.GetType())
                {
                    var comparison = comparableObjectToValidate.CompareTo(valueToCompare);
                    switch (ComparisonOperator)
                    {
                        case ComparisonOperator.GreaterThan:
                            isValid = comparison > 0;
                            break;
                        case ComparisonOperator.GreaterThanEqual:
                            isValid = comparison >= 0;
                            break;
                        case ComparisonOperator.LessThan:
                            isValid = comparison < 0;
                            break;
                        case ComparisonOperator.LessThanEqual:
                            isValid = comparison <= 0;
                            break;
                    }

                    isValid = isValid ^ Negated;
                }
            }

            if (!isValid)
            {
                LogValidationResult(
                    validationResults,
                    string.Format(CultureInfo.CurrentCulture, MessageTemplate, objectToValidate, key, Tag, objectToValidate, ComparisonOperator, ValueToCompare),
                    currentTarget,
                    key
                );
            }
        }
    }
}
