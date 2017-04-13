using System;
using System.Globalization;

namespace QP8.Services.Tests.Infrastructure.Helpers
{
    internal static class DateTimeHelpers
    {
        internal static TimeSpan ParseTimeSpan(string rawTimeSpan)
        {
            return TimeSpan.Parse(rawTimeSpan, CultureInfo.InvariantCulture);
        }

        internal static DateTime ParseDateTime(string rawDateTime)
        {
            return DateTime.Parse(rawDateTime, CultureInfo.InvariantCulture);
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
