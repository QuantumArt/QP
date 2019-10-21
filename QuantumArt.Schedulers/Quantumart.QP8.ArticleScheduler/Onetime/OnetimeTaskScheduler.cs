using System;
using Npgsql.Logging;
using Quantumart.QP8.ArticleScheduler.Interfaces;
using Quantumart.QP8.ArticleScheduler.Recurring;
using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Services.API.ArticleScheduler;
using Quantumart.QP8.Configuration.Models;
using Quantumart.QP8.Constants;
using NLog;
using NLog.Fluent;


namespace Quantumart.QP8.ArticleScheduler.Onetime
{
    internal class OnetimeTaskScheduler : IOnetimeTaskScheduler
    {
        private readonly QaConfigCustomer _customer;
        private readonly IArticleOnetimeSchedulerService _onetimeService;
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

        public OnetimeTaskScheduler(QaConfigCustomer customer, IArticleOnetimeSchedulerService onetimeService)
        {
            _customer = customer;
            _onetimeService = onetimeService;
        }

        public void Run(ArticleScheduleTask articleTask)
        {
            var task = OnetimeTask.CreateOnetimeTask(articleTask);
            var currentTime = _onetimeService.GetCurrentDBDateTime();
            var comparison = GetTaskRange(task).CompareRangeTo(currentTime);
            if (ShouldProcessTask(task, currentTime))
            {
                ProcessTask(task, comparison);
            }
        }

        public bool ShouldProcessTask(ISchedulerTask task, DateTimeOffset dateTimeToCheck) => GetTaskRange((OnetimeTask)task).CompareRangeTo(dateTimeToCheck) >= 0;

        public bool ShouldProcessTask(ArticleScheduleTask task, DateTimeOffset dateTimeToCheck) => ShouldProcessTask(OnetimeTask.CreateOnetimeTask(task), dateTimeToCheck);

        private void ProcessTask(OnetimeTask task, int comparison)
        {
            if (comparison > 0)
            {
                var article = _onetimeService.HideAndCloseSchedule(task.Id);
                if (article != null && article.Visible)
                {
                    Logger.Info()
                        .Message(
                        "Article [{id}: {name}] has been hidden on customer code: {customerCode}",
                        article.Id, article.Name, _customer.CustomerName)
                        .Write();

                }
            }

            if (comparison == 0)
            {
                var article = task.EndDateTime.Year == ArticleScheduleConstants.Infinity.Year
                    ? _onetimeService.ShowAndCloseSchedule(task.Id)
                    : _onetimeService.ShowArticle(task.ArticleId);

                if (article != null && !article.Visible)
                {
                    Logger.Info()
                        .Message(
                            "Article [{id}: {name}] has been shown on customer code: {customerCode}",
                            article.Id, article.Name, _customer.CustomerName)
                        .Write();
                }
            }
        }

        private static Tuple<DateTimeOffset, DateTimeOffset> GetTaskRange(OnetimeTask task) => Tuple.Create(task.StartDateTime, task.EndDateTime);
    }
}
