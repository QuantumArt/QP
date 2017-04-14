using System;
using System.Globalization;

namespace QP8.Services.Tests.Infrastructure.Helpers
{
    internal static class DateTimeHelpers
    {
        internal static DateTime ParseDateTime(string rawDateTime)
        {
            return DateTime.Parse(rawDateTime, CultureInfo.InvariantCulture);
        }

        internal static DateTime ParseDate(string rawDateTime)
        {
            return ParseDateTime(rawDateTime).Date;
        }

        internal static TimeSpan ParseTime(string rawDateTime)
        {
            return ParseDateTime(rawDateTime).TimeOfDay;
        }

        internal static Tuple<DateTime, DateTime> GetRangeTuple(string startRawDateTime, string endRawDateTime)
        {
            return GetRangeTuple(ParseDateTime(startRawDateTime), ParseDateTime(endRawDateTime));
        }

        internal static Tuple<DateTime, DateTime> GetRangeTuple(DateTime startDateTime, DateTime endDateTime)
        {
            return new Tuple<DateTime, DateTime>(startDateTime, endDateTime);
        }
    }
}
