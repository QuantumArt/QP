using System;
using System.Linq;

namespace Quantumart.QP8.ArticleScheduler.Recurring
{
    /// <summary>
    /// Вычисляет ближайшее к дате начало диапазона для Weekly расписаний
    /// </summary>
    public class DailyStartCalculator : RecurringStartCalculatorBase
    {
        public DailyStartCalculator(int interval, DateTime startDate, DateTime endDate, TimeSpan startTime)
        {
            CalculateNearestStartDateFunc = dateTime =>
            {
                return Optimize(new Tuple<DateTime, DateTime>(startDate.Date, endDate.Date), dateTime.Date)
                .GetEveryNDayFromRange(interval) // получаем каждый n-й день
                .Select(d => d.Add(startTime)) // получаем точное время старта
                .GetNearestPreviousDateFromList(dateTime);
            };
        }
    }
}
