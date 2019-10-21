using System;
using Quantumart.QP8.ArticleScheduler.Interfaces;

namespace Quantumart.QP8.ArticleScheduler.Recurring.RecurringCalculators
{
    public abstract class RecurringStartCalculatorBase : IRecurringStartCalculator
    {
        protected Func<DateTimeOffset, DateTimeOffset?> CalculateNearestStartDateFunc;

        public virtual DateTimeOffset? GetNearestStartDateBeforeSpecifiedDate(DateTimeOffset dateTime) => CalculateNearestStartDateFunc(dateTime);

        public Tuple<DateTimeOffset, DateTimeOffset> Optimize(Tuple<DateTimeOffset, DateTimeOffset> range, DateTimeOffset dt)
        {
            var position = range.CompareRangeTo(dt);
            if (position < 0)
            {
                return new Tuple<DateTimeOffset, DateTimeOffset>(range.Item1, range.Item1);
            }

            return position == 0 ? new Tuple<DateTimeOffset, DateTimeOffset>(range.Item1, dt.Date) : range;
        }
    }
}
