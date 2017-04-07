using System;
using Quantumart.QP8.ArticleScheduler.Interfaces;

namespace Quantumart.QP8.ArticleScheduler.Recurring
{
    public abstract class RecurringStartCalculatorBase : IRecurringStartCalculator
    {
        protected Func<DateTime, DateTime?> CalculateNearestStartDateFunc;

        public virtual DateTime? GetStartDateBeforeSpecifiedDate(DateTime dateTime)
        {
            return CalculateNearestStartDateFunc(dateTime);
        }

        public Tuple<DateTime, DateTime> Optimize(Tuple<DateTime, DateTime> range, DateTime dt)
        {
            var pos = range.CompareRangeTo(dt);
            if (pos < 0)
            {
                return new Tuple<DateTime, DateTime>(range.Item1, range.Item1);
            }

            return pos == 0 ? new Tuple<DateTime, DateTime>(range.Item1, dt.Date) : range;
        }
    }
}
