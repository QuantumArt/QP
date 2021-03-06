using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using AutoMapper;
using Quantumart.QP8.BLL.Helpers;
using Quantumart.QP8.Constants;
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
            var dt = Common.SearchInArticles(QPContext.EFContext, QPConnectionScope.Current.DbConnection, siteId, userId, sqlSearchString, articleId, TranslateSortExpression(listCmd.SortExpression), listCmd.StartRecord, listCmd.PageSize, out totalRecords);
            var result = Mapper.Map<IEnumerable<DataRow>, IEnumerable<SearchInArticlesResultItem>>(dt.AsEnumerable()).ToList();
            if (QPContext.DatabaseType == DatabaseType.Postgres)
            {
                var ids = result.Select(n => n.Id).ToArray();
                var descriptions = Common.GetPgFtDescriptions(QPConnectionScope.Current.DbConnection, ids, sqlSearchString);
                foreach (var item in result)
                {
                    if (descriptions.TryGetValue(item.Id, out var text))
                    {
                        item.Text = text;
                    }
                }
            }
            else
            {
                foreach (var item in result)
                {
                    item.Text = Cleaner.RemoveAllHtmlTags(item.Text);
                }
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
            var dbType = QPContext.DatabaseType;
            if (dbType == DatabaseType.Postgres)
            {
                if (String.IsNullOrEmpty(sortExpression))
                {
                    return $@"""Rank"" desc";
                }

                var parts = sortExpression.Split(' ');
                parts[0] = $@"""{parts[0]}""";
                return string.Join(" ", parts);
            }
            var replaces = new Dictionary<string, string>()
            {
                { "Text", "Rank" }
            };

            return TranslateHelper.TranslateSortExpression(sortExpression, replaces, $"Rank desc");
        }
    }
}
