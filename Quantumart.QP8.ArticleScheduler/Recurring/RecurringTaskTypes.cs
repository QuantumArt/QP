using Quantumart.QP8.Constants;

namespace Quantumart.QP8.ArticleScheduler.Recurring
{
    /// <summary>
    /// Типы расписаний циклических задач
    /// </summary>
    public enum RecurringTaskTypes
    {
        Daily = ScheduleFreqTypes.RecurringDaily,
        Weekly = ScheduleFreqTypes.RecurringWeekly,
        Monthly = ScheduleFreqTypes.RecurringMonthly,
        MonthlyRelative = ScheduleFreqTypes.RecurringMonthlyRelative
    }
}
