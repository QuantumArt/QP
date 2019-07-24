using System;
using System.Collections.Generic;
using System.Linq;
using Quantumart.QP8.BLL.Repository.ArticleRepositories;
using Quantumart.QP8.Constants;
using Quantumart.QP8.Utils.FullTextSearch;

namespace Quantumart.QP8.BLL.Services
{
    public interface ISearchInArticlesService
    {
        /// <summary>
        /// Получить результаты поиск статей во всех контентах сайта
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="searchString"></param>
        /// <param name="listCmd"></param>
        /// <param name="totalRecords"></param>
        /// <returns></returns>
        IEnumerable<SearchInArticlesResultItem> SearchInArticles(int siteId, int userId, string searchString, ListCommand listCmd, out int totalRecords);
    }

    public class SearchInArticlesService : ISearchInArticlesService
    {
        private readonly ISearchGrammarParser grammaParser;
        private readonly ISearchInArticlesRepository siaRepository;

        public SearchInArticlesService(ISearchGrammarParser grammaParser, ISearchInArticlesRepository siaRepository)
        {
            this.grammaParser = grammaParser ?? throw new ArgumentNullException(nameof(grammaParser));
            this.siaRepository = siaRepository ?? throw new ArgumentNullException(nameof(siaRepository));
        }

        public IEnumerable<SearchInArticlesResultItem> SearchInArticles(int siteId, int userId, string searchString, ListCommand listCmd, out int totalRecords)
        {
            totalRecords = 0;
            string sqlSearchString;
            if (QPContext.DatabaseType == DatabaseType.SqlServer)
            {
                if (!grammaParser.TryParse(searchString, out sqlSearchString))
                {
                    return Enumerable.Empty<SearchInArticlesResultItem>();
                }
            }
            else
            {
                sqlSearchString = searchString;
            }


            if (string.IsNullOrWhiteSpace(sqlSearchString))
            {
                return Enumerable.Empty<SearchInArticlesResultItem>();
            }

            using (new QPConnectionScope())
            {
                var resultArticleId = int.TryParse(searchString, out var articleId) && articleId > 0 ? articleId : (int?)null;
                var result = siaRepository.SearchInArticles(siteId, userId, sqlSearchString, resultArticleId, listCmd, out totalRecords).ToArray();
                if (result.Any())
                {
                    if (QPContext.DatabaseType == DatabaseType.SqlServer)
                    {
                        var wordForms = Enumerable.Empty<string>();
                        var version = siaRepository.GetSqlServerVersion();
                        if (version.Major >= 10)
                        {
                            // если это 2008 или старше - то получить словоформы через запрос к sql server
                            wordForms = siaRepository.GetWordForms(sqlSearchString);
                        }
                        else
                        {
                            wordForms = FoundTextMarker.SplitIntoWords(searchString);
                        }

                        // Выделить релевантные участки
                        foreach (var r in result)
                        {
                            r.Text = FoundTextMarker.GetRelevantMarkedText(r.Text, wordForms, 20, "<span class='seachResultHighlight'>", "</span>");
                        }
                    }
                }
                return result;

            }

        }
    }
}
