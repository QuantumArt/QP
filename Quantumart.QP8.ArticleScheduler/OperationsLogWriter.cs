using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Practices.EnterpriseLibrary.Logging;
using Microsoft.Practices.EnterpriseLibrary.Common.Configuration;
using System.Diagnostics;
using Quantumart.QP8.BLL;
using Quantumart.QP8.Resources;
using System.Data.SqlClient;

namespace Quantumart.QP8.ArticleScheduler
{
	/// <summary>
	/// Бизнес-логгер
	/// </summary>
	internal interface IOperationsLogWriter
	{
		void ShowArticle(Article article);
		void HideArticle(Article article);
		void PublishArticle(Article article);
	}

	class OperationsLogWriter : IOperationsLogWriter
	{
		private static readonly string OPERATIONS_CATEGORY_NAME = "Operations";
		private static readonly string OPERATIONS_LOG_ENTRY_TITLE = "Article schedule operation report";

		LogWriter writer;
		string connectionString;

		public OperationsLogWriter(string connectionString)
		{
			if (String.IsNullOrEmpty(connectionString))
				throw new ArgumentNullException("connectionString");

			this.connectionString = connectionString;
			writer = EnterpriseLibraryContainer.Current.GetInstance<LogWriter>();
		}		

		private void Write(string message)
		{
			LogEntry logEntry = new LogEntry()
			{
				Severity = TraceEventType.Information,
				Title = OPERATIONS_LOG_ENTRY_TITLE,				
				Message = message
			};
			logEntry.Categories.Add(OPERATIONS_CATEGORY_NAME);

			writer.Write(logEntry);
		}

		#region IOperationsLogWriter Members

		public void ShowArticle(Article article)
		{
			if (article != null)
			{
				string message = FormatMessage(ArticleSchedulerStrings.ArticleHasBeenShown, article);
				Write(message);
			}
		}

		public void HideArticle(Article article)
		{
			if (article != null)
			{
				string message = FormatMessage(ArticleSchedulerStrings.ArticleHasBeenHidden, article);
				Write(message);
			}
		}

		public void PublishArticle(Article article)
		{
			if (article != null)
			{				
				string message = FormatMessage(ArticleSchedulerStrings.ArticleHasBeenPublished, article);
				Write(message);				
			}
		}

		private string FormatMessage(string template, Article article)
		{
			SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder(connectionString);
			return String.Format(template, article.Id, article.Name, DateTime.Now, builder.DataSource, builder.InitialCatalog);
		}

		#endregion
	}
}
