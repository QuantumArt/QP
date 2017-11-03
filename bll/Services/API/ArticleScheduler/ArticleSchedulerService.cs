using System;
using System.Collections.Generic;
using System.Transactions;
using Quantumart.QP8.BLL.Repository;
using Quantumart.QP8.BLL.Repository.ArticleRepositories;
using Quantumart.QP8.Constants;
using Quantumart.QP8.DAL;

namespace Quantumart.QP8.BLL.Services.API.ArticleScheduler
{
    public class ArticleSchedulerService :
        IArticleSchedulerService,
        IArticleOnetimeSchedulerService,
        IArticlePublishingSchedulerService,
        IArticleRecurringSchedulerService
    {
        private readonly string _connectionString;

        public ArticleSchedulerService(string connectionString)
        {
            _connectionString = connectionString;
        }

        public IEnumerable<ArticleScheduleTask> GetScheduleTaskList()
        {
            using (new QPConnectionScope(_connectionString))
            {
                return ScheduleRepository.GetScheduleTaskList();
            }
        }

        public DateTime GetCurrentDBDateTime()
        {
            using (var scope = new QPConnectionScope(_connectionString))
            {
                return Common.GetSqlDate(scope.DbConnection);
            }
        }

        public Article ShowArticle(int articleId)
        {
            using (new QPConnectionScope(_connectionString))
            {
                var article = ArticleRepository.GetById(articleId);
                if (article != null && !article.Visible)
                {
                    article.LoadFieldValues();
                    var repo = new NotificationPushRepository();
                    repo.PrepareNotifications(article, new[] { NotificationCode.Update });
                    QPContext.EFContext.SetContentItemVisible(articleId, true, article.LastModifiedBy);
                    repo.SendNotifications();
                }

                return article;
            }
        }

        public Article HideArticle(int articleId)
        {
            using (new QPConnectionScope(_connectionString))
            {
                var article = ArticleRepository.GetById(articleId);
                if (article != null && article.Visible)
                {
                    article.LoadFieldValues();
                    var repo = new NotificationPushRepository();
                    repo.PrepareNotifications(article, new[] { NotificationCode.Update });
                    QPContext.EFContext.SetContentItemVisible(articleId, false, article.LastModifiedBy);
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
                using (new QPConnectionScope(_connectionString))
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
                using (new QPConnectionScope(_connectionString))
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
                using (new QPConnectionScope(_connectionString))
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
                            QPContext.EFContext.MergeArticle(schedule.ArticleId, article.LastModifiedBy);
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
