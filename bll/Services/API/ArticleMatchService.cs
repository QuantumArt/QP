using Quantumart.QP8.BLL.Repository.ArticleMatching;
using Quantumart.QP8.BLL.Repository.ArticleMatching.Conditions;
using Quantumart.QP8.BLL.Repository.ArticleMatching.Mappers;
using Quantumart.QP8.BLL.Repository.ArticleMatching.Models;

namespace Quantumart.QP8.BLL.Services.API
{
	public class ArticleMatchService<T> : IArticleMatchService<T>
		where T : class
	{
		#region Private fields
		private readonly string _connectionString;
		private readonly IConditionMapper<T> _mapper;
		#endregion

		#region Constructor
		public ArticleMatchService(string connectionString, IConditionMapper<T> mapper)
		{
			_connectionString = connectionString;
			_mapper = mapper;
		}
		#endregion

		#region IArticleMatchService implementation
		public ArticleInfo[] MatchArticles(int[] contentIds, T condition, MatchMode mode)
		{
			ConditionBase conditionTree = _mapper.Map(condition);

			using (new QPConnectionScope(_connectionString))
			{
				return ArticleMatchRepository.MatchArticles(contentIds, conditionTree, mode);
			}
		}

		public ArticleInfo[] MatchArticles(int contentId, T condition, MatchMode mode)
		{
			return MatchArticles(new[] { contentId }, condition, mode);
		}
		#endregion
	}
}
