using Quantumart.QP8.BLL.Repository.ArticleMatching;
using Quantumart.QP8.BLL.Repository.ArticleMatching.Models;

namespace Quantumart.QP8.BLL.Services.API
{
	public interface IArticleMatchService<T>
	{
		ArticleInfo[] MatchArticles(int[] contentIds, T condition, MatchMode mode);
		ArticleInfo[] MatchArticles(int contentId, T condition, MatchMode mode);
	}
}
