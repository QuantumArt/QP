using System;
using System.Linq;

namespace Quantumart.QP8.ArticleScheduler.Recurring.RecurringCalculators
{
    public class DailyStartCalculator : RecurringStartCalculatorBase
    {
        private readonly int _interval;
        private readonly DateTime _startDate;
        private readonly DateTime _endDate;

        public DailyStartCalculator(int interval, DateTime startDate, DateTime endDate)
        {
            _interval = interval;
            _startDate = startDate;
            _endDate = endDate;

            CalculateNearestStartDateFunc = GetNearestStartDate;
        }

        private DateTime? GetNearestStartDate(DateTime dateTime)
        {
            return Optimize(new Tuple<DateTime, DateTime>(_startDate, _endDate), dateTime)
                .GetEveryNDayFromRange(_interval) // получаем каждый n-й день
                .GetNearestPreviousDateFromList(dateTime);
        }
    }
}
