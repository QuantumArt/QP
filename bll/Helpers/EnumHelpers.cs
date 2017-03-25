using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using QP8.Infrastructure;

namespace Quantumart.QP8.BLL.Helpers
{
    public class EnumHelpers
    {
        /// <summary>
        /// Описание для Enum
        /// </summary>
        /// <param name="value">Значение множества</param>
        /// <returns>Текстовое описание описание</returns>
        public static string Description<TEnum>(TEnum value)
            where TEnum : struct, IConvertible
        {
            return Descriptions<TEnum>(value).First();
        }

        /// <summary>
        /// Возвращает описания для Enum
        /// </summary>
        /// <param name="value">Значение множества</param>
        /// <returns>Массив тесктовых описаний</returns>
        public static IEnumerable<string> Descriptions<TEnum>(TEnum value)
            where TEnum : struct, IConvertible
        {
            Ensure.IsEnum<TEnum>();
            var attributes = (DescriptionAttribute[])typeof(TEnum).GetField(value.ToString()).GetCustomAttributes(typeof(DescriptionAttribute), false);
            if (!attributes.Any())
            {
                return new[] { value.ToString() };
            }

            return attributes.Select(attr => attr.Description);
        }
    }
}
