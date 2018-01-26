using System;
using Microsoft.Practices.EnterpriseLibrary.Validation;
using Microsoft.Practices.EnterpriseLibrary.Validation.Validators;

namespace Quantumart.QP8.Validators
{
    /// <summary>
    /// Представляет реализацию <see cref="RequiredValidator" /> в виде атрибута
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Method | AttributeTargets.Class | AttributeTargets.Parameter, AllowMultiple = true, Inherited = false)]
    public sealed class RequiredValidatorAttribute : ValueValidatorAttribute
    {
        protected string _dependPropertyName = null;
        protected bool _inverse = false;

        public RequiredValidatorAttribute()
        {
        }

        public RequiredValidatorAttribute(string dependPropertyName)
        {
            _dependPropertyName = dependPropertyName;
        }

        /// <summary>
        /// Создает описанный атрибутом объект <see cref="RequiredValidator" />
        /// </summary>
        /// <param name="targetType">тип объекта, который должен быть проверен валидатором</param>
        /// <returns>созданный <see cref="RequiredValidator"/></returns>
        protected override Validator DoCreateValidator(Type targetType)
        {
            if (_dependPropertyName != null)
            {
                return new RequiredValidator(_dependPropertyName, _inverse);
            }
            return new RequiredValidator(Negated);
        }
    }
}
