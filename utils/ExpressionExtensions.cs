using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.Reflection;

namespace Quantumart.QP8.Utils
{
	public static class ExpressionExtensions
	{
		/// <summary>
		/// Получить имя свойства объекта
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="e"></param>
		/// <returns></returns>
		public static string GetPropertyName<T>(this Expression<Func<T>> propertyLambda)
		{
			Type type = typeof(T);

			var member = propertyLambda.Body as MemberExpression;
			if (member == null)
				throw new ArgumentException(string.Format(
					"Expression '{0}' refers to a method, not a property.",
					propertyLambda.ToString()));
			
			PropertyInfo propInfo = member.Member as PropertyInfo;
			if (propInfo == null)
				throw new ArgumentException(string.Format(
					"Expression '{0}' refers to a field, not a property.",
					propertyLambda.ToString()));
			
			return member.Member.Name;
		}
	}
}
