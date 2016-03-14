using Quantumart.QP8.BLL.Repository.ArticleMatching.Conditions;

namespace Quantumart.QP8.BLL.Repository.ArticleMatching.Mappers
{
	public interface IConditionMapper<T>
		where T : class
	{
		ConditionBase Map(T source);
	}
}
