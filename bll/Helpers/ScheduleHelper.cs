using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Quantumart.QP8.Constants;

namespace Quantumart.QP8.BLL.Helpers
{
    public static class ScheduleHelper
    {
        
        public static DateTime GetStartDateTime(int? dateValue, int? timeValue)
        {
            return GetScheduleDateTimeFromSqlValues(dateValue, timeValue, DefaultStartDate);
        }

        public static DateTime GetEndDateTime(int? dateValue, int? timeValue)
        {
            return GetScheduleDateTimeFromSqlValues(dateValue, timeValue, DefaultEndDate);
        }


        public static DateTime DefaultStartDate
        {
            get
            {
                DateTime tomorrow = DateTime.Now.AddDays(1);
                return new DateTime(tomorrow.Year, tomorrow.Month, tomorrow.Day, 0, 0, 0);
            }
        }

        public static DateTime DefaultEndDate
        {
            get
            {

                DateTime tomorrow = DateTime.Now.AddDays(1);
                return new DateTime(tomorrow.Year, tomorrow.Month, tomorrow.Day, 23, 59, 59);
            }
        }

        private static DateTime GetScheduleDateTimeFromSqlValues(int? dateValue, int? timeValue, DateTime defaultDate)
        {
            return (dateValue != null && timeValue != null) ? GetScheduleDateTimeFromSqlValues((int)dateValue, (int)timeValue) : defaultDate;
        }

		public static DateTime GetScheduleDateFromSqlValues(int? dateValue, DateTime defaultDate)
		{
			return dateValue.HasValue ? GetScheduleDateFromSqlValues(dateValue.Value) : defaultDate;
		}

		public static TimeSpan GetScheduleTimeFromSqlValues(int? timeValue, TimeSpan defaultTime)
		{
			return timeValue.HasValue ? GetScheduleTimeFromSqlValues(timeValue.Value) : defaultTime;
		}


        internal static DateTime GetScheduleDateTimeFromSqlValues(int dateValue, int timeValue)
        {			
			return GetScheduleDateFromSqlValues(dateValue) + GetScheduleTimeFromSqlValues(timeValue);
        }

		internal static DateTime GetScheduleDateFromSqlValues(int dateValue)
		{
			int year = dateValue / 10000; ;
			int month = (dateValue % 10000) / 100;
			int day = dateValue % 100;			
			return new DateTime(year, month, day);
		}

		internal static TimeSpan GetScheduleTimeFromSqlValues(int timeValue)
		{			
			int hour = timeValue / 10000;
			int minute = (timeValue % 10000) / 100;
			int second = timeValue % 100;
			return new TimeSpan(hour, minute, second);
		}


        internal static Tuple<int, int> GetSqlValuesFromScheduleDateTime(DateTime date)
        {
			return new Tuple<int, int>(GetSqlValuesFromScheduleDate(date.Date), GetSqlValuesFromScheduleTime(date.TimeOfDay));
        }

		internal static int GetSqlValuesFromScheduleDate(DateTime date)
		{
			return date.Year * 10000 + date.Month * 100 + date.Day;
		}

		internal static int GetSqlValuesFromScheduleTime(TimeSpan time)
		{
			return time.Hours * 10000 + time.Minutes * 100 + time.Seconds;
		}

              

		public static TimeSpan GetDuration(string durationUnits, Decimal duration, DateTime startDate)
		{
			switch (durationUnits.ToLower())
			{
				case "mi":
					return TimeSpan.FromMinutes(Decimal.ToDouble(duration));
				case "hh":
					return TimeSpan.FromHours(Decimal.ToDouble(duration));
				case "dd":
					return TimeSpan.FromDays(Decimal.ToDouble(duration));
				case "wk":
					return TimeSpan.FromTicks(TimeSpan.TicksPerDay * 7 * Decimal.ToInt64(duration));
				case "mm":
					return startDate.Date.AddMonths(Decimal.ToInt32(duration)) - startDate.Date;
				case "yy":
					return startDate.Date.AddYears(Decimal.ToInt32(duration)) - startDate.Date;
				default:
					throw new ArgumentException("Unknown duration units: " + durationUnits);
			}
		}

		public static ShowDurationUnit ParseDurationUnit(string durationUnits)
		{
			switch (durationUnits.ToLower())
			{
				case "mi":
					return ShowDurationUnit.Minutes;
				case "hh":
					return ShowDurationUnit.Hours;
				case "dd":
					return ShowDurationUnit.Days;
				case "wk":
					return ShowDurationUnit.Weeks;
				case "mm":
					return ShowDurationUnit.Months;
				case "yy":
					return ShowDurationUnit.Years;
				default:
					throw new ArgumentException("Unknown duration units: " + durationUnits);
			}
		}

		public static string ToDbValue(this ShowDurationUnit unit)
		{
			switch (unit)
			{
				case ShowDurationUnit.Minutes:
					return "mi";
				case ShowDurationUnit.Hours:
					return "hh";
				case ShowDurationUnit.Days:
					return "dd";
				case ShowDurationUnit.Weeks:
					return "wk";
				case ShowDurationUnit.Months:
					return "mm";
				case ShowDurationUnit.Years:
					return "yy";
				default:
					throw new ArgumentException("Unknown duration units: " + unit);
			}
		}
    }
}
