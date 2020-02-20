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
        private readonly DateTime _startDate;
        private readonly DateTime _endDate;

        public MonthlyRelativeStartCalculator(int interval, int relativeInterval, int recurrenceFactor, DateTime startDate, DateTime endDate)
        {
            _interval = interval;
            _relativeInterval = relativeInterval;
            _recurrenceFactor = recurrenceFactor;
            _startDate = startDate;
            _endDate = endDate;

            CalculateNearestStartDateFunc = GetNearestStartDate;
        }

        private static Func<DateTime, bool> GetIntervalPredicate(int interval)
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

        private static IEnumerable<DateTime> ApplyRelativeInternalConditions(IEnumerable<DateTime> enumerator, int relativeInterval)
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
                    return Enumerable.Empty<DateTime>();
            }
        }

        private DateTime? GetNearestStartDate(DateTime dateTime)
        {
            var allStarts = Optimize(new Tuple<DateTime, DateTime>(_startDate, _endDate), dateTime)
                .GetEveryFullMonthLimitedByFactor(_recurrenceFactor) // получаем полные месяца, но только те, которые ограничены recurrenceFactor
                .GetAllDaysFromRange()
                .Where(GetIntervalPredicate(_interval)).ToArray();

            allStarts = ApplyRelativeInternalConditions(allStarts, _relativeInterval)
                .Where(d => _startDate <= d && _endDate >= d).ToArray(); // только те даты что в диапазоне

            return allStarts.GetNearestPreviousDateFromList(dateTime); // ближайшее время старта до или если нет то null
        }
    }
}
