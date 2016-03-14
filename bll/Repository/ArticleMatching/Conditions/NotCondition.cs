using System.Linq;

namespace Quantumart.QP8.BLL.Repository.ArticleMatching.Conditions
{
	public class NotCondition : ConditionBase
	{
		public NotCondition(ConditionBase condition)
		{
			Conditions = new ConditionBase[] { condition };
		}

		public override string GetCurrentExpression()
		{
			return "NOT (" + GetChildExpressions().Single() + ")";
		}
	}
}
