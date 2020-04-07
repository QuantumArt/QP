using Quantumart.QP8.BLL.Repository.ArticleMatching;
using Quantumart.QP8.BLL.Repository.ArticleMatching.Mappers;
using Quantumart.QP8.BLL.Repository.ArticleMatching.Models;
using Quantumart.QP8.Configuration;
using Quantumart.QP8.Constants;

namespace Quantumart.QP8.BLL.Services.API
{
    public class ArticleMatchService<T> : IArticleMatchService<T>
        where T : class
    {
        private readonly string _connectionString;
        private readonly DatabaseType _dbType;
        private readonly IConditionMapper<T> _mapper;

        public ArticleMatchService(string connectionString, DatabaseType dbType, IConditionMapper<T> mapper)
        {
            _connectionString = connectionString;
            _dbType = dbType;
            _mapper = mapper;
        }

        public ArticleInfo[] MatchArticles(int[] contentIds, T condition, MatchMode mode)
        {
            var conditionTree = _mapper.Map(condition);
            QPContext.CurrentDbConnectionInfo = new QpConnectionInfo(_connectionString, _dbType);
            using (new QPConnectionScope())
            {
                return ArticleMatchRepository.MatchArticles(contentIds, conditionTree, mode);
            }
        }

        public ArticleInfo[] MatchArticles(int contentId, T condition, MatchMode mode) => MatchArticles(new[] { contentId }, condition, mode);
    }
}
