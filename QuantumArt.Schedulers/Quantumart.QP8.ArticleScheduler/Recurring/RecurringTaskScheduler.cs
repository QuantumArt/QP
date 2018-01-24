using System;
using QP8.Infrastructure;
using QP8.Infrastructure.Logging;
using Quantumart.QP8.ArticleScheduler.Interfaces;
using Quantumart.QP8.ArticleScheduler.Recurring.RecurringCalculators;
using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Services.API.ArticleScheduler;
using Quantumart.QP8.Configuration.Models;

namespace Quantumart.QP8.ArticleScheduler.Recurring
{
    internal class RecurringTaskScheduler : IRecurringTaskScheduler
    {
        private readonly QaConfigCustomer _customer;
        private readonly IArticleRecurringSchedulerService _recurringService;

        public RecurringTaskScheduler(QaConfigCustomer customer, IArticleRecurringSchedulerService recurringService)
        {
            _customer = customer;
            _recurringService = recurringService;
        }

        public void Run(ArticleScheduleTask articleTask)
        {
            var task = RecurringTask.Create(articleTask);
            var currentTime = _recurringService.GetCurrentDBDateTime();
            var taskRange = GetTaskRange(task);
            var comparison = taskRange.CompareRangeTo(currentTime);
            if (ShouldProcessTask(task, currentTime))
            {
                ProcessTask(task, currentTime, comparison);
            }
        }

        public bool ShouldProcessTask(ISchedulerTask task, DateTime dateTimeToCheck)
        {
            var recurringTask = (RecurringTask)task;
            var nearestStartDate = RecurringStartCalculatorFactory.Create(recurringTask).GetNearestStartDateBeforeSpecifiedDate(dateTimeToCheck);
            if (!nearestStartDate.HasValue)
            {
                return false;
            }

            var comparison = GetTaskRange(recurringTask).CompareRangeTo(dateTimeToCheck);
            var nearestComparisonToShowArticle = GetNearestComparison(recurringTask, dateTimeToCheck);
            return nearestComparisonToShowArticle == 0 || nearestComparisonToShowArticle > 0 && comparison >= 0;
        }

        public bool ShouldProcessTask(ArticleScheduleTask task, DateTime dateTimeToCheck) => ShouldProcessTask(RecurringTask.Create(task), dateTimeToCheck);

        private void ProcessTask(RecurringTask task, DateTime currentTime, int comparison)
        {
            var nearestComparisonToShowArticle = GetNearestComparison(task, currentTime);
            if (nearestComparisonToShowArticle == 0)
            {
                var articleWithinShowRange = _recurringService.ShowArticle(task.ArticleId);
                if (articleWithinShowRange != null && !articleWithinShowRange.Visible)
                {
                    Logger.Log.Info($"Article [{articleWithinShowRange.Id}: {articleWithinShowRange.Name}] has been shown on customer code: {_customer.CustomerName}");
                }
            }
            else if (nearestComparisonToShowArticle > 0 && comparison == 0)
            {
                var articleOutOfShowRange = _recurringService.HideArticle(task.ArticleId);
                if (articleOutOfShowRange != null && articleOutOfShowRange.Visible)
                {
                    Logger.Log.Info($"Article [{articleOutOfShowRange.Id}: {articleOutOfShowRange.Name}] has been hidden on customer code: {_customer.CustomerName}");
                }
            }
            else if (nearestComparisonToShowArticle > 0 && comparison > 0)
            {
                var articleOutOfShowAndTaskRanges = _recurringService.HideAndCloseSchedule(task.Id);
                if (articleOutOfShowAndTaskRanges != null && articleOutOfShowAndTaskRanges.Visible)
                {
                    Logger.Log.Info($"Article [{articleOutOfShowAndTaskRanges.Id}: {articleOutOfShowAndTaskRanges.Name}] has been hidden on customer code: {_customer.CustomerName}");
                }
            }
        }

        private static int GetNearestComparison(RecurringTask task, DateTime currentTime)
        {
            var recurringCalculator = RecurringStartCalculatorFactory.Create(task);
            var nearestStartDate = recurringCalculator.GetNearestStartDateBeforeSpecifiedDate(currentTime);
            Ensure.NotNull(nearestStartDate);

            // ReSharper disable once PossibleInvalidOperationException
            var nearestEndDate = GetNearestEndDate(task, nearestStartDate.Value + task.Duration);
            var nearestTaskRangeToShowArticle = GetTaskRange(nearestStartDate.Value, nearestEndDate);
            return nearestTaskRangeToShowArticle.CompareRangeTo(currentTime);
        }

        private static DateTime GetNearestEndDate(RecurringTask task, DateTime nearestEndDate)
        {
            var taskRange = GetTaskRange(task);
            return taskRange.CompareRangeTo(nearestEndDate) > 0 ? taskRange.Item2 : nearestEndDate;
        }

        private static Tuple<DateTime, DateTime> GetTaskRange(RecurringTask task) => GetTaskRange(task.StartDate + task.StartTime, task.EndDate + task.StartTime);

        private static Tuple<DateTime, DateTime> GetTaskRange(DateTime startDateTime, DateTime endDateTime) => Tuple.Create(startDateTime, endDateTime);
    }
}
