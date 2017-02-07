namespace Quantumart.QP8.Constants
{
    /// <summary>
    /// Константы для валидации расписаний показа
    /// </summary>
    public static class ScheduleValidationConstants
    {
        public const int ScheduleRecurringMinValue = 1;
        public const int ScheduleRecurringMaxValue = 100;

        public const int DayOfMonthMinValue = 1;
        public const int DayOfMonthMaxValue = 31;

        public const int DurationMinValue = 1;
        public const int DurationMaxValue = 100;
    }
}
