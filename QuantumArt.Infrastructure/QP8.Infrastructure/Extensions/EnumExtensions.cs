using System;
using QP8.Infrastructure.Helpers;

namespace QP8.Infrastructure.Extensions
{
    public static class EnumExtensions
    {
        public static string Description<TEnum>(this TEnum value)
            where TEnum : struct, IConvertible =>
            EnumHelpers.Description(value);
    }
}
