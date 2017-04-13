using System;
using QP8.Infrastructure.Logging;
using Quantumart.QP8.ArticleScheduler.Interfaces;
using Quantumart.QP8.ArticleScheduler.Recurring.RecurringCalculators;
using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Services.ArticleScheduler;
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
            var nearestStartDate = RecurringStartCalculatorFactory.Create(recurringTask).GetNearesStartDateBeforeSpecifiedDate(dateTimeToCheck);
            if (!nearestStartDate.HasValue)
            {
                return false;
            }

            var comparison = GetTaskRange(recurringTask).CompareRangeTo(dateTimeToCheck);
            var nearestComparisonToShowArticle = GetNearestComparison(recurringTask, dateTimeToCheck);
            return nearestComparisonToShowArticle == 0 || nearestComparisonToShowArticle > 0 && comparison >= 0;
        }

        public bool ShouldProcessTask(ArticleScheduleTask task, DateTime dateTimeToCheck)
        {
            return ShouldProcessTask(RecurringTask.Create(task), dateTimeToCheck);
        }

        private void ProcessTask(RecurringTask task, DateTime currentTime, int comparison)
        {
            var nearestComparisonToShowArticle = GetNearestComparison(task, currentTime);
            if (nearestComparisonToShowArticle == 0)
            {
                // внутри диапазона показа
                var articleWithinRangeToShow = _recurringService.ShowArticle(task.ArticleId);
                if (articleWithinRangeToShow != null && !articleWithinRangeToShow.Visible)
                {
                    Logger.Log.Info($"Article [{articleWithinRangeToShow.Id}: {articleWithinRangeToShow.Name}] has been shown on customer code: {_customer.CustomerName}");
                }
            }
            else if (nearestComparisonToShowArticle > 0 && comparison == 0)
            {
                // за диапазоном показа, но внутри диапазона задачи
                var article = _recurringService.HideArticle(task.ArticleId);
                if (article != null && article.Visible)
                {
                    Logger.Log.Info($"Article [{article.Id}: {article.Name}] has been hidden on customer code: {_customer.CustomerName}");
                }
            }
            else if (nearestComparisonToShowArticle > 0 && comparison > 0)
            {
                // за диапазоном показа и за диапазоном задачи
                var article = _recurringService.HideAndCloseSchedule(task.Id);
                if (article != null && article.Visible)
                {
                    Logger.Log.Info($"Article [{article.Id}: {article.Name}] has been hidden on customer code: {_customer.CustomerName}");
                }
            }
        }

        private static int GetNearestComparison(RecurringTask task, DateTime currentTime)
        {
            var recurringCalculator = RecurringStartCalculatorFactory.Create(task);
            var nearestStartDate = recurringCalculator.GetNearesStartDateBeforeSpecifiedDate(currentTime).Value;
            var nearestEndDate = GetNearestEndDate(task, nearestStartDate + task.Duration);
            var nearestTaskRangeToShowArticle = GetTaskRange(nearestStartDate, nearestEndDate);
            return nearestTaskRangeToShowArticle.CompareRangeTo(currentTime);
        }

        private static DateTime GetNearestEndDate(RecurringTask task, DateTime nearestEndDate)
        {
            var taskRange = GetTaskRange(task);
            return taskRange.CompareRangeTo(nearestEndDate) > 0 ? taskRange.Item2 : nearestEndDate;
        }

        private static Tuple<DateTime, DateTime> GetTaskRange(RecurringTask task)
        {
            return GetTaskRange(task.StartDate + task.StartTime, task.EndDate + task.StartTime);
        }

        private static Tuple<DateTime, DateTime> GetTaskRange(DateTime startDateTime, DateTime endDateTime)
        {
            return Tuple.Create(startDateTime, endDateTime);
        }
    }
}
