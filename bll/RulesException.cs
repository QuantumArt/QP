using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using Quantumart.QP8.Utils;

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

        private string GetPropertyName(LambdaExpression expr)
        {
            var member = expr.Body as MemberExpression;
            if (member == null)
            {
                return "";
            }

            var propInfo = member.Member as PropertyInfo;
            if (propInfo == null)
            {
                return "";
            }

            return member.Member.Name;
        }

        public IEnumerable<ValidationResult> GetValidationResults(string prefix)
        {
            prefix = string.IsNullOrEmpty(prefix) ? string.Empty : prefix + ".";
            var criticalErrors = Errors.Where(n => n.Critical).ToList();
            foreach (var error in criticalErrors)
            {
                yield return GetValidationResultFromRuleViolation(prefix, error);
            }

            if (!criticalErrors.Any())
            {
                foreach (var error in Errors.Where(n => !n.Critical))
                {
                    yield return GetValidationResultFromRuleViolation(prefix, error);
                }
            }
        }

        private ValidationResult GetValidationResultFromRuleViolation(string prefix, RuleViolation error)
        {
            var name = GetPropertyName(prefix, error);
            var members = new[] { name };
            return new ValidationResult(error.Message, members);
        }

        private string GetPropertyName(string prefix, RuleViolation error)
        {
            if (error.Property != null)
            {
                var name = GetPropertyName(error.Property);
                return string.IsNullOrEmpty(name) ? "" : prefix + name;
            }

            if (error.PropertyName != null)
            {
               return error.PropertyName;
            }

            return "";
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
