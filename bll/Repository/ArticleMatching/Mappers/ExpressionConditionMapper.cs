using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Quantumart.QP8.BLL.Repository.ArticleMatching.Conditions;
using Quantumart.QP8.BLL.Repository.ArticleMatching.Models;
using FieldInfo = System.Reflection.FieldInfo;

namespace Quantumart.QP8.BLL.Repository.ArticleMatching.Mappers
{
	public class ExpressionConditionMapper : IConditionMapper<Expression<Predicate<IArticle>>>
	{
		#region Private fields
		private readonly static Dictionary<ExpressionType, string> _singleExpressionMap = new Dictionary<ExpressionType, string>
		{
			{ExpressionType.Equal, "="},
			{ExpressionType.NotEqual, "<>"}
		};

		private readonly static Dictionary<ExpressionType, string> _binaryExpressionMap = new Dictionary<ExpressionType, string>
		{
			{ExpressionType.OrElse, "or"},
			{ExpressionType.AndAlso, "and"}
		};
		#endregion

		#region IConditionMapper implementation
		public ConditionBase Map(Expression<Predicate<IArticle>> source) => Map(source.Body);

	    #endregion

		#region private methods
		private static ConditionBase Map(Expression expression)
		{
			var binary = expression as BinaryExpression;
			string operation = null;

			if (binary != null)
			{
			    if (_singleExpressionMap.ContainsKey(binary.NodeType))
				{
					operation = _singleExpressionMap[binary.NodeType];
					return new ComparitionCondition(GetValue(binary.Left), GetValue(binary.Right), operation);
				}

			    if (_binaryExpressionMap.ContainsKey(binary.NodeType))
			    {
			        operation = _binaryExpressionMap[binary.NodeType];
			        var condition = new LogicalCondition { Operation = operation };

			        condition.Conditions = new[]
			        {
			            Map(binary.Left),
			            Map(binary.Right)
			        };

			        return condition;
			    }
			}

			return null;
		}

		private static object GetValue(Expression expression)
		{
			if (expression is ConstantExpression)
			{
				return (expression as ConstantExpression).Value;
			}

		    if (expression is MemberExpression)
		    {
		        var member = expression as MemberExpression;

		        if (member.Expression is ConstantExpression)
		        {
		            var obj = (member.Expression as ConstantExpression).Value;
		            var t = member.Member;
		            var t2 = t as FieldInfo;
		            var res = t2.GetValue(obj);
		            return res;
		        }

		        var call = member.Expression as MethodCallExpression;
		        var fields = GetFields(call).ToArray();
		        return fields;
		    }

		    return null;
		}

		private static IEnumerable<QueryField> GetFields(MethodCallExpression call)
		{
			var values = call.Arguments.OfType<ConstantExpression>().ToArray();
			var currentField = new QueryField { Name = values[0].Value as string };

			if (values.Length == 2)
			{
				currentField.ContentId = values[1].Value as int?;
			}

			var nextCall = call.Object as MethodCallExpression;

			if (nextCall != null)
			{
				foreach (var field in GetFields(nextCall))
				{
					yield return field;
				}
			}

			if (currentField != null)
			{
				yield return currentField;
			}
		}
		#endregion
	}
}
