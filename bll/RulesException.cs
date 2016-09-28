using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Text;

namespace Quantumart.QP8.BLL
{

    public class RuleViolation
    {
        public RuleViolation()
        {
            Critical = false;
        }

        public LambdaExpression Property { get; set; }

        public string Message { get; set; }

        public string PropertyName { get; set; }

        public string PropertyValue { get; set; }

        public bool Critical { get; set; }
    }

    [Serializable]
    [SuppressMessage("Microsoft.Usage", "CA2240")]
    public class RulesException : Exception
    {
        public readonly IList<RuleViolation> Errors = new List<RuleViolation>();

        private static readonly Expression<Func<object, object>> ThisObject = x => x;

        public bool IsEmpty => Errors.Count == 0;

        public void CriticalErrorForModel(string message)
        {
            Errors.Add(new RuleViolation { Property = ThisObject, Message = message, Critical = true });
        }

        public void ErrorForModel(string message)
        {
            Errors.Add(new RuleViolation { Property = ThisObject, Message = message });
        }

        public void Error(string propertyName, string propertyValue, string message)
        {
            Errors.Add(new RuleViolation { PropertyName = propertyName, PropertyValue = propertyValue, Message = message });
        }

        public static void Wrap(Exception ex)
        {
            var newEx = new RulesException();
            var sb = new StringBuilder(ex.Message);
            while (ex.InnerException != null)
            {
                ex = ex.InnerException;
                sb.Append(" ");
                sb.Append(ex.Message);
            }

            newEx.ErrorForModel(sb.ToString());
            throw newEx;
        }
    }

    [Serializable]
    public class RulesException<T> : RulesException
    {
        public void ErrorFor<TP>(Expression<Func<T, TP>> property, string message)
        {
            Errors.Add(new RuleViolation { Property = property, Message = message });
        }
    }
}
