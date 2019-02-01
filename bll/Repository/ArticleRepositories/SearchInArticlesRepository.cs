using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using AutoMapper;
using Quantumart.QP8.BLL.Helpers;
using Quantumart.QP8.DAL;
using Quantumart.QP8.Utils;

namespace Quantumart.QP8.BLL.Repository.ArticleRepositories
{
    public interface ISearchInArticlesRepository
    {
        /// <summary>
        /// Выполнить поиск
        /// </summary>
        IEnumerable<SearchInArticlesResultItem> SearchInArticles(int siteId, int userId, string sqlSearchString, int? articleId, ListCommand listCmd, out int totalRecords);

        /// <summary>
        /// Получить версию SQL Server
        /// </summary>
        Version GetSqlServerVersion();

        /// <summary>
        /// Получить словоформы для строки запроса
        /// </summary>
        IEnumerable<string> GetWordForms(string sqlSearchString);
    }

    public class SearchInArticlesRepository : ISearchInArticlesRepository
    {
        public IEnumerable<SearchInArticlesResultItem> SearchInArticles(int siteId, int userId, string sqlSearchString, int? articleId, ListCommand listCmd, out int totalRecords)
        {
            var dt = Common.SearchInArticles(QPConnectionScope.Current.DbConnection, siteId, userId, sqlSearchString, articleId, TranslateSortExpression(listCmd.SortExpression), listCmd.StartRecord, listCmd.PageSize, out totalRecords);
            var result = Mapper.Map<IEnumerable<DataRow>, IEnumerable<SearchInArticlesResultItem>>(dt.AsEnumerable()).ToList();
            foreach (var item in result)
            {
                item.Text = Cleaner.RemoveAllHtmlTags(item.Text);
            }

            return result;
        }

        public Version GetSqlServerVersion() => Common.GetSqlServerVersion(QPConnectionScope.Current.DbConnection);

        public IEnumerable<string> GetWordForms(string sqlSearchString) => Common.GetWordForms(QPConnectionScope.Current.DbConnection, sqlSearchString);

        /// <summary>
        /// Траслирует SortExpression
        /// </summary>
        private static string TranslateSortExpression(string sortExpression)
        {
            var replaces = new Dictionary<string, string>
            {
                { "Text", "data.[rank]" },
                { "Id", "data.content_item_id" },
                { "Created", "ci.created" },
                { "Modified", "ci.modified" },
                { "LastModifiedByUser", "usr.[LOGIN]" },
                { "ParentName", "c.CONTENT_NAME" },
                { "StatusName", "st.STATUS_TYPE_NAME" }
            };

            return TranslateHelper.TranslateSortExpression(sortExpression, replaces, "data.[rank] desc");
        }
    }
}
