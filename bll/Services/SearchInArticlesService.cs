using System;
using System.Collections.Generic;
using System.Linq;
using Quantumart.QP8.BLL.Repository.ArticleRepositories;
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
            if (!grammaParser.TryParse(searchString, out var sqlSearchString))
            {
                return Enumerable.Empty<SearchInArticlesResultItem>();
            }

            if (string.IsNullOrWhiteSpace(sqlSearchString))
            {
                return Enumerable.Empty<SearchInArticlesResultItem>();
            }

            int.TryParse(searchString, out var articleId);
            var result = siaRepository.SearchInArticles(siteId, userId, sqlSearchString, articleId > 0 ? articleId : (int?)null, listCmd, out totalRecords);
            if (result.Any())
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

            return result;
        }
    }
}
