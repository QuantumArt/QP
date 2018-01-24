namespace Quantumart.QP8.BLL.Repository.ArticleMatching.Conditions
{
    public class LogicalCondition : ConditionBase
    {
        public string Operation { get; set; }
        public override string GetCurrentExpression() => "(" + string.Join(" " + Operation + " ", GetChildExpressions()) + ")";
    }
}
