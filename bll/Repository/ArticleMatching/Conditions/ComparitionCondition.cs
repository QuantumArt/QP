using System;
using System.Collections.Generic;
using Quantumart.QP8.BLL.Repository.ArticleMatching.Models;

namespace Quantumart.QP8.BLL.Repository.ArticleMatching.Conditions
{
    public class ComparitionCondition : ConditionBase
    {
        #region Constructors

        public ComparitionCondition()
        {
        }

        public ComparitionCondition(object left, object right, string operation)
        {
            Operation = operation;

            var fieldConditions = new List<FieldCondition>();
            object value = null;

            if (left is QueryField[])
            {
                fieldConditions.Add(new FieldCondition { Fields = (QueryField[])left });
            }
            else
            {
                value = GetValue(left);
            }

            if (right is string[])
            {
                fieldConditions.Add(new FieldCondition { Fields = (QueryField[])right });
            }
            else
            {
                value = GetValue(right);
            }

            if (fieldConditions.Count == 2)
            {
                Conditions = fieldConditions.ToArray();
            }
            else if (fieldConditions.Count == 1)
            {
                var fieldCondition = fieldConditions[0];
                fieldCondition.Type = GetType(value);

                Conditions = new ConditionBase[]
                {
                    fieldCondition,
                    new ValueCondition { Value = GetValue(value) }
                };
            }
            else
            {
                throw new ArgumentException();
            }
        }

        public ComparitionCondition(QueryField[] fields, object value, string operation)
        {
            Operation = operation;

            Conditions = new ConditionBase[]
            {
                new FieldCondition { Fields = fields, Type = GetType(value) },
                new ValueCondition { Value = GetValue(value) }
            };
        }

        public ComparitionCondition(QueryField[] leftFields, QueryField[] rightFields, string operation)
        {
            Operation = operation;

            Conditions = new ConditionBase[]
            {
                new FieldCondition { Fields = leftFields },
                new FieldCondition { Fields = rightFields }
            };
        }

        #endregion

        private object GetValue(object value)
        {
            if (value is bool)
            {
                return (bool)value ? 1 : 0;
            }

            return value;
        }

        private string GetType(object value)
        {
            if (value == null)
            {
                return null;
            }

            if (value is string)
            {
                return "string";
            }

            if (value is int || value is decimal || value is bool)
            {
                return "numeric";
            }

            if (value is DateTime)
            {
                return "date";
            }

            throw new NotImplementedException();
        }

        public string Operation { get; set; }
        public override string GetCurrentExpression() => string.Join(" " + Operation + " ", GetChildExpressions());
    }
}
