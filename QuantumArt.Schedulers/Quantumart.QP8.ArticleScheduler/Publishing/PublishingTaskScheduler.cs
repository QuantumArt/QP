using System;
using QP8.Infrastructure.Logging;
using Quantumart.QP8.ArticleScheduler.Interfaces;
using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Services.API.ArticleScheduler;
using Quantumart.QP8.Configuration.Models;

namespace Quantumart.QP8.ArticleScheduler.Publishing
{
    internal class PublishingTaskScheduler : IPublishingTaskScheduler
    {
        private readonly QaConfigCustomer _customer;
        private readonly IArticlePublishingSchedulerService _publishingService;

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
                Logger.Log.Info($"Article [{article.Id}: {article.Name}] has been published on customer code: {_customer.CustomerName}");
            }
        }

        public bool ShouldProcessTask(ISchedulerTask task, DateTime dateTimeToCheck) => dateTimeToCheck >= ((PublishingTask)task).PublishingDateTime;

        public bool ShouldProcessTask(ArticleScheduleTask task, DateTime dateTimeToCheck) => ShouldProcessTask(PublishingTask.Create(task), dateTimeToCheck);
    }
}
