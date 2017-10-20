namespace Quantumart.QP8.BLL.Repository.ArticleMatching.Conditions
{
	public class ParameterCondition : ConditionBase
	{
		public string Parameter { get; set; }

		public override string GetCurrentExpression() => Parameter;
	}
}
