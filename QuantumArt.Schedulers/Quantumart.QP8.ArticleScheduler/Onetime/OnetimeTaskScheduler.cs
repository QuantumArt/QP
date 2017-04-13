using System;
using QP8.Infrastructure.Logging;
using Quantumart.QP8.ArticleScheduler.Interfaces;
using Quantumart.QP8.ArticleScheduler.Recurring;
using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Services.ArticleScheduler;
using Quantumart.QP8.Configuration.Models;
using Quantumart.QP8.Constants;

namespace Quantumart.QP8.ArticleScheduler.Onetime
{
    internal class OnetimeTaskScheduler : IOnetimeTaskScheduler
    {
        private readonly QaConfigCustomer _customer;
        private readonly IArticleOnetimeSchedulerService _onetimeService;

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

        public bool ShouldProcessTask(ISchedulerTask task, DateTime dateTimeToCheck)
        {
            return GetTaskRange((OnetimeTask)task).CompareRangeTo(dateTimeToCheck) >= 0;
        }

        public bool ShouldProcessTask(ArticleScheduleTask task, DateTime dateTimeToCheck)
        {
            return ShouldProcessTask(OnetimeTask.CreateOnetimeTask(task), dateTimeToCheck);
        }

        private void ProcessTask(OnetimeTask task, int comparison)
        {
            if (comparison > 0)
            {
                var article = _onetimeService.HideAndCloseSchedule(task.Id);
                if (article != null && article.Visible)
                {
                    Logger.Log.Info($"Article [{article.Id}: {article.Name}] has been hidden on customer code: {_customer.CustomerName}");
                }
            }

            if (comparison == 0)
            {
                var article = task.EndDateTime.Year == ArticleScheduleConstants.Infinity.Year
                    ? _onetimeService.ShowAndCloseSchedule(task.Id)
                    : _onetimeService.ShowArticle(task.ArticleId);

                if (article != null && !article.Visible)
                {
                    Logger.Log.Info($"Article [{article.Id}: {article.Name}] has been shown on customer code: {_customer.CustomerName}");
                }
            }
        }

        private static Tuple<DateTime, DateTime> GetTaskRange(OnetimeTask task)
        {
            return Tuple.Create(task.StartDateTime, task.EndDateTime);
        }
    }
}
