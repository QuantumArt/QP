using System;
using System.ComponentModel.DataAnnotations;
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

        internal static bool IsValidProperty(PropertyInfo propertyInfo)
        {
            return null != propertyInfo && propertyInfo.CanRead && propertyInfo.GetIndexParameters().Length == 0;
        }

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

        internal static bool IsValidField(FieldInfo fieldInfo)
        {
            return null != fieldInfo;
        }

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

        internal static bool IsValidMethod(MethodInfo methodInfo)
        {
            return null != methodInfo && typeof(void) != methodInfo.ReturnType && methodInfo.GetParameters().Length == 0;
        }

        internal static T ExtractValidationAttribute<T>(MemberInfo attributeProvider, string ruleset)
            where T : BaseValidationAttribute
        {
            return attributeProvider != null
                ? GetCustomAttributes(attributeProvider, typeof(T), false).Cast<T>().FirstOrDefault(attribute => ruleset.Equals(attribute.Ruleset))
                : null;
        }

        internal static T ExtractValidationAttribute<T>(ParameterInfo attributeProvider, string ruleset)
            where T : BaseValidationAttribute
        {
            return attributeProvider?.GetCustomAttributes(typeof(T), false).Cast<T>().FirstOrDefault(attribute => ruleset.Equals(attribute.Ruleset));
        }

        /// <summary>
        /// Retrieves an array of the custom attributes applied to a member of a type, looking for the existence
        /// of a metadata type where the attributes are actually specified.
        /// Parameters specify the member, the type of the custom attribute to search
        /// for, and whether to search ancestors of the member.
        /// </summary>
        /// <param name="element">An object derived from the <see cref="MemberInfo"/> class that describes a
        /// constructor, event, field, method, or property member of a class.</param>
        /// <param name="attributeType">The type, or a base type, of the custom attribute to search for.</param>
        /// <param name="inherit">If <see langword="true"/>, specifies to also search the ancestors of element for
        /// custom attributes.</param>
        /// <returns>An <see cref="Attribute"/> array that contains the custom attributes of type type applied to
        /// element, or an empty array if no such custom attributes exist.</returns>
        /// <seealso cref="MetadataTypeAttribute"/>
        public static Attribute[] GetCustomAttributes(MemberInfo element, Type attributeType, bool inherit)
        {
            var matchingElement = GetMatchingElement(element);
            return Attribute.GetCustomAttributes(matchingElement, attributeType, inherit);
        }

        private static MemberInfo GetMatchingElement(MemberInfo element)
        {
            var sourceType = element as Type;
            var elementIsType = sourceType != null;
            if (sourceType == null)
            {
                sourceType = element.DeclaringType;
            }

            var metadataTypeAttribute = (MetadataTypeAttribute)Attribute.GetCustomAttribute(sourceType, typeof(MetadataTypeAttribute), false);
            if (metadataTypeAttribute == null)
            {
                return element;
            }

            sourceType = metadataTypeAttribute.MetadataClassType;
            if (elementIsType)
            {
                return sourceType;
            }

            var matchingMembers = sourceType.GetMember(element.Name, element.MemberType, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (matchingMembers.Length > 0)
            {
                var methodBase = element as MethodBase;
                if (methodBase == null)
                {
                    return matchingMembers[0];
                }

                var parameterTypes = methodBase.GetParameters().Select(pi => pi.ParameterType).ToArray();
                return matchingMembers.Cast<MethodBase>().FirstOrDefault(mb => MatchMethodBase(mb, parameterTypes)) ?? element;
            }

            return element;
        }

        private static bool MatchMethodBase(MethodBase mb, Type[] parameterTypes)
        {
            var parameters = mb.GetParameters();
            if (parameters.Length != parameterTypes.Length)
            {
                return false;
            }

            return !parameters.Where((t, i) => t.ParameterType != parameterTypes[i]).Any();
        }
    }
}
