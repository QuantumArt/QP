using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Quantumart.QP8.Utils
{
    public static class DateTimeExtensions
    {
        public static string ValueToDisplay(this DateTime dt)
        {
            return dt.ToString("G");
        }
    }
}
