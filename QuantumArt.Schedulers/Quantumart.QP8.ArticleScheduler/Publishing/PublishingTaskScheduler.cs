using System;
using Quantumart.QP8.ArticleScheduler.Interfaces;
using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Services.API.ArticleScheduler;
using Quantumart.QP8.Configuration.Models;
using NLog;
using NLog.Fluent;

namespace Quantumart.QP8.ArticleScheduler.Publishing
{
    internal class PublishingTaskScheduler : IPublishingTaskScheduler
    {
        private readonly QaConfigCustomer _customer;
        private readonly IArticlePublishingSchedulerService _publishingService;
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

        public PublishingTaskScheduler(QaConfigCustomer customer, IArticlePublishingSchedulerService publishingService)
        {
            _customer = customer;
            _publishingService = publishingService;
        }

        public void Run(ArticleScheduleTask articleTask)
        {
            var task = PublishingTask.Create(articleTask);
            var currentTime = _publishingService.GetCurrentDBDateTime();
            if (ShouldProcessTask(task, currentTime))
            {
                var article = _publishingService.PublishAndCloseSchedule(task.Id);
                Logger.Info()
                    .Message(
                        "Article [{id}: {name}] has been published on customer code: {customerCode}",
                    article.Id, article.Name, _customer.CustomerName)
                    .Write();
            }
        }

        public bool ShouldProcessTask(ISchedulerTask task, DateTimeOffset dateTimeToCheck)
        {
            return ShouldProcessTask((PublishingTask)task, dateTimeToCheck);
        }

        public bool ShouldProcessTask(PublishingTask task, DateTimeOffset dateTimeToCheck)
        {
            var dt1 = dateTimeToCheck;
            var dt2 = task.PublishingDateTime;
            var result = dt1 >= dt2 ;
            if (!result)
            {
                Logger.Trace()
                    .Message(
                        "Article [{id}] has been skipped for processing on customer code: {customerCode}." +
                        " {currentDateTime} < {publishingDateTime}. ",
                        task.ArticleId, _customer.CustomerName, dt1, dt2)
                    .Write();
            }

            return result;
        }

        public bool ShouldProcessTask(ArticleScheduleTask task, DateTimeOffset dateTimeToCheck) => ShouldProcessTask(PublishingTask.Create(task), dateTimeToCheck);
    }
}
