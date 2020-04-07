using System;
using System.Globalization;

namespace QP8.Services.Tests.Infrastructure.Helpers
{
    internal static class DateTimeHelpers
    {
        internal static DateTime ParseDateTime(string rawDateTime) => DateTime.Parse(rawDateTime, CultureInfo.InvariantCulture);

        internal static DateTime ParseDate(string rawDateTime) => ParseDateTime(rawDateTime).Date;

        internal static TimeSpan ParseTime(string rawDateTime) => ParseDateTime(rawDateTime).TimeOfDay;

        internal static Tuple<DateTime, DateTime> GetRangeTuple(string startRawDateTime, string endRawDateTime) => GetRangeTuple(ParseDateTime(startRawDateTime), ParseDateTime(endRawDateTime));

        internal static Tuple<DateTime, DateTime> GetRangeTuple(DateTime startDateTime, DateTime endDateTime) => new Tuple<DateTime, DateTime>(startDateTime, endDateTime);
    }
}
