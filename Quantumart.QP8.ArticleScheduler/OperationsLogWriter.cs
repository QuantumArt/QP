using System;
using System.Data.SqlClient;
using System.Diagnostics;
using Microsoft.Practices.EnterpriseLibrary.Common.Configuration;
using Microsoft.Practices.EnterpriseLibrary.Logging;
using Quantumart.QP8.BLL;
using Quantumart.QP8.Resources;

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

    internal class OperationsLogWriter : IOperationsLogWriter
    {
        private const string OperationsCategoryName = "Operations";
        private const string OperationsLogEntryTitle = "Article schedule operation report";

        private readonly LogWriter _writer;
        private readonly string _connectionString;

        public OperationsLogWriter(string connectionString)
        {
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new ArgumentNullException(nameof(connectionString));
            }

            _connectionString = connectionString;
            _writer = EnterpriseLibraryContainer.Current.GetInstance<LogWriter>();
        }

        private void Write(string message)
        {
            var logEntry = new LogEntry
            {
                Severity = TraceEventType.Information,
                Title = OperationsLogEntryTitle,
                Message = message
            };

            logEntry.Categories.Add(OperationsCategoryName);
            _writer.Write(logEntry);
        }

        public void ShowArticle(Article article)
        {
            if (article != null)
            {
                var message = FormatMessage(ArticleSchedulerStrings.ArticleHasBeenShown, article);
                Write(message);
            }
        }

        public void HideArticle(Article article)
        {
            if (article != null)
            {
                var message = FormatMessage(ArticleSchedulerStrings.ArticleHasBeenHidden, article);
                Write(message);
            }
        }

        public void PublishArticle(Article article)
        {
            if (article != null)
            {
                var message = FormatMessage(ArticleSchedulerStrings.ArticleHasBeenPublished, article);
                Write(message);
            }
        }

        private string FormatMessage(string template, Article article)
        {
            var builder = new SqlConnectionStringBuilder(_connectionString);
            return string.Format(template, article.Id, article.Name, DateTime.Now, builder.DataSource, builder.InitialCatalog);
        }
    }
}
