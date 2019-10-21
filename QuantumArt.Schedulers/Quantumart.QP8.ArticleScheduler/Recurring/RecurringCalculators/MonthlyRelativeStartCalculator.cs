using System;
using System.Collections.Generic;
using System.Linq;

namespace Quantumart.QP8.ArticleScheduler.Recurring.RecurringCalculators
{
    public class MonthlyRelativeStartCalculator : RecurringStartCalculatorBase
    {
        private readonly int _interval;
        private readonly int _relativeInterval;
        private readonly int _recurrenceFactor;
        private readonly DateTimeOffset _startDate;
        private readonly DateTimeOffset _endDate;
        private readonly TimeSpan _startTime;

        public MonthlyRelativeStartCalculator(int interval, int relativeInterval, int recurrenceFactor, DateTimeOffset startDate, DateTimeOffset endDate, TimeSpan startTime)
        {
            _interval = interval;
            _relativeInterval = relativeInterval;
            _recurrenceFactor = recurrenceFactor;
            _startDate = startDate;
            _endDate = endDate;
            _startTime = startTime;

            CalculateNearestStartDateFunc = GetNearestStartDate;
        }

        private static Func<DateTimeOffset, bool> GetIntervalPredicate(int interval)
        {
            switch (interval)
            {
                case 1:
                    return d => d.DayOfWeek == DayOfWeek.Sunday;
                case 2:
                    return d => d.DayOfWeek == DayOfWeek.Monday;
                case 3:
                    return d => d.DayOfWeek == DayOfWeek.Tuesday;
                case 4:
                    return d => d.DayOfWeek == DayOfWeek.Wednesday;
                case 5:
                    return d => d.DayOfWeek == DayOfWeek.Thursday;
                case 6:
                    return d => d.DayOfWeek == DayOfWeek.Friday;
                case 7:
                    return d => d.DayOfWeek == DayOfWeek.Saturday;
                case 8:
                    return d => true;
                case 9:
                    return d => d.IsWeekday();
                case 10:
                    return d => d.IsWeekend();
                default:
                    return d => false;
            }
        }

        private static IEnumerable<DateTimeOffset> ApplyRelativeInternalConditions(IEnumerable<DateTimeOffset> enumerator, int relativeInterval)
        {
            switch (relativeInterval)
            {
                case 1:
                    return enumerator.GetEveryNDayGroupedByMonth(1);
                case 2:
                    return enumerator.GetEveryNDayGroupedByMonth(2);
                case 4:
                    return enumerator.GetEveryNDayGroupedByMonth(3);
                case 8:
                    return enumerator.GetEveryNDayGroupedByMonth(4);
                case 16:
                    return enumerator.GetEveryLastDayGroupedByMonth();
                default:
                    return Enumerable.Empty<DateTimeOffset>();
            }
        }

        private DateTimeOffset? GetNearestStartDate(DateTimeOffset dateTime)
        {
            var allStarts = Optimize(new Tuple<DateTimeOffset, DateTimeOffset>(_startDate.Date, _endDate.Date), dateTime.Date)
                .GetEveryFullMonthLimitedByFactor(_recurrenceFactor) // получаем полные месяца, но только те, которые ограничены recurrenceFactor
                .GetAllDaysFromRange()
                .Where(GetIntervalPredicate(_interval));

            allStarts = ApplyRelativeInternalConditions(allStarts, _relativeInterval)
                .Where(d => _startDate.Date <= d.Date && _endDate.Date >= d.Date) // только те даты что в диапазоне
                .Select(d => d.Add(_startTime)); // получаем точное время старта

            return allStarts.GetNearestPreviousDateFromList(dateTime); // ближайшее время старта до или если нет то null
        }
    }
}
