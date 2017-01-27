using System;

namespace Quantumart.QPublishing.Helpers
{
    internal static class CastDbNull
    {
        public static T To<T>(object value, T defaultValue)
        {
            return value != DBNull.Value ? (T)value : defaultValue;
        }

        public static T To<T>(object value)
        {
            return To(value, default(T));
        }
    }
}
