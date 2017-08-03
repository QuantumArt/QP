using System;

namespace Quantumart.QP8.Utils
{
    public static class DateTimeExtensions
    {
        public static string ValueToDisplay(this DateTime dt) => dt.ToString("G");
    }
}
