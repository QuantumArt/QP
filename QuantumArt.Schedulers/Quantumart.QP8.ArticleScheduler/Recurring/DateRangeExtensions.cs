using System;
using System.Collections.Generic;
using System.Linq;
using QP8.Infrastructure;

namespace Quantumart.QP8.ArticleScheduler.Recurring
{
    internal static class DateRangeExtensions
    {
        /// <summary>
        /// Определяет положение даты относительно диапазона
        /// </summary>
        /// <returns>
        /// -1 - дата до диапазона
        /// 0 - дата в диапазоне
        /// 1 - дата после диапазона
        /// </returns>
        internal static int CompareRangeTo(this Tuple<DateTime, DateTime> range, DateTime dt)
        {
            if (range.Item1 > dt)
            {
                return -1;
            }

            return range.Item2 < dt ? 1 : 0;
        }

        internal static IEnumerable<Tuple<DateTime, DateTime>> GetEveryFullMonthLimitedByFactor(this Tuple<DateTime, DateTime> range, int recurrenceFactor)
        {
            var startRangeFirstDay = range.Item1.GetMonthStartDate();
            var endRangeFirstDay = range.Item2.GetMonthStartDate();
            var countFactor = recurrenceFactor;
            var currentDate = startRangeFirstDay;
            while (currentDate < endRangeFirstDay.AddMonths(1))
            {
                if (countFactor % recurrenceFactor == 0)
                {
                    yield return Tuple.Create(currentDate, currentDate.AddMonths(1).AddDays(-1));
                }

                currentDate = currentDate.AddMonths(1);
                countFactor++;
            }
        }

        internal static IEnumerable<Tuple<DateTime, DateTime>> GetEveryFullWeekLimitedByFactor(this Tuple<DateTime, DateTime> range, int recurrenceFactor)
        {
            var startRangeFirstDay = range.Item1.GetWeekStartDate();
            var endRangeFirstDay = range.Item2.GetWeekStartDate();
            var countFactor = recurrenceFactor;
            var currentDate = startRangeFirstDay;
            while (currentDate < endRangeFirstDay.AddDays(7))
            {
                if (countFactor % recurrenceFactor == 0)
                {
                    yield return Tuple.Create(currentDate, currentDate.AddDays(6));
                }

                currentDate = currentDate.AddDays(7);
                countFactor++;
            }
        }

        internal static IEnumerable<DateTime> GetEveryNDayFromRange(this Tuple<DateTime, DateTime> range, int n)
        {
            var startRangeFirstDay = range.Item1.Date;
            var endRangeFirstDay = range.Item2.Date;
            var countFactor = n;
            var currentDate = startRangeFirstDay;
            while (currentDate < endRangeFirstDay.AddDays(1))
            {
                if (countFactor % n == 0)
                {
                    yield return currentDate;
                }

                currentDate = currentDate.AddDays(1);
                countFactor++;
            }
        }

        internal static IEnumerable<DateTime> GetAllDaysFromRange(this IEnumerable<Tuple<DateTime, DateTime>> ranges)
        {
            foreach (var range in ranges)
            {
                var currentDate = range.Item1.Date;
                var endDate = range.Item2.Date;
                while (currentDate <= endDate)
                {
                    yield return currentDate;

                    currentDate = currentDate.AddDays(1);
                }
            }
        }

        internal static IEnumerable<DateTime> GetEveryNDayGroupedByMonth(this IEnumerable<DateTime> days, int number)
        {
            Ensure.That<ArgumentOutOfRangeException>(number >= 1 && number <= 31, $"Day number: {number} is out of range");
            return (from day in days
                group day by day.GetMonthStartDate()
                into g
                select g.OrderBy(d => d).Skip(number - 1).FirstOrDefault()).Where(d => !d.Equals(default(DateTime)));
        }

        internal static IEnumerable<DateTime> GetEveryLastDayGroupedByMonth(this IEnumerable<DateTime> days)
        {
            return (from day in days
                    group day by day.GetMonthStartDate()
                    into g
                    select g.OrderBy(d => d).LastOrDefault())
                .Where(d => !d.Equals(default(DateTime)));
        }

        internal static DateTime? GetNearestPreviousDateFromList(this IEnumerable<DateTime> dates, DateTime dateTime)
        {
            var previouses = dates.Where(d => d <= dateTime).ToList();
            if (previouses.Any())
            {
                return previouses.Max();
            }

            return null;
        }

        internal static DateTime GetWeekStartDate(this DateTime dt)
        {
            var iteratorDate = dt;
            while (iteratorDate.DayOfWeek != DayOfWeek.Monday)
            {
                iteratorDate = iteratorDate.AddDays(-1);
            }

            return iteratorDate.Date;
        }

        internal static DateTime GetMonthStartDate(this DateTime dt) => new DateTime(dt.Year, dt.Month, 1);

        internal static bool IsWeekend(this DateTime dt) => dt.DayOfWeek == DayOfWeek.Saturday || dt.DayOfWeek == DayOfWeek.Sunday;

        internal static bool IsWeekday(this DateTime dt) => !IsWeekend(dt);
    }
}
