﻿using System;
using System.Linq;

namespace Quantumart.QP8.ArticleScheduler.Recurring.RecurringCalculators
{
    public class DailyStartCalculator : RecurringStartCalculatorBase
    {
        private readonly int _interval;
        private readonly DateTimeOffset _startDate;
        private readonly DateTimeOffset _endDate;
        private readonly TimeSpan _startTime;

        public DailyStartCalculator(int interval, DateTimeOffset startDate, DateTimeOffset endDate, TimeSpan startTime)
        {
            _interval = interval;
            _startDate = startDate;
            _endDate = endDate;
            _startTime = startTime;

            CalculateNearestStartDateFunc = GetNearestStartDate;
        }

        private DateTimeOffset? GetNearestStartDate(DateTimeOffset dateTime)
        {
            return Optimize(new Tuple<DateTimeOffset, DateTimeOffset>(_startDate.Date, _endDate.Date), dateTime.Date)
                .GetEveryNDayFromRange(_interval) // получаем каждый n-й день
                .Select(d => d.Add(_startTime)) // получаем точное время старта
                .GetNearestPreviousDateFromList(dateTime);
        }
    }
}
