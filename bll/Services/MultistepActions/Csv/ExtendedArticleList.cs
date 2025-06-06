using System;
using System.Collections.Generic;
using System.Linq;

namespace Quantumart.QP8.BLL.Services.MultistepActions.Csv
{
    public class ExtendedArticleList : List<ExtendedArticle>
    {
        public HashSet<Field> ExtensionFields { get; }

        public ExtendedArticleList()
        {
            ExtensionFields = new HashSet<Field>();
        }

        public ExtendedArticleList(ExtendedArticleList articles)
        {
            ExtensionFields = new HashSet<Field>(articles.ExtensionFields);
        }

        public List<Article> GetBaseArticles()
        {
            return this.Select(a => a.BaseArticle).ToList();
        }

        public List<int> GetBaseArticleIds()
        {
            return this.Select(a => a.BaseArticle.Id).ToList();
        }

        public List<int> GetExistingBaseArticleIds()
        {
            return this.Where(a => a.BaseArticle.Id > 0).Select(a => a.BaseArticle.Id).ToList();
        }

        public List<Article> GetAggregatedArticles(Field field)
        {
            return this.Select(a => a.Extensions[field]).ToList();
        }

        public IEnumerable<List<Article>> GetAllAggregatedArticles()
        {
            return ExtensionFields.Select(GetAggregatedArticles);
        }

        public ExtendedArticleList Filter(Func<Article, bool> predicate)
        {
            ExtendedArticleList result = new(this);
            result.AddRange(this.Where(article => predicate(article.BaseArticle)));
            return result;
        }
    }
}
