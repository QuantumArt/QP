namespace Quantumart.QP8.Utils.FullTextSearch
{
    /// <summary>
    /// Интерфейс парсера поисковой строки в параметры полнотекстового поиска для Sql Server
    /// </summary>
    public interface ISearchGrammarParser
    {
        /// <summary>
        /// Парсит поисковую строку
        /// </summary>
        /// <param name="queryString">Поисковая строка</param>
        /// <param name="result">результат парсинга</param>
        /// <returns>True - успешно</returns>
        bool TryParse(string queryString, out string result);
    }
}
