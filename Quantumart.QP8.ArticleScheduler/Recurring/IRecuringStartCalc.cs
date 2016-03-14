using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
		/// <param name="dateTime"></param>
		/// <returns></returns>
		DateTime? GetStart(DateTime dateTime);
	}
}
