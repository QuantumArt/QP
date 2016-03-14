using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Quantumart.QP8.ArticleScheduler.Recurring
{
	/// <summary>
	/// Вычисляет ближайшее к дате начало диапазона для Monthly расписаний 
	/// </summary>
	public class MonthlyStartCalc : RecuringStartCalcBase
	{
		public MonthlyStartCalc(int interval, int recurrenceFactor,
			DateTime startDate, DateTime endDate, TimeSpan startTime)
		{
			calc = (dateTime) =>
				{
					return Optimize(new Tuple<DateTime, DateTime>(startDate.Date, endDate.Date), dateTime.Date)
					.EveryMonths(recurrenceFactor) // получаем полные месяца, но только те, которые ограничены recurrenceFactor
					.Days() // получаем даты
					.DayByMonth(interval) // получить только те дни в каждом месяце которые соответствуют interval
					.Where(d => startDate.Date <= d.Date && endDate.Date >= d.Date) // только те даты что в диапазоне
					.Select(d => d.Add(startTime)) // получаем точное время старта
					.Nearest(dateTime);
				};
		}
	}
}
