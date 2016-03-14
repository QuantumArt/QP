using System;
using System.Globalization;
using Microsoft.Practices.EnterpriseLibrary.Validation;
using Microsoft.Practices.EnterpriseLibrary.Validation.Validators;

namespace Quantumart.QP8.Validators
{
	public class ValueComparisonValidator : ValueValidator
	{
		private object _valueToCompare;
		/// <summary>
		/// Значение, которое нужно сравнить со значением проверямого объекта
		/// </summary>
		public object ValueToCompare
		{
			get { return _valueToCompare; }
		}

        private ComparisonOperator _comparisonOperator;
        /// <summary>
		/// Оператор, который определяет как будут сравниваться значения
        /// </summary>
        public ComparisonOperator ComparisonOperator
        {
            get { return _comparisonOperator; }
        }

		/// <summary>
		/// Возвращает типовой шаблон сообщения об ошибке, который используется, если валидация не опровергнута
		/// </summary>
		protected override string DefaultNonNegatedMessageTemplate
		{
			get { return Resources.ValueComparisonValidatorNonNegatedDefaultMessageTemplate; }
		}

		/// <summary>
		/// Возвращает типовой  шаблон сообщения об ошибке, который используется, если валидация опровергнута
		/// </summary>
		protected override string DefaultNegatedMessageTemplate
		{
			get { return Resources.ValueComparisonValidatorNegatedDefaultMessageTemplate; }
		}

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
        public ValueComparisonValidator(object valueToCompare,
			ComparisonOperator comparisonOperator,
            string messageTemplate,
            string tag)
            : this(valueToCompare, comparisonOperator, messageTemplate, tag, false)
        {
			this._valueToCompare = valueToCompare;
			this._comparisonOperator = comparisonOperator;
        }

		/// <summary>
		/// Инициализирует новый экземпляр класса <see cref="ValueComparisonValidator"/>
		/// </summary>
		/// <param name="valueToCompare">значение, которое нужно сравнить со значением проверямого объекта</param>
		/// <param name="comparisonOperator">оператор, который определяет как будут сравниваться значения</param>
		/// <param name="negated">принимает значение true, если валидатор должен опровергнуть результат валидации</param>
		public ValueComparisonValidator(object valueToCompare,
			ComparisonOperator comparisonOperator,
			bool negated)
			: this(valueToCompare, comparisonOperator, null, null, negated)
		{
			this._valueToCompare = valueToCompare;
			this._comparisonOperator = comparisonOperator;
		}

        /// <summary>
		/// Инициализирует новый экземпляр класса <see cref="ValueComparisonValidator"/>
        /// </summary>
		/// <param name="valueToCompare">значение, которое нужно сравнить со значением проверямого объекта</param>
		/// <param name="comparisonOperator">оператор, который определяет как будут сравниваться значения</param>
		/// <param name="messageTemplate">шаблон сообщения об ошибке</param>
		/// <param name="negated">принимает значение true, если валидатор должен опровергнуть результат валидации</param>
        public ValueComparisonValidator(object valueToCompare,
			ComparisonOperator comparisonOperator,
            string messageTemplate,
            bool negated)
			: this(valueToCompare, comparisonOperator, messageTemplate, null, negated)
        {
			this._valueToCompare = valueToCompare;
			this._comparisonOperator = comparisonOperator;
        }

        /// <summary>
		/// Инициализирует новый экземпляр класса <see cref="ValueComparisonValidator"/>
        /// </summary>
		/// <param name="valueToCompare">значение, которое нужно сравнить со значением проверямого объекта</param>
		/// <param name="comparisonOperator">оператор, который определяет как будут сравниваться значения</param>
		/// <param name="messageTemplate">шаблон сообщения об ошибке</param>
		/// <param name="tag">тэг, характеризующий назначение валидатора</param>
		/// <param name="negated">принимает значение true, если валидатор должен опровергнуть результат валидации</param>
		public ValueComparisonValidator(object valueToCompare,
			ComparisonOperator comparisonOperator,
            string messageTemplate,
            string tag,
            bool negated)
            : base(messageTemplate, tag, negated)
        {
			this._valueToCompare = valueToCompare;
			this._comparisonOperator = comparisonOperator;
        }

        /// <summary>
		/// Осуществляет валидацию путем сравнения заданного значения со значением указанного свойства объекта
        /// </summary>
		/// <param name="objectToValidate">объект для проверки</param>
		/// <param name="currentTarget">объект, на котором производится валидация</param>
		/// <param name="key">ключ, который идентифицирует источник <paramref name="objectToValidate"/></param>
		/// <param name="validationResults">результаты валидации</param>
        public override void DoValidate(object objectToValidate,
            object currentTarget,
            string key,
            ValidationResults validationResults)
        {
			object valueToCompare = null;

			try
			{
				valueToCompare = Utils.Converter.ChangeType(this._valueToCompare, objectToValidate.GetType());
			}
			catch
			{
				LogValidationResult(validationResults, Resources.ValueComparisonValidatorFailureToCastValue, currentTarget, key);

				return;
			}

            bool isValid = false;

			if (this._comparisonOperator == ComparisonOperator.Equal || this._comparisonOperator == ComparisonOperator.NotEqual)
			{
				isValid = (objectToValidate != null ? objectToValidate.Equals(valueToCompare) : valueToCompare == null)
					^ (this._comparisonOperator == ComparisonOperator.NotEqual)
					^ this.Negated;
			}
			else
			{
				IComparable comparableObjectToValidate = objectToValidate as IComparable;
				if (comparableObjectToValidate != null
					&& valueToCompare != null
					&& comparableObjectToValidate.GetType() == valueToCompare.GetType())
				{
					int comparison = comparableObjectToValidate.CompareTo(valueToCompare);

					switch (this._comparisonOperator)
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
						default:
							break;
					}

					isValid = isValid ^ this.Negated;
				}
				else
				{
					isValid = false;
				}
			}

            if (!isValid)
            {
                LogValidationResult(validationResults,
                    string.Format(
                        CultureInfo.CurrentCulture,
                        MessageTemplate,
						objectToValidate,
                        key,
                        this.Tag,
                        objectToValidate,
                        this._comparisonOperator,
						this._valueToCompare
					),
					currentTarget,
					key);
            }
        }
	}
}
