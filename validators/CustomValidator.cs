using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Practices.EnterpriseLibrary.Common.Configuration;
using Microsoft.Practices.EnterpriseLibrary.Validation;
using Microsoft.Practices.EnterpriseLibrary.Validation.Configuration;
using Microsoft.Practices.EnterpriseLibrary.Validation.Properties;
using Microsoft.Practices.EnterpriseLibrary.Validation.Validators;
using System.Globalization;
using Quantumart.QP8;

namespace Quantumart.QP8.Validators
{
	public class CustomValidator : ValueValidator
	{
		private Type _validatorType;
		/// <summary>
		/// Возвращает тип объекта, который содержит валидационный метод
		/// </summary>
		public Type ValidatorType
		{
			get { return _validatorType; }
		}

		private string _method;
		/// <summary>
		///  Возвращает имя валидационного метода
		/// </summary>
		public string Method
		{
			get { return _method; }
		}

		/// <summary>
		/// Возвращает типовой шаблон сообщения об ошибке, который используется, если валидация не опровергнута 
		/// </summary>
		protected override string DefaultNonNegatedMessageTemplate
		{
			get { return Resources.CustomNonNegatedValidatorDefaultMessageTemplate; }
		}

		/// <summary>
		/// Возвращает типовой  шаблон сообщения об ошибке, который используется, если валидация опровергнута 
		/// </summary>
		protected override string DefaultNegatedMessageTemplate
		{
			get { return Resources.CustomNegatedValidatorDefaultMessageTemplate; }
		}

		/// <summary>
		/// <para>Инициализирует новый экземпляр класса <see cref="CustomValidator"/></para>
		/// </summary>
		/// <param name="validatorType">тип объекта, который содержит валидационный метод</param>
		/// <param name="method">имя валидационного метода</param>
		public CustomValidator(Type validatorType, string method)
			: this(validatorType, method, false)
		{ }

		/// <summary>
		///		<para>Инициализирует новый экземпляр класса <see cref="CustomValidator"/></para>
		/// </summary>
		/// <param name="validatorType">тип объекта, который содержит валидационный метод</param>
		/// <param name="method">имя валидационного метода</param>
		/// <param name="negated">принимает значение true, если валидатор должен опровергнуть результат валидации</param>
		public CustomValidator(Type validatorType, string method, bool negated)
			: this(validatorType, method, negated, null)
		{ }

		/// <summary>
		/// <para>Инициализирует новый экземпляр класса <see cref="CustomValidator"/></para>
		/// </summary>
		/// <param name="validatorType">тип объекта, который содержит валидационный метод</param>
		/// <param name="method">имя валидационного метода</param>
		/// <param name="messageTemplate">шаблон сообщения об ошибке</param>
		public CustomValidator(Type validatorType, string method, string messageTemplate)
			: this(validatorType, method, false, messageTemplate)
		{ }

		/// <summary>
		/// <para>Инициализирует новый экземпляр класса <see cref="CustomValidator"/></para>
		/// </summary>
		/// <param name="validatorType">тип объекта, который содержит валидационный метод</param>
		/// <param name="method">имя валидационного метода</param>
		/// <param name="negated">принимает значение true, если валидатор должен опровергнуть результат валидации</param>
		/// <param name="messageTemplate">шаблон сообщения об ошибке</param>
		public CustomValidator(Type validatorType, string method, bool negated, string messageTemplate)
			: base(messageTemplate, null, negated)
		{
			this._validatorType = validatorType;
			this._method = method;
		}

		/// <summary>
		/// Запускает пользовательскую валидацию
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
			string messageTemplate = this.MessageTemplate;

			// Получаем информацию о методе и его параметрах
			MethodInfo tempMethodInfo = _validatorType.GetMethod(_method);
			ParameterInfo[] tempParametersInfo = tempMethodInfo.GetParameters();

			// Заполняем массив типов параметров
			Type[] parameterTypes = new Type[3];
			parameterTypes[0] = tempParametersInfo[0].ParameterType;
			parameterTypes[1] = tempParametersInfo[1].ParameterType;
			parameterTypes[2] = Type.GetType("System.String&");

			tempParametersInfo = null;
			tempMethodInfo = null;

			// Получаем информацию о методе
			MethodInfo methodInfo = _validatorType.GetMethod(_method, parameterTypes);

			// Заполняем массив фактических значений параметров
			Object[] parameters = new Object[3];
			parameters[0] = Utils.Converter.ChangeType(objectToValidate, parameterTypes[0]);
			parameters[1] = Utils.Converter.ChangeType(currentTarget,parameterTypes[1]);
			parameters[2] = null;

			// Вызываем метод и получаем результат проверки
			if (methodInfo.IsStatic)
			{
				isValid = (bool)methodInfo.Invoke(_validatorType, parameters);
			}
			else
			{
				// Создаем экземпляр объекта
				Object validatorObject = Activator.CreateInstance(_validatorType);

				isValid = (bool)methodInfo.Invoke(validatorObject, parameters);

				validatorObject = null;
			}

			if (isValid == Negated)
			{
				if (parameters[2] != null && !String.IsNullOrWhiteSpace(parameters[2].ToString()))
				{
					this.MessageTemplate = parameters[2].ToString();
				}
				else
				{
					this.MessageTemplate = messageTemplate;
				}
				LogValidationResult(
					validationResults, 
					string.Format(
                        CultureInfo.CurrentCulture,
						MessageTemplate,
						objectToValidate,
                        key,
                        this.Tag,
						_validatorType,
						_method
					),
					currentTarget,
					key);
			}

			parameters = null;
			parameterTypes = null;
		}
	}
}
