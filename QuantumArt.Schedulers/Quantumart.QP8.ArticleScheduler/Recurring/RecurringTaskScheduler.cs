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
            var taskRangePos = taskRange.Position(currentDateTime);
            var calc = CreateRecuringStartCalc(task);

            // получить ближайшую дату начала показа статьи
            var showRangeStartDt = calc.GetStart(currentDateTime);
            if (!showRangeStartDt.HasValue)
            {
                // если вычислитель вернул null - значит ничего не делаем
                return;
            }

            // Определяем время окончания показа статьи
            var showRangeEndDt = showRangeStartDt.Value + task.Duration;
            if (taskRange.Position(showRangeEndDt) > 0)
            {
                // Если необходимо то ограничить время окончания показа статьи правой границей диапазона задачи
                showRangeEndDt = taskRange.Item2;
            }

            // диапазон показа статьи
            Article article;
            var showRange = Tuple.Create(showRangeStartDt.Value, showRangeEndDt);

            // определить положение текущей даты относительно диапазона показа статьи
            var showRangePos = showRange.Position(currentDateTime);
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

        private static IRecuringStartCalc CreateRecuringStartCalc(RecurringTask task)
        {
            switch (task.TaskType)
            {
                case RecurringTaskTypes.Daily:
                    return new DailyStartCalc(task.Interval, task.StartDate, task.EndDate, task.StartTime);
                case RecurringTaskTypes.Weekly:
                    return new WeeklyStartCalc(task.Interval, task.RecurrenceFactor, task.StartDate, task.EndDate, task.StartTime);
                case RecurringTaskTypes.Monthly:
                    return new MonthlyStartCalc(task.Interval, task.RecurrenceFactor, task.StartDate, task.EndDate, task.StartTime);
                case RecurringTaskTypes.MonthlyRelative:
                    return new MonthlyRelativeStartCalc(task.Interval, task.RelativeInterval, task.RecurrenceFactor, task.StartDate, task.EndDate, task.StartTime);
                default:
                    throw new ArgumentException("Неопределенный тип расписания циклической задачи: " + task.TaskType);
            }
        }
    }
}
