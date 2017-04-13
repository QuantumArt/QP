﻿using System;

namespace Quantumart.QP8.ArticleScheduler.Interfaces
{
    /// <summary>
    /// Интерфейс классов определяющих начало периода в Recurring-расписаний
    /// </summary>
    public interface IRecurringStartCalculator
    {
        /// <summary>
        /// Получить время старта до указанной даты иначе null
        /// </summary>
        DateTime? GetStartDateBeforeSpecifiedDate(DateTime dateTime);

        /// <summary>
        /// Оптимизирует размер диапазона, так как нам нужны даты старта только предшествующие указанной дате, то:
        /// если указанная дата еще не в диапазоне, то конец диапазона можно сделать равным началу,
        /// если указанная дата в диапазоне - конец диапазона можно сделать равным указанной дате
        /// </summary>
        Tuple<DateTime, DateTime> Optimize(Tuple<DateTime, DateTime> range, DateTime dt);
    }
}
