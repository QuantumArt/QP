using System;
using System.Collections.Generic;
using System.Data;
using System.Transactions;
using Quantumart.QP8.BLL.Repository;
using Quantumart.QP8.BLL.Repository.ArticleRepositories;
using Quantumart.QP8.Configuration;
using Quantumart.QP8.Constants;
using Quantumart.QP8.DAL;
using IsolationLevel = System.Transactions.IsolationLevel;

namespace Quantumart.QP8.BLL.Services.API.ArticleScheduler
{
    public class ArticleSchedulerService :
        IArticleSchedulerService,
        IArticleOnetimeSchedulerService,
        IArticlePublishingSchedulerService,
        IArticleRecurringSchedulerService
    {
        private readonly string _connectionString;
        private readonly DatabaseType _dbType;

        public ArticleSchedulerService(string connectionString, DatabaseType dbType)
        {
            _connectionString = connectionString;
            _dbType = dbType;
        }

        public IEnumerable<ArticleScheduleTask> GetScheduleTaskList()
        {
            QPContext.CurrentDbConnectionInfo = new QpConnectionInfo(_connectionString, _dbType);
            using (new QPConnectionScope())
            {
                return ScheduleRepository.GetScheduleTaskList();
            }
        }

        public DateTime GetCurrentDBDateTime()
        {
            QPContext.CurrentDbConnectionInfo = new QpConnectionInfo(_connectionString, _dbType);
            using (var scope = new QPConnectionScope())
            {
                return Common.GetSqlDate(scope.DbConnection);
            }
        }

        public Article ShowArticle(int articleId)
        {
            QPContext.CurrentDbConnectionInfo = new QpConnectionInfo(_connectionString, _dbType);
            using (var scope = new QPConnectionScope())
            {
                var article = ArticleRepository.GetById(articleId);
                if (article != null && !article.Visible)
                {
                    article.LoadFieldValues();
                    var repo = new NotificationPushRepository();
                    repo.PrepareNotifications(article, new[] { NotificationCode.Update });
                    Common.SetContentItemVisible(scope.DbConnection, articleId, true, article.LastModifiedBy);
                    repo.SendNotifications();
                }

                return article;
            }
        }

        public Article HideArticle(int articleId)
        {
            QPContext.CurrentDbConnectionInfo = new QpConnectionInfo(_connectionString, _dbType);
            using (var scope = new QPConnectionScope())
            {
                var article = ArticleRepository.GetById(articleId);
                if (article != null && article.Visible)
                {
                    article.LoadFieldValues();
                    var repo = new NotificationPushRepository();
                    repo.PrepareNotifications(article, new[] { NotificationCode.Update });
                    Common.SetContentItemVisible(scope.DbConnection, articleId, false, article.LastModifiedBy);
                    repo.SendNotifications();
                }

                return article;
            }
        }

        public Article ShowAndCloseSchedule(int scheduleId)
        {
            Article article = null;
            using (var transaction = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted }))
            {
                QPContext.CurrentDbConnectionInfo = new QpConnectionInfo(_connectionString, _dbType);
                using (new QPConnectionScope())
                {
                    var schedule = ScheduleRepository.GetScheduleById(scheduleId);
                    if (schedule != null)
                    {
                        article = ShowArticle(schedule.ArticleId);
                        schedule.Article = article;
                        ScheduleRepository.Delete(schedule);
                    }
                }

                transaction.Complete();
            }

            return article;
        }

        public Article HideAndCloseSchedule(int scheduleId)
        {
            Article article = null;
            using (var transaction = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted }))
            {
                QPContext.CurrentDbConnectionInfo = new QpConnectionInfo(_connectionString, _dbType);
                using (new QPConnectionScope())
                {
                    var schedule = ScheduleRepository.GetScheduleById(scheduleId);
                    if (schedule != null)
                    {
                        article = HideArticle(schedule.ArticleId);
                        schedule.Article = article;
                        ScheduleRepository.Delete(schedule);
                    }
                }

                transaction.Complete();
            }

            return article;
        }

        public Article PublishAndCloseSchedule(int scheduleId)
        {
            Article article = null;
            using (var transaction = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted }))
            {
                QPContext.CurrentDbConnectionInfo = new QpConnectionInfo(_connectionString, _dbType);
                using (var scope = new QPConnectionScope())
                {
                    var schedule = ScheduleRepository.GetScheduleById(scheduleId);
                    if (schedule != null)
                    {
                        article = ArticleRepository.GetById(schedule.ArticleId);
                        schedule.Article = article;
                        if (article != null && article.Delayed)
                        {
                            QPContext.IsLive = true;
                            article.LoadFieldValues();
                            QPContext.IsLive = false;

                            var repo = new NotificationPushRepository();
                            repo.PrepareNotifications(article, new[] { NotificationCode.DelayedPublication });
                            Common.MergeArticle(scope.DbConnection, schedule.ArticleId, article.LastModifiedBy);
                            repo.SendNotifications();
                        }
                        else
                        {
                            ScheduleRepository.Delete(schedule);
                        }
                    }
                }

                transaction.Complete();
            }

            return article;
        }
    }
}
