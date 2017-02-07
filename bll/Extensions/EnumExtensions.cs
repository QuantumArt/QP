using System;
using Quantumart.QP8.BLL.Helpers;

namespace Quantumart.QP8.BLL.Extensions
{
    public static class EnumExtensions
    {
        public static string Description<TEnum>(this TEnum value)
            where TEnum : struct, IConvertible
        {
            return EnumHelpers.Description(value);
        }
    }
}
