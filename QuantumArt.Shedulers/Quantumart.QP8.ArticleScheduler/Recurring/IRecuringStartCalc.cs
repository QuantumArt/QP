﻿using System;

namespace Quantumart.QP8.ArticleScheduler.Recurring
{
    /// <summary>
    /// Интерфейс классов определяющих начало периода в Recuring-расписаний
    /// </summary>
    public interface IRecuringStartCalc
    {
        /// <summary>
        /// Получить время старта до указанной даты иначе null
        /// </summary>
        DateTime? GetStart(DateTime dateTime);
    }
}