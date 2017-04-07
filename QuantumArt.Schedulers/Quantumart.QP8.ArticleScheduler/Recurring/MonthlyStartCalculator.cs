using System;
using System.Linq;

namespace Quantumart.QP8.ArticleScheduler.Recurring
{
    /// <summary>
    /// Вычисляет ближайшее к дате начало диапазона для Monthly расписаний
    /// </summary>
    public class MonthlyStartCalculator : RecurringStartCalculatorBase
    {
        public MonthlyStartCalculator(int interval, int recurrenceFactor, DateTime startDate, DateTime endDate, TimeSpan startTime)
        {
            CalculateNearestStartDateFunc = dateTime =>
            {
                return Optimize(new Tuple<DateTime, DateTime>(startDate.Date, endDate.Date), dateTime.Date)
                    .GetEveryFullMonthLimitedByFactor(recurrenceFactor) // получаем полные месяца, но только те, которые ограничены recurrenceFactor
                    .GetAllDaysFromRange() // получаем даты
                    .GetEveryNDayGroupedByMonth(interval) // получить только те дни в каждом месяце которые соответствуют interval
                    .Where(d => startDate.Date <= d.Date && endDate.Date >= d.Date) // только те даты что в диапазоне
                    .Select(d => d.Add(startTime)) // получаем точное время старта
                    .GetNearestPreviousDateFromList(dateTime);
            };
        }
    }
}
