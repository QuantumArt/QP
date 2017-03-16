using System;

namespace Quantumart.QP8.ArticleScheduler.Recurring
{
    public abstract class RecuringStartCalcBase : IRecuringStartCalc
    {
        /// <summary>
        /// Вычисляет ближайшую дату старта
        /// </summary>
        protected Func<DateTime, DateTime?> Calc;

        /// <summary>
        /// Оптимизирует размер диапазона
        /// так как нам нужны даты старта только преджествующие указанной дате, то:
        /// если указанная дата еще не в диапазоне, то конец диапазона можно сделать равным началу
        /// если указанная дата в диапазоне, то конец диапазона можно сделать равным указанной дате
        /// </summary>
        protected Tuple<DateTime, DateTime> Optimize(Tuple<DateTime, DateTime> range, DateTime dt)
        {
            var pos = range.Position(dt);
            if (pos < 0)
            {
                return new Tuple<DateTime, DateTime>(range.Item1, range.Item1);
            }

            return pos == 0 ? new Tuple<DateTime, DateTime>(range.Item1, dt.Date) : range;
        }

        /// <summary>
        /// Получить время старта до указанной даты иначе null
        /// </summary>
        public virtual DateTime? GetStart(DateTime dateTime)
        {
            return Calc(dateTime);
        }
    }
}
