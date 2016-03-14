using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Quantumart.QP8.ArticleScheduler
{
	public static class DateRangeExtensions
	{
		/// <summary>
		/// Возвращает полные месяца, но только те, которые ограничены recurrenceFactor
		/// </summary>
		/// <param name="range"></param>
		/// <param name="recurrenceFactor"></param>
		/// <returns></returns>
		public static IEnumerable<Tuple<DateTime, DateTime>> EveryMonths(this Tuple<DateTime, DateTime> range, int recurrenceFactor)
		{
			var startRangeFirstDay = range.Item1.GetMounth();
			var endRangeFirstDay = range.Item2.GetMounth();

			int countFactor = recurrenceFactor;
			DateTime currentDate = startRangeFirstDay;

			while (currentDate < endRangeFirstDay.AddMonths(1))
			{				
				if (countFactor % recurrenceFactor == 0)
					yield return Tuple.Create(currentDate, currentDate.AddMonths(1).AddDays(-1));
				currentDate = currentDate.AddMonths(1);
				countFactor++;
			}			
		}

		/// <summary>
		/// Возвращает полные неели, но только те, которые ограничены recurrenceFactor
		/// </summary>
		/// <param name="range"></param>
		/// <param name="recurrenceFactor"></param>
		/// <returns></returns>
		public static IEnumerable<Tuple<DateTime, DateTime>> EveryWeeks(this Tuple<DateTime, DateTime> range, int recurrenceFactor)
		{
			var startRangeFirstDay = range.Item1.GetWeek();
			var endRangeFirstDay = range.Item2.GetWeek();

			int countFactor = recurrenceFactor;
			DateTime currentDate = startRangeFirstDay;

			while (currentDate < endRangeFirstDay.AddDays(7))
			{
				if (countFactor % recurrenceFactor == 0)
					yield return Tuple.Create(currentDate, currentDate.AddDays(6));
				currentDate = currentDate.AddDays(7);
				countFactor++;
			}
		}

		/// <summary>
		/// Возвращает каждый n-й день 
		/// </summary>
		/// <param name="range"></param>
		/// <param name="recurrenceFactor"></param>
		/// <returns></returns>
		public static IEnumerable<DateTime> EveryDays(this Tuple<DateTime, DateTime> range, int n)
		{
			var startRangeFirstDay = range.Item1.Date;
			var endRangeFirstDay = range.Item2.Date;

			int countFactor = n;
			DateTime currentDate = startRangeFirstDay;

			while (currentDate < endRangeFirstDay.AddDays(1))
			{
				if (countFactor % n == 0)					
					yield return currentDate;
				currentDate = currentDate.AddDays(1);
				countFactor++;
			}
		}

		/// <summary>
		/// Получить все дни диапазона
		/// </summary>
		/// <param name="range"></param>
		/// <returns></returns>
		public static IEnumerable<DateTime> Days(this IEnumerable<Tuple<DateTime, DateTime>> ranges)
		{
			foreach (var range in ranges)
			{
				var currentDate = range.Item1.Date;
				var endDate = range.Item2.Date;
				while (currentDate <= endDate)
				{
					yield return currentDate;
					currentDate = currentDate.AddDays(1);
				}
			}
		}
	
		/// <summary>
		/// Получить n-й дни с групировкой по месяцам
		/// </summary>
		/// <param name="days"></param>
		/// <param name="number"></param>
		/// <returns></returns>
		public static IEnumerable<DateTime> DayByMonth(this IEnumerable<DateTime> days, int number)
		{
			if (number < 1 || number > 31)
				throw new ArgumentOutOfRangeException("number = " + number);
			return (from day in days
					group day by day.GetMounth() into g
					select g.OrderBy(d => d).Skip(number - 1).FirstOrDefault())
							.Where(d => !d.Equals(default(DateTime)));
		}

		/// <summary>
		/// Получить последние дни с групировкой по месяцам
		/// </summary>
		/// <param name="days"></param>
		/// <param name="number"></param>
		/// <returns></returns>
		public static IEnumerable<DateTime> LastByMonth(this IEnumerable<DateTime> days)
		{			
			return (from day in days
					group day by day.GetMounth() into g
					select g.OrderBy(d => d).LastOrDefault())
							.Where(d => !d.Equals(default(DateTime)));
		}

		/// <summary>
		/// Получить ближайшую дату для указанной по след. алгоритму:
		/// взять ближайшую предшествующую, если ее нет то вернуть null
		/// </summary>
		/// <param name="days"></param>
		/// <param name="dateTime"></param>
		/// <returns></returns>
		public static DateTime? Nearest(this IEnumerable<DateTime> dates, DateTime dateTime)
		{
			// Получить ближайшую предыдущую
			var previuoses = dates.Where(d => d <= dateTime);
			if (previuoses.Any())
				return previuoses.Max();
			else
				return null;
		}

		/// <summary>
		/// Рабочий ли день
		/// </summary>
		/// <param name="dt"></param>
		/// <returns></returns>
		public static bool IsWeekday(this DateTime dt)
		{
			return dt.DayOfWeek == DayOfWeek.Friday ||
				dt.DayOfWeek == DayOfWeek.Monday ||
				dt.DayOfWeek == DayOfWeek.Thursday ||
				dt.DayOfWeek == DayOfWeek.Tuesday ||
				dt.DayOfWeek == DayOfWeek.Wednesday;
		}

		/// <summary>
		/// Выходной ли день
		/// </summary>
		/// <param name="dt"></param>
		/// <returns></returns>
		public static bool IsWeekend(this DateTime dt)
		{
			return dt.DayOfWeek == DayOfWeek.Saturday || dt.DayOfWeek == DayOfWeek.Sunday;
		}

		/// <summary>
		/// Возвращает дату начала месяца
		/// </summary>
		/// <param name="dt"></param>
		/// <returns></returns>
		public static DateTime GetMounth(this DateTime dt)
		{
			return new DateTime(dt.Year, dt.Month, 1);
		}

		/// <summary>
		/// Возвращает дату начала недели
		/// </summary>
		/// <param name="dt"></param>
		/// <returns></returns>
		public static DateTime GetWeek(this DateTime dt)
		{
			var iteratorDate = dt;
			while (iteratorDate.DayOfWeek != DayOfWeek.Monday)
			{
				iteratorDate = iteratorDate.AddDays(-1);
			}
			return iteratorDate.Date;
		}

		/// <summary>
		/// Определяет положение даты относительно диапазона
		/// </summary>
		/// <param name="range"></param>
		/// <param name="dt"></param>
		/// <returns>
		/// -1 - дата до диапазона
		/// 0 - дата в диапазоне
		/// 1 - дата после диапазона
		/// </returns>
		public static int Position(this Tuple<DateTime, DateTime> range, DateTime dt)
		{
			if (range.Item1 > dt)
				return -1;
			if (range.Item2 < dt)
				return 1;
			return 0;
		}
	}
}
