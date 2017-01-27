namespace Quantumart.QP8.Constants
{
    /// <summary>
    /// Возможные значения FreqTypes у расписаний показа статей
    /// </summary>
    public static class ScheduleFreqTypes
    {
        public const int None = 0;
        public const int OneTime = 1;
        public const int Publishing = 2;
        public const int RecurringDaily = 4;
        public const int RecurringWeekly = 8;
        public const int RecurringMonthly = 16;
        public const int RecurringMonthlyRelative = 32;
    }
}
