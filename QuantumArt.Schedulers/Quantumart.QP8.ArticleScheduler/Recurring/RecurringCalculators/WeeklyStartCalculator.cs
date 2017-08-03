using System;
using System.Linq;

namespace Quantumart.QP8.ArticleScheduler.Recurring.RecurringCalculators
{
    public class WeeklyStartCalculator : RecurringStartCalculatorBase
    {
        private readonly int _interval;
        private readonly int _recurrenceFactor;
        private readonly DateTime _startDate;
        private readonly DateTime _endDate;
        private readonly TimeSpan _startTime;

        public WeeklyStartCalculator(int interval, int recurrenceFactor, DateTime startDate, DateTime endDate, TimeSpan startTime)
        {
            _interval = interval;
            _recurrenceFactor = recurrenceFactor;
            _startDate = startDate;
            _endDate = endDate;
            _startTime = startTime;

            CalculateNearestStartDateFunc = GetNearestStartDate;
        }

        private static bool IntervalPredicate(DateTime dateTime, int interval) =>
            (interval & 1) == 1 && dateTime.DayOfWeek == DayOfWeek.Sunday ||
            (interval & 2) == 2 && dateTime.DayOfWeek == DayOfWeek.Monday ||
            (interval & 4) == 4 && dateTime.DayOfWeek == DayOfWeek.Tuesday ||
            (interval & 8) == 8 && dateTime.DayOfWeek == DayOfWeek.Wednesday ||
            (interval & 16) == 16 && dateTime.DayOfWeek == DayOfWeek.Thursday ||
            (interval & 32) == 32 && dateTime.DayOfWeek == DayOfWeek.Friday ||
            (interval & 64) == 64 && dateTime.DayOfWeek == DayOfWeek.Saturday;

        private DateTime? GetNearestStartDate(DateTime dateTime)
        {
            return Optimize(new Tuple<DateTime, DateTime>(_startDate.Date, _endDate.Date), dateTime.Date)
                .GetEveryFullWeekLimitedByFactor(_recurrenceFactor) // получаем полные недели, но только те, которые ограничены recurrenceFactor
                .GetAllDaysFromRange()
                .Where(d => IntervalPredicate(d, _interval))
                .Where(d => _startDate.Date <= d.Date && _endDate.Date >= d.Date) // только те даты что в диапазоне
                .Select(d => d.Add(_startTime)) // получаем точное время старта
                .GetNearestPreviousDateFromList(dateTime);
        }
    }
}
