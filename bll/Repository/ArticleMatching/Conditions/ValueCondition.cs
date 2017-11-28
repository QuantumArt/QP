using System;

namespace Quantumart.QP8.BLL.Repository.ArticleMatching.Conditions
{
    public class ValueCondition : ConditionBase
    {
        public object Value { get; set; }

        public override string GetCurrentExpression() => throw new NotImplementedException();
    }
}
