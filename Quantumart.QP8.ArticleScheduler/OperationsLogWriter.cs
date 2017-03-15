using System;
using System.Data.SqlClient;
using QP8.Infrastructure.Logging;
using Quantumart.QP8.BLL;
using Quantumart.QP8.Resources;

namespace Quantumart.QP8.ArticleScheduler
{
    internal class OperationsLogWriter : IOperationsLogWriter
    {
        private readonly string _connectionString;

        public OperationsLogWriter(string connectionString)
        {
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new ArgumentNullException(nameof(connectionString));
            }

            _connectionString = connectionString;
        }

        public void ShowArticle(Article article)
        {
            if (article != null)
            {
                var message = FormatMessage(ArticleSchedulerStrings.ArticleHasBeenShown, article);
                Logger.Log.Info(message);
            }
        }

        public void HideArticle(Article article)
        {
            if (article != null)
            {
                var message = FormatMessage(ArticleSchedulerStrings.ArticleHasBeenHidden, article);
                Logger.Log.Info(message);
            }
        }

        public void PublishArticle(Article article)
        {
            if (article != null)
            {
                var message = FormatMessage(ArticleSchedulerStrings.ArticleHasBeenPublished, article);
                Logger.Log.Info(message);
            }
        }

        private string FormatMessage(string template, EntityObject article)
        {
            var builder = new SqlConnectionStringBuilder(_connectionString);
            return string.Format(template, article.Id, article.Name, DateTime.Now, builder.DataSource, builder.InitialCatalog);
        }
    }
}
