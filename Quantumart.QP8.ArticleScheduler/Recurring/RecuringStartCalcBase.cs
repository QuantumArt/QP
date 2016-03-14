using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Quantumart.QP8.ArticleScheduler.Recurring
{
	public abstract class RecuringStartCalcBase : IRecuringStartCalc
	{
		/// <summary>
		/// Вычисляет ближайшую дату старта
		/// </summary>
		protected Func<DateTime, DateTime?> calc;

		/// <summary>
		/// Оптимизирует размер диапазона
		/// так как нам нужны даты старта только преджествующие указанной дате, то:
		/// если указанная дата еще не в диапазоне, то конец диапазона можно сделать равным началу
		/// если указанная дата в диапазоне, то конец диапазона можно сделать равным указанной дате
		/// </summary>
		/// <param name="?"></param>
		/// <returns></returns>
		protected Tuple<DateTime, DateTime> Optimize(Tuple<DateTime, DateTime> range, DateTime dt)
		{
			var pos = range.Position(dt);
			if (pos < 0)
				return new Tuple<DateTime, DateTime>(range.Item1, range.Item1);
			else if (pos == 0)
				return new Tuple<DateTime, DateTime>(range.Item1, dt.Date);
			else
				return range;
		}

		/// <summary>
		/// Получить время старта до указанной даты иначе null
		/// </summary>
		/// <param name="dateTime"></param>
		/// <returns></returns>
		public virtual DateTime? GetStart(DateTime dateTime)
		{
			return calc(dateTime);
		}
	}
}
