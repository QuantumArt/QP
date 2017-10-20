using System;
using System.Collections.Generic;
using System.Linq;
using QP8.Infrastructure;
using Quantumart.QP8.Utils;
using Quantumart.QP8.Utils.FullTextSearch;

namespace Quantumart.QP8.BLL.Repository.Articles
{
    /// <summary>
    /// Парсер Full Text Search параметров запроса для получения списка статей
    /// </summary>
    public class ArticleFullTextSearchQueryParser
    {
        private readonly ISearchGrammarParser _iSearchGrammarParser;

        public ArticleFullTextSearchQueryParser(ISearchGrammarParser iSearchGrammarParser)
        {
            Ensure.NotNull(iSearchGrammarParser);
            _iSearchGrammarParser = iSearchGrammarParser;
        }

        /// <summary>
        /// Парсит Full Text Search параметры
        /// </summary>
        /// <param name="searchQueryParams">параметры запроса</param>
        /// <param name="hasError">есть ли ошибка при парсинге поисковой строки</param>
        /// <param name="fieldIdList">список id полей(атрибутов)</param>
        /// <param name="queryString">параметры Full Text поиска для Sql Server</param>
        /// <param name="rawQueryString">Строка запроса</param>
        /// <returns>True - искать, False - не искать</returns>
        public bool Parse(IEnumerable<ArticleSearchQueryParam> searchQueryParams, out bool? hasError, out string fieldIdList, out string queryString, out string rawQueryString)
        {
            hasError = null;
            fieldIdList = null;
            queryString = null;
            rawQueryString = null;

            var processedParam = searchQueryParams?.FirstOrDefault(p => p.SearchType == ArticleFieldSearchType.FullText);
            if (processedParam == null)
            {
                return false;
            }

            var allIds = false;
            int[] ids = null;
            if (string.IsNullOrWhiteSpace(processedParam.FieldID))
            {
                allIds = true;
            }
            else
            {
                // получить ID, все что не число будет преобразовано в -1000
                ids = Converter.ToInt32Collection(processedParam.FieldID, ',', true, -1000);
                if (ids.Length == 0)
                {
                    throw new ArgumentException("FieldID");
                }

                // если есть -1000 значить есть не числа
                if (ids.Any(i => i == -1000))
                {
                    throw new FormatException("FieldID");
                }
            }

            // параметры должны быть не пустые и их не меньше 1х (используем 1й и второй - остальные отбрасываем)
            if (processedParam.QueryParams == null || processedParam.QueryParams.Length < 1)
            {
                throw new ArgumentException();
            }

            // первый параметр должен быть null или строкой
            if (processedParam.QueryParams[0] != null && !(processedParam.QueryParams[0] is string))
            {
                throw new InvalidCastException();
            }

            var qString = (string)processedParam.QueryParams[0];
            if (string.IsNullOrWhiteSpace(qString))
            {
                return false;
            }

            if (!_iSearchGrammarParser.TryParse(qString, out var sqlQString))
            {
                hasError = true;
                return true;
            }

            hasError = false;
            fieldIdList = allIds ? string.Empty : string.Join(",", ids);
            queryString = Cleaner.ToSafeSqlString(sqlQString);
            rawQueryString = qString;

            return true;
        }
    }
}
