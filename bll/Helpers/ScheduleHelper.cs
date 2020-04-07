using System;
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
                var tomorrow = DateTime.Now.AddDays(1);
                return new DateTime(tomorrow.Year, tomorrow.Month, tomorrow.Day, 0, 0, 0);
            }
        }

        public static DateTime DefaultEndDate
        {
            get
            {
                var tomorrow = DateTime.Now.AddDays(1);
                return new DateTime(tomorrow.Year, tomorrow.Month, tomorrow.Day, 23, 59, 59);
            }
        }

        private static DateTime GetScheduleDateTimeFromSqlValues(int? dateValue, int? timeValue, DateTime defaultDate) => dateValue != null && timeValue != null ? GetScheduleDateTimeFromSqlValues((int)dateValue, (int)timeValue) : defaultDate;

        public static DateTime GetScheduleDateFromSqlValues(int? dateValue, DateTime defaultDate) => dateValue.HasValue ? GetScheduleDateFromSqlValues(dateValue.Value) : defaultDate;

        public static TimeSpan GetScheduleTimeFromSqlValues(int? timeValue, TimeSpan defaultTime) => timeValue.HasValue ? GetScheduleTimeFromSqlValues(timeValue.Value) : defaultTime;

        internal static DateTime GetScheduleDateTimeFromSqlValues(int dateValue, int timeValue) => GetScheduleDateFromSqlValues(dateValue) + GetScheduleTimeFromSqlValues(timeValue);

        internal static DateTime GetScheduleDateFromSqlValues(int dateValue)
        {
            var year = dateValue / 10000;
            ;
            var month = dateValue % 10000 / 100;
            var day = dateValue % 100;
            return new DateTime(year, month, day);
        }

        internal static TimeSpan GetScheduleTimeFromSqlValues(int timeValue)
        {
            var hour = timeValue / 10000;
            var minute = timeValue % 10000 / 100;
            var second = timeValue % 100;
            return new TimeSpan(hour, minute, second);
        }

        internal static Tuple<int, int> GetSqlValuesFromScheduleDateTime(DateTime date) => new Tuple<int, int>(GetSqlValuesFromScheduleDate(date.Date), GetSqlValuesFromScheduleTime(date.TimeOfDay));

        internal static int GetSqlValuesFromScheduleDate(DateTime date) => date.Year * 10000 + date.Month * 100 + date.Day;

        internal static int GetSqlValuesFromScheduleTime(TimeSpan time) => time.Hours * 10000 + time.Minutes * 100 + time.Seconds;

        public static TimeSpan GetDuration(string durationUnits, decimal duration, DateTime startDate)
        {
            switch (durationUnits.ToLower())
            {
                case "mi":
                    return TimeSpan.FromMinutes(decimal.ToDouble(duration));
                case "hh":
                    return TimeSpan.FromHours(decimal.ToDouble(duration));
                case "dd":
                    return TimeSpan.FromDays(decimal.ToDouble(duration));
                case "wk":
                    return TimeSpan.FromTicks(TimeSpan.TicksPerDay * 7 * decimal.ToInt64(duration));
                case "mm":
                    return startDate.Date.AddMonths(decimal.ToInt32(duration)) - startDate.Date;
                case "yy":
                    return startDate.Date.AddYears(decimal.ToInt32(duration)) - startDate.Date;
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
