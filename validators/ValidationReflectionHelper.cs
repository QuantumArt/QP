using System;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Microsoft.Practices.EnterpriseLibrary.Validation.Validators;

namespace Quantumart.QP8.Validators
{
    /// <summary>
    /// Helper for reflection access.
    /// </summary>
    public static class ValidationReflectionHelper
    {
        internal static PropertyInfo GetProperty(Type type, string propertyName, bool throwIfInvalid)
        {
            if (string.IsNullOrEmpty(propertyName))
            {
                throw new ArgumentNullException(nameof(propertyName));
            }

            var propertyInfo = type.GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
            if (!IsValidProperty(propertyInfo))
            {
                if (throwIfInvalid)
                {
                    throw new ArgumentException(
                        string.Format(
                            CultureInfo.CurrentCulture,
                            Resources.ExceptionInvalidProperty,
                            propertyName,
                            type.FullName));
                }

                return null;
            }

            return propertyInfo;
        }

        internal static bool IsValidProperty(PropertyInfo propertyInfo) => null != propertyInfo && propertyInfo.CanRead && propertyInfo.GetIndexParameters().Length == 0;

        internal static FieldInfo GetField(Type type, string fieldName, bool throwIfInvalid)
        {
            if (string.IsNullOrEmpty(fieldName))
            {
                throw new ArgumentNullException(nameof(fieldName));
            }

            var fieldInfo = type.GetField(fieldName, BindingFlags.Public | BindingFlags.Instance);
            if (!IsValidField(fieldInfo))
            {
                if (throwIfInvalid)
                {
                    throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Resources.ExceptionInvalidField, fieldName, type.FullName));
                }

                return null;
            }

            return fieldInfo;
        }

        internal static bool IsValidField(FieldInfo fieldInfo) => null != fieldInfo;

        internal static MethodInfo GetMethod(Type type, string methodName, bool throwIfInvalid)
        {
            if (string.IsNullOrEmpty(methodName))
            {
                throw new ArgumentNullException(nameof(methodName));
            }

            var methodInfo = type.GetMethod(methodName, BindingFlags.Public | BindingFlags.Instance, null, Type.EmptyTypes, null);
            if (!IsValidMethod(methodInfo))
            {
                if (throwIfInvalid)
                {
                    throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Resources.ExceptionInvalidMethod, methodName, type.FullName));
                }

                return null;
            }

            return methodInfo;
        }

        internal static bool IsValidMethod(MethodInfo methodInfo) => null != methodInfo && typeof(void) != methodInfo.ReturnType && methodInfo.GetParameters().Length == 0;

    }
}
