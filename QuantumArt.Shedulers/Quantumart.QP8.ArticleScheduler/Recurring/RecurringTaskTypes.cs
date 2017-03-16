using Quantumart.QP8.Constants;

namespace Quantumart.QP8.ArticleScheduler.Recurring
{
    public enum RecurringTaskTypes
    {
        Daily = ScheduleFreqTypes.RecurringDaily,
        Weekly = ScheduleFreqTypes.RecurringWeekly,
        Monthly = ScheduleFreqTypes.RecurringMonthly,
        MonthlyRelative = ScheduleFreqTypes.RecurringMonthlyRelative
    }
}
