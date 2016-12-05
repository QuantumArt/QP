using System;
using System.Linq;

namespace Quantumart.QP8.ArticleScheduler.Recurring
{
    /// <summary>
    /// Вычисляет ближайшее к дате начало диапазона для Weekly расписаний
    /// </summary>
    public class WeeklyStartCalc : RecuringStartCalcBase
    {
        public WeeklyStartCalc(int interval, int recurrenceFactor,
            DateTime startDate, DateTime endDate, TimeSpan startTime)
        {
            calc = dateTime =>
                {
                    return Optimize(new Tuple<DateTime, DateTime>(startDate.Date, endDate.Date), dateTime.Date)
                        .EveryWeeks(recurrenceFactor) // получаем полные недели, но только те, которые ограничены recurrenceFactor
                        .Days() // получаем дни
                        .Where(d => IntervalPredicate(d, interval))
                        .Where(d => startDate.Date <= d.Date && endDate.Date >= d.Date) // только те даты что в диапазоне
                        .Select(d => d.Add(startTime)) // получаем точное время старта
                        .Nearest(dateTime);
                };
        }

        /// <summary>
        /// Возвращает предикат для фильтрации по interval
        /// </summary>
        private static bool IntervalPredicate(DateTime dateTime, int interval)
        {
            return (interval & 1) == 1 && dateTime.DayOfWeek == DayOfWeek.Sunday ||
                   (interval & 2) == 2 && dateTime.DayOfWeek == DayOfWeek.Monday ||
                   (interval & 4) == 4 && dateTime.DayOfWeek == DayOfWeek.Tuesday ||
                   (interval & 8) == 8 && dateTime.DayOfWeek == DayOfWeek.Wednesday ||
                   (interval & 16) == 16 && dateTime.DayOfWeek == DayOfWeek.Thursday ||
                   (interval & 32) == 32 && dateTime.DayOfWeek == DayOfWeek.Friday ||
                   (interval & 64) == 64 && dateTime.DayOfWeek == DayOfWeek.Saturday;
        }
    }
}
