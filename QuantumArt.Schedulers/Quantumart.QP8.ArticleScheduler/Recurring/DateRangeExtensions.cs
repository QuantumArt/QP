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
        internal static int CompareRangeTo(this Tuple<DateTimeOffset, DateTimeOffset> range, DateTimeOffset dt)
        {
            if (range.Item1.ToUniversalTime() > dt.ToUniversalTime())
            {
                return -1;
            }

            return range.Item2.ToUniversalTime() < dt.ToUniversalTime() ? 1 : 0;
        }

        internal static IEnumerable<Tuple<DateTimeOffset, DateTimeOffset>> GetEveryFullMonthLimitedByFactor(this Tuple<DateTimeOffset, DateTimeOffset> range, int recurrenceFactor)
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

        internal static IEnumerable<Tuple<DateTimeOffset, DateTimeOffset>> GetEveryFullWeekLimitedByFactor(this Tuple<DateTimeOffset, DateTimeOffset> range, int recurrenceFactor)
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

        internal static IEnumerable<DateTimeOffset> GetEveryNDayFromRange(this Tuple<DateTimeOffset, DateTimeOffset> range, int n)
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

        internal static IEnumerable<DateTimeOffset> GetAllDaysFromRange(this IEnumerable<Tuple<DateTimeOffset, DateTimeOffset>> ranges)
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

        internal static IEnumerable<DateTimeOffset> GetEveryNDayGroupedByMonth(this IEnumerable<DateTimeOffset> days, int number)
        {
            Ensure.That<ArgumentOutOfRangeException>(number >= 1 && number <= 31, $"Day number: {number} is out of range");
            return (from day in days
                group day by day.GetMonthStartDate()
                into g
                select g.OrderBy(d => d).Skip(number - 1).FirstOrDefault()).Where(d => !d.Equals(default(DateTime)));
        }

        internal static IEnumerable<DateTimeOffset> GetEveryLastDayGroupedByMonth(this IEnumerable<DateTimeOffset> days)
        {
            return (from day in days
                    group day by day.GetMonthStartDate()
                    into g
                    select g.OrderBy(d => d).LastOrDefault())
                .Where(d => !d.Equals(default(DateTime)));
        }

        internal static DateTimeOffset? GetNearestPreviousDateFromList(this IEnumerable<DateTimeOffset> dates, DateTimeOffset dateTime)
        {
            var previouses = dates.Where(d => d <= dateTime).ToList();
            if (previouses.Any())
            {
                return previouses.Max();
            }

            return null;
        }

        internal static DateTimeOffset GetWeekStartDate(this DateTimeOffset dt)
        {
            var iteratorDate = dt;
            while (iteratorDate.DayOfWeek != DayOfWeek.Monday)
            {
                iteratorDate = iteratorDate.AddDays(-1);
            }

            return iteratorDate.Date;
        }

        internal static DateTimeOffset GetMonthStartDate(this DateTimeOffset dt) => new DateTimeOffset(new DateTime(dt.Year, dt.Month, 1));

        internal static bool IsWeekend(this DateTimeOffset dt) => dt.DayOfWeek == DayOfWeek.Saturday || dt.DayOfWeek == DayOfWeek.Sunday;

        internal static bool IsWeekday(this DateTimeOffset dt) => !IsWeekend(dt);
    }
}
