using System;
using Microsoft.Practices.EnterpriseLibrary.Validation;
using Microsoft.Practices.EnterpriseLibrary.Validation.Validators;

namespace Quantumart.QP8.Validators
{
    /// <summary>
    /// Представляет реализацию <see cref="CustomValidator"/> в виде атрибута
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Method | AttributeTargets.Class | AttributeTargets.Parameter, AllowMultiple = true, Inherited = false)]
    public sealed class CustomValidatorAttribute : ValueValidatorAttribute
    {
        private readonly Type _validatorType; // тип объекта, который содержит валидационный метод
        private readonly string _method; // имя валидационного метода

        public CustomValidatorAttribute(Type validatorType, string method)
        {
            _validatorType = validatorType;
            _method = method;
        }

        /// <summary>
        /// Создает описанный атрибутом объект <see cref="CustomValidator"/>
        /// </summary>
        /// <param name="targetType">тип объекта, который должен быть проверен валидатором</param>
        /// <returns>созданный <see cref="CustomValidator"/></returns>
        protected override Validator DoCreateValidator(Type targetType)
        {
            return new CustomValidator(_validatorType, _method, Negated);
        }
    }
}
