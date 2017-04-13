using System;
using Quantumart.QP8.ArticleScheduler.Interfaces;

namespace Quantumart.QP8.ArticleScheduler.Recurring.RecurringCalculators
{
    internal class RecurringStartCalculatorFactory
    {
        internal static IRecurringStartCalculator Create(RecurringTask task)
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
