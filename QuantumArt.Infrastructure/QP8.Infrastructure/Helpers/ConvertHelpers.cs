using System;

namespace QP8.Infrastructure.Helpers
{
    public class ConvertHelpers
    {
        public static T ChangeType<T>(object value)
        {
            var type = typeof(T);
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                if (value == null)
                {
                    return default(T);
                }

                type = Nullable.GetUnderlyingType(type);
            }

            return value == DBNull.Value
                ? default(T)
                : (T)Convert.ChangeType(value, type);
        }

        public static object ChangeType(object value, Type conversion)
        {
            var type = conversion;
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                if (value == null || value == DBNull.Value)
                {
                    return null;
                }

                type = Nullable.GetUnderlyingType(type);
            }

            return value == DBNull.Value
                ? null
                : Convert.ChangeType(value, type);
        }
    }
}
