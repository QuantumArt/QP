using System;
using System.Collections.Generic;
using System.Data;
using AutoMapper;
using Quantumart.QP8.BLL.Helpers;
using Quantumart.QP8.Utils;

namespace Quantumart.QP8.BLL.Repository.Articles
{
	public interface ISearchInArticlesRepository
	{
		/// <summary>
		/// Выполнить поиск
		/// </summary>
		/// <param name="siteId"></param>
		/// <param name="sqlSearchString"></param>
		/// <param name="listCmd"></param>
		/// <param name="totalRecords"></param>
		/// <returns></returns>
		IEnumerable<SearchInArticlesResultItem> SearchInArticles(int siteId, int userId, string sqlSearchString, int? articleId, ListCommand listCmd, out int totalRecords);
		/// <summary>
		/// Получить версию SQL Server
		/// </summary>
		/// <returns></returns>
		Version GetSqlServerVersion();
		/// <summary>
		/// Получить словоформы для строки запроса
		/// </summary>
		/// <param name="sqlSearchString"></param>
		/// <returns></returns>
		IEnumerable<string> GetWordForms(string sqlSearchString);
	}

	public class SearchInArticlesRepository : ISearchInArticlesRepository
	{		
		#region ISearchInArticlesRepository Members

		public IEnumerable<SearchInArticlesResultItem> SearchInArticles(int siteId, int userId, string sqlSearchString, int? articleId, ListCommand listCmd, out int totalRecords)
		{
			// Получить данные из БД
			var dt = QPContext.EFContext.SearchInArticles(siteId, userId, sqlSearchString, articleId, TranslateSortExpression(listCmd.SortExpression), listCmd.StartRecord, listCmd.PageSize, out totalRecords);

			// Транформировать в Biz коллекцию			
			var result = Mapper.Map<IDataReader, IEnumerable<SearchInArticlesResultItem>>(dt.CreateDataReader());
			// Удалить тэги
			foreach (var item in result)
			{
				item.Text = Cleaner.RemoveAllHtmlTags(item.Text);
			}

			return result;
		}

		public Version GetSqlServerVersion() => QPContext.EFContext.GetSqlServerVersion();

	    public IEnumerable<string> GetWordForms(string sqlSearchString) => QPContext.EFContext.GetWordForms(sqlSearchString);

	    #endregion

		/// <summary>
		/// Траслирует SortExpression 
		/// </summary>
		/// <param name="sortExpression">SortExpression</param>
		/// <returns>SortExpression</returns>
		private static string TranslateSortExpression(string sortExpression)
		{
			var replaces = new Dictionary<string, string>
			{ 
				{ "Text", "data.[rank]" },
 				{ "Id" , "data.content_item_id" },
				{ "Created" , "ci.created" },
				{ "Modified" , "ci.modified" },
				{ "LastModifiedByUser", "usr.[LOGIN]" },
				{ "ParentName", "c.CONTENT_NAME" },
				{ "StatusName", "st.STATUS_TYPE_NAME" }
			};
			return TranslateHelper.TranslateSortExpression(sortExpression, replaces, "data.[rank] desc");
		}		
	}
}
