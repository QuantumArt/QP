using System;
using System.Globalization;

namespace QP8.Services.Tests.Infrastructure.Helpers
{
    internal static class DateTimeHelpers
    {
        internal static DateTimeOffset ParseDateTime(string rawDateTime) => DateTime.Parse(rawDateTime, CultureInfo.InvariantCulture);

        internal static DateTimeOffset ParseDate(string rawDateTime) => ParseDateTime(rawDateTime).Date;

        internal static TimeSpan ParseTime(string rawDateTime) => ParseDateTime(rawDateTime).TimeOfDay;

        internal static Tuple<DateTimeOffset, DateTimeOffset> GetRangeTuple(string startRawDateTime, string endRawDateTime) => GetRangeTuple(ParseDateTime(startRawDateTime), ParseDateTime(endRawDateTime));

        internal static Tuple<DateTimeOffset, DateTimeOffset> GetRangeTuple(DateTimeOffset startDateTime, DateTimeOffset endDateTime) => new Tuple<DateTimeOffset, DateTimeOffset>(startDateTime, endDateTime);
    }
}
