using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;

namespace QP8.Infrastructure.Helpers
{
    public static class EnumHelpers
    {
        /// <summary>
        /// Описание для Enum
        /// </summary>
        /// <param name="value">Значение множества</param>
        /// <returns>Текстовое описание описание</returns>
        public static string Description<TEnum>(TEnum value)
            where TEnum : struct, IConvertible =>
            Descriptions(value).First();

        /// <summary>
        /// Возвращает описания для Enum
        /// </summary>
        /// <param name="value">Значение множества</param>
        /// <returns>Массив тесктовых описаний</returns>
        public static IEnumerable<string> Descriptions<TEnum>(TEnum value)
            where TEnum : struct, IConvertible
        {
            Ensure.IsEnum<TEnum>();
            var attributes = (DescriptionAttribute[])typeof(TEnum).GetField(value.ToString(CultureInfo.InvariantCulture)).GetCustomAttributes(typeof(DescriptionAttribute), false);
            return !attributes.Any() ? new[] { value.ToString(CultureInfo.InvariantCulture) } : attributes.Select(attr => attr.Description);
        }
    }
}
