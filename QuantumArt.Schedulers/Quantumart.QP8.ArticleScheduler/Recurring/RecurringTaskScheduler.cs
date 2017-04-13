using System;
using QP8.Infrastructure.Logging;
using Quantumart.QP8.ArticleScheduler.Interfaces;
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
            var currentDateTime = _recurringService.GetCurrentDBDateTime();
            var taskRange = Tuple.Create(task.StartDate + task.StartTime, task.EndDate + task.StartTime);
            var taskRangePos = taskRange.CompareRangeTo(currentDateTime);
            var calc = CreateRecurringStartCalculator(task);

            // получить ближайшую дату начала показа статьи
            var showRangeStartDt = calc.GetStartDateBeforeSpecifiedDate(currentDateTime);
            if (!showRangeStartDt.HasValue)
            {
                return;
            }

            // Определяем время окончания показа статьи
            var showRangeEndDt = showRangeStartDt.Value + task.Duration;
            if (taskRange.CompareRangeTo(showRangeEndDt) > 0)
            {
                // Если необходимо то ограничить время окончания показа статьи правой границей диапазона задачи
                showRangeEndDt = taskRange.Item2;
            }

            // диапазон показа статьи
            Article article;
            var showRange = Tuple.Create(showRangeStartDt.Value, showRangeEndDt);

            // определить положение текущей даты относительно диапазона показа статьи
            var showRangePos = showRange.CompareRangeTo(currentDateTime);
            if (showRangePos == 0)
            {
                // внутри диапазона показа
                article = _recurringService.ShowArticle(task.ArticleId);
                if (article != null && !article.Visible)
                {
                    Logger.Log.Info($"Article [{article.Id}: {article.Name}] has been shown on customer code: {_customer.CustomerName}");
                }
            }
            else if (showRangePos > 0 && taskRangePos == 0)
            {
                // за диапазоном показа, но внутри диапазона задачи
                article = _recurringService.HideArticle(task.ArticleId);
                if (article != null && article.Visible)
                {
                    Logger.Log.Info($"Article [{article.Id}: {article.Name}] has been hidden on customer code: {_customer.CustomerName}");
                }
            }
            else if (showRangePos > 0 && taskRangePos > 0)
            {
                // за диапазоном показа и за диапазоном задачи
                article = _recurringService.HideAndCloseSchedule(task.Id);
                if (article != null && article.Visible)
                {
                    Logger.Log.Info($"Article [{article.Id}: {article.Name}] has been hidden on customer code: {_customer.CustomerName}");
                }
            }
        }

        private static IRecurringStartCalculator CreateRecurringStartCalculator(RecurringTask task)
        {
            switch (task.TaskType)
            {
                case RecurringTaskTypes.Daily:
                    return new DailyStartCalculator(task.Interval, task.StartDate, task.EndDate, task.StartTime);
                case RecurringTaskTypes.Weekly:
                    return new WeeklyStartCalculator(task.Interval, task.RecurrenceFactor, task.StartDate, task.EndDate, task.StartTime);
                case RecurringTaskTypes.Monthly:
                    return new MonthlyStartCalculator(task.Interval, task.RecurrenceFactor, task.StartDate, task.EndDate, task.StartTime);
                case RecurringTaskTypes.MonthlyRelative:
                    return new MonthlyRelativeStartCalculator(task.Interval, task.RelativeInterval, task.RecurrenceFactor, task.StartDate, task.EndDate, task.StartTime);
                default:
                    throw new ArgumentException("Неопределенный тип расписания циклической задачи: " + task.TaskType);
            }
        }
    }
}
