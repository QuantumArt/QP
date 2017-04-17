using System;
using System.Globalization;
using Microsoft.Practices.EnterpriseLibrary.Validation;
using Microsoft.Practices.EnterpriseLibrary.Validation.Validators;
using Quantumart.QP8.Utils;

namespace Quantumart.QP8.Validators
{
    public class CustomValidator : ValueValidator
    {
        /// <summary>
        /// Возвращает тип объекта, который содержит валидационный метод
        /// </summary>
        public Type ValidatorType { get; }

        /// <summary>
        ///  Возвращает имя валидационного метода
        /// </summary>
        public string Method { get; }

        /// <summary>
        /// Возвращает типовой шаблон сообщения об ошибке, который используется, если валидация не опровергнута
        /// </summary>
        protected override string DefaultNonNegatedMessageTemplate => Resources.CustomNonNegatedValidatorDefaultMessageTemplate;

        /// <summary>
        /// Возвращает типовой  шаблон сообщения об ошибке, который используется, если валидация опровергнута
        /// </summary>
        protected override string DefaultNegatedMessageTemplate => Resources.CustomNegatedValidatorDefaultMessageTemplate;

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
            ValidatorType = validatorType;
            Method = method;
        }

        /// <summary>
        /// Запускает пользовательскую валидацию
        /// </summary>
        /// <param name="objectToValidate">объект для проверки</param>
        /// <param name="currentTarget">объект, на котором производится валидация</param>
        /// <param name="key">ключ, который идентифицирует источник <paramref name="objectToValidate"/></param>
        /// <param name="validationResults">результаты валидации</param>
        public override void DoValidate(object objectToValidate, object currentTarget, string key, ValidationResults validationResults)
        {
            bool isValid;
            var messageTemplate = MessageTemplate;

            // Получаем информацию о методе и его параметрах
            var tempMethodInfo = ValidatorType.GetMethod(Method);
            var tempParametersInfo = tempMethodInfo.GetParameters();

            // Заполняем массив типов параметров
            var parameterTypes = new Type[3];
            parameterTypes[0] = tempParametersInfo[0].ParameterType;
            parameterTypes[1] = tempParametersInfo[1].ParameterType;
            parameterTypes[2] = Type.GetType("System.String&");

            // Получаем информацию о методе
            var methodInfo = ValidatorType.GetMethod(Method, parameterTypes);

            // Заполняем массив фактических значений параметров
            var parameters = new object[3];
            parameters[0] = Converter.ChangeType(objectToValidate, parameterTypes[0]);
            parameters[1] = Converter.ChangeType(currentTarget, parameterTypes[1]);
            parameters[2] = null;

            // Вызываем метод и получаем результат проверки
            if (methodInfo.IsStatic)
            {
                isValid = (bool)methodInfo.Invoke(ValidatorType, parameters);
            }
            else
            {
                // Создаем экземпляр объекта
                var validatorObject = Activator.CreateInstance(ValidatorType);
                isValid = (bool)methodInfo.Invoke(validatorObject, parameters);
            }

            if (isValid == Negated)
            {
                MessageTemplate = !string.IsNullOrWhiteSpace(parameters[2]?.ToString())
                    ? parameters[2].ToString()
                    : messageTemplate;

                LogValidationResult(
                    validationResults,
                    string.Format(CultureInfo.CurrentCulture, MessageTemplate, objectToValidate, key, Tag, ValidatorType, Method),
                    currentTarget,
                    key
               );
            }
        }
    }
}
