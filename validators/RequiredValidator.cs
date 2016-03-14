using System;
using System.Collections.Generic;
using Microsoft.Practices.EnterpriseLibrary.Common.Configuration;
using Microsoft.Practices.EnterpriseLibrary.Validation;
using Microsoft.Practices.EnterpriseLibrary.Validation.Validators;
using Microsoft.Practices.EnterpriseLibrary.Validation.Configuration;
using Microsoft.Practices.EnterpriseLibrary.Validation.Properties;
using System.Globalization;

namespace Quantumart.QP8.Validators
{
	public class RequiredValidator : ValueValidator
	{
		/// <summary>
		/// Возвращает типовой шаблон сообщения об ошибке, который используется, если валидация не опровергнута 
		/// </summary>
		protected override string DefaultNonNegatedMessageTemplate
		{
			get { return Resources.RequiredNonNegatedValidatorDefaultMessageTemplate; }
		}

		/// <summary>
		/// Возвращает типовой  шаблон сообщения об ошибке, который используется, если валидация опровергнута 
		/// </summary>
		protected override string DefaultNegatedMessageTemplate
		{
			get { return Resources.RequiredNegatedValidatorDefaultMessageTemplate; }
		}

		/// <summary>
		/// <para>Инициализирует новый экземпляр класса <see cref="RequiredValidator"/></para>
		/// </summary>
		public RequiredValidator()
			: this(false)
		{ }

		/// <summary>
		/// <para>Инициализирует новый экземпляр класса <see cref="RequiredValidator"/></para>
		/// </summary>
		public RequiredValidator(bool negated)
			: this(negated, null)
		{ }

		/// <summary>
		/// <para>Инициализирует новый экземпляр класса <see cref="RequiredValidator"/></para>
		/// </summary>
		/// <param name="messageTemplate">шаблон сообщения об ошибке</param>
		public RequiredValidator(string messageTemplate)
			: this(false, messageTemplate)
		{ }

		/// <summary>
		/// <para>Инициализирует новый экземпляр класса <see cref="RequiredValidator"/> с шаблоном сообщения об ошибке</para>
		/// </summary>
		/// <param name="negated">принимает значение true, если валидатор должен опровергнуть результат валидации</param>
		/// <param name="messageTemplate">шаблон сообщения об ошибке</param>
		public RequiredValidator(bool negated, string messageTemplate)
			: base(messageTemplate, null, negated)
		{ }

		/// <summary>
		/// Проверяет обязательность заполнения <paramref name="objectToValidate"/>
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
			bool isValid = false;
			if (objectToValidate != null && !String.IsNullOrEmpty(objectToValidate.ToString()))
			{
				isValid = true;
			}

			if (isValid == Negated)
			{
				LogValidationResult(validationResults, GetMessage(objectToValidate, key), currentTarget, key);
			}
		}
	}
}
