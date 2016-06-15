using System;
using System.Collections.Generic;
using System.Transactions;
using Quantumart.QP8.BLL.Repository;
using Quantumart.QP8.BLL.Repository.Articles;
using Quantumart.QP8.BLL.Services.ArticleScheduler;
using Quantumart.QP8.Constants;
using Quantumart.QP8.DAL;

namespace Quantumart.QP8.BLL.Services.API.ArticleScheduler
{
    public class ArticleSchedulerService : IArticleSchedulerService, IArticleOnetimeSchedulerService, IArticlePublishingSchedulerService, IArticleRecurringSchedulerService
    {
        readonly string _connectionString;

        public ArticleSchedulerService(string connectionString)
        {
            if (string.IsNullOrEmpty(connectionString))
                throw new ArgumentNullException(nameof(connectionString));

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
                if (article != null && !article.Visible) {
                    QPContext.EFContext.SetContentItemVisible(articleId, true);
                }

                article?.LoadFieldValues();
                return article;
            }		
        }

        public Article HideArticle(int articleId)
        {
            using (new QPConnectionScope(_connectionString))
            {
                var article = ArticleRepository.GetById(articleId);
                if (article != null && article.Visible) {
                    QPContext.EFContext.SetContentItemVisible(articleId, false);
                }

                article?.LoadFieldValues();
                return article;
            }
        }

        public Article ShowAndCloseSchedule(int scheduleId)
        {
            Article article = null; 
            using (var transaction = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions {IsolationLevel = IsolationLevel.ReadCommitted}))
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

            SendNotification(article, NotificationCode.Update);

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

            SendNotification(article, NotificationCode.Update);

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
                            QPContext.EFContext.MergeArticle(schedule.ArticleId);
                        }
                        else
                            ScheduleRepository.Delete(schedule);

                        article?.LoadFieldValues();
                    }
                }
                transaction.Complete();
            }

            SendNotification(article, NotificationCode.DelayedPublication);

            return article;
        }

        private static void SendNotification(Article article, string code)
        {
            if (article == null) return;
            using (new QPConnectionScope())
            {
                var rep = new NotificationPushRepository();
                rep.PrepareNotifications(article.ContentId, new[] { article.Id }, code);
                rep.SendNotifications();
            }
        }
    }
}
