using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Quantumart.QP8.Utils
{
    public static class ExpressionExtensions
    {
        public static string GetPropertyName<T>(this Expression<Func<T>> propertyLambda)
        {
            var member = propertyLambda.Body as MemberExpression;
            if (member == null)
            {
                throw new ArgumentException($"Expression '{propertyLambda}' refers to a method, not a property.");
            }

            var propInfo = member.Member as PropertyInfo;
            if (propInfo == null)
            {
                throw new ArgumentException($"Expression '{propertyLambda}' refers to a field, not a property.");
            }

            return member.Member.Name;
        }
    }
}
