using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Quantumart.QP8.ArticleScheduler.Recurring
{
	/// <summary>
	/// Вычисляет ближайшее к дате начало диапазона для Weekly расписаний 
	/// </summary>
	public class DailyStartCalc : RecuringStartCalcBase
	{		
		public DailyStartCalc(int interval,
			DateTime startDate, DateTime endDate, TimeSpan startTime)
		{
			calc = (dateTime) =>
			{
				return Optimize(new Tuple<DateTime, DateTime>(startDate.Date, endDate.Date), dateTime.Date)
				.EveryDays(interval) // получаем каждый n-й день
				.Select(d => d.Add(startTime)) // получаем точное время старта				 								
				.Nearest(dateTime);
			};
		}						
	}
}
