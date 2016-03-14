using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Quantumart.QP8.ArticleScheduler.Recurring
{
	/// <summary>
	/// Вычисляет ближайшее к дате начало диапазона для Monthly Relative расписаний 
	/// </summary>
	public class MonthlyRelativeStartCalc : RecuringStartCalcBase
	{								
		public MonthlyRelativeStartCalc(int interval, int relativeInterval, int recurrenceFactor,
			DateTime startDate, DateTime endDate, TimeSpan startTime)
		{
			calc = (dateTime) =>
				{

					IEnumerable<DateTime> allStarts = Optimize(new Tuple<DateTime, DateTime>(startDate.Date, endDate.Date), dateTime.Date)
						.EveryMonths(recurrenceFactor) // получаем полные месяца, но только те, которые ограничены recurrenceFactor
						.Days()
						.Where(GetIntervalPredicate(interval));
					allStarts = ApplyRelativeInternalConditions(allStarts, relativeInterval)
						.Where(d => startDate.Date <= d.Date && endDate.Date >= d.Date) // только те даты что в диапазоне
						.Select(d => d.Add(startTime)); // получаем точное время старта
					return allStarts.Nearest(dateTime); // ближайшее время старта до или если нет то null
				};			 								
		}

		/// <summary>
		/// Возвращает предикат для фильтрации по interval
		/// </summary>
		/// <param name="interval"></param>
		/// <returns></returns>
		private Func<DateTime, bool> GetIntervalPredicate(int interval)
		{
			switch (interval)
			{
				case 1:
					return d => d.DayOfWeek == DayOfWeek.Sunday;
				case 2:
					return d => d.DayOfWeek == DayOfWeek.Monday;
				case 3:
					return d => d.DayOfWeek == DayOfWeek.Tuesday;
				case 4:
					return d => d.DayOfWeek == DayOfWeek.Wednesday;
				case 5:
					return d => d.DayOfWeek == DayOfWeek.Thursday;
				case 6:
					return d => d.DayOfWeek == DayOfWeek.Friday;
				case 7:
					return d => d.DayOfWeek == DayOfWeek.Saturday;
				case 8:
					return d => true;
				case 9:
					return d => d.IsWeekday();
				case 10:
					return d => d.IsWeekend();
				default:
					return d => false;
			}			
		}

		/// <summary>
		/// Фильтрует по условия определяемому relativeInterval
		/// </summary>
		/// <param name="enumerator"></param>
		/// <param name="relativeInterval"></param>
		/// <returns></returns>
		private IEnumerable<DateTime> ApplyRelativeInternalConditions(IEnumerable<DateTime> enumerator, int relativeInterval)
		{
			switch (relativeInterval)
			{
				case 1:
					return enumerator.DayByMonth(1);
				case 2:
					return enumerator.DayByMonth(2);
				case 4:
					return enumerator.DayByMonth(3);
				case 8:
					return enumerator.DayByMonth(4);
				case 16:
					return enumerator.LastByMonth();
				default:
					return Enumerable.Empty<DateTime>();
			}
		}
		
	}
}
