using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Quantumart.QP8.Utils.FullTextSearch;
using System.Data;
using AutoMapper;
using Quantumart.QP8.BLL.Helpers;
using Quantumart.QP8.BLL.Repository.Articles;

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
		ISearchGrammarParser grammaParser;
		ISearchInArticlesRepository siaRepository;

		public SearchInArticlesService(ISearchGrammarParser grammaParser, ISearchInArticlesRepository siaRepository)
		{
			if (grammaParser == null)
				throw new ArgumentNullException("grammaParser");
			if (siaRepository == null)
				throw new ArgumentNullException("siaRepository");

			this.grammaParser = grammaParser;
			this.siaRepository = siaRepository;
		}		

		public IEnumerable<SearchInArticlesResultItem> SearchInArticles(int siteId, int userId, string searchString, ListCommand listCmd, out int totalRecords)
		{
			totalRecords = 0;			

			string sqlSearchString;
			if(!grammaParser.TryParse(searchString, out sqlSearchString) )
				return Enumerable.Empty<SearchInArticlesResultItem>();
			if (String.IsNullOrWhiteSpace(sqlSearchString))
				return Enumerable.Empty<SearchInArticlesResultItem>();

			int articleId;
			Int32.TryParse(searchString, out articleId);

			// Выполнить поиск
			var result = siaRepository.SearchInArticles(siteId, userId, sqlSearchString, (articleId > 0 ? articleId : (int?)null), listCmd, out totalRecords);

			if (result.Any())
			{				
				var wordForms = Enumerable.Empty<string>();
				// получить версию sql server
				var version = siaRepository.GetSqlServerVersion();
				// если это 2008 или старше - то получить словоформы через запрос к sql server
				if (version.Major >= 10)
					wordForms = siaRepository.GetWordForms(sqlSearchString);
				else
					wordForms = FoundTextMarker.SplitIntoWords(searchString);
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
