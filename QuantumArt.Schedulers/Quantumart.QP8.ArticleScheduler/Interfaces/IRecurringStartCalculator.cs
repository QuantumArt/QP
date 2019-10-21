using System;

namespace Quantumart.QP8.ArticleScheduler.Interfaces
{
    /// <summary>
    /// Интерфейс классов определяющих начало периода в Recurring-расписаний
    /// Вычисляет, ближайшее к заданной дате, - начало диапазона
    /// </summary>
    public interface IRecurringStartCalculator
    {
        /// <summary>
        /// Получить ближайшее время старта до указанной даты или null
        /// </summary>
        DateTimeOffset? GetNearestStartDateBeforeSpecifiedDate(DateTimeOffset dateTime);

        /// <summary>
        /// Оптимизирует размер диапазона, так как нам нужны даты старта только предшествующие указанной дате:
        /// если указанная дата еще не в диапазоне, то конец диапазона можно сделать равным началу,
        /// если указанная дата в диапазоне - конец диапазона можно сделать равным указанной дате
        /// </summary>
        Tuple<DateTimeOffset, DateTimeOffset> Optimize(Tuple<DateTimeOffset, DateTimeOffset> range, DateTimeOffset dt);
    }
}
