using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlTypes;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using Quantumart.QP8.Utils.Constants;

namespace Quantumart.QP8.Utils
{
    public static class Converter
    {
        public static bool CanParse<T>(string s)
        {
            if (typeof(T) == typeof(DateTime) || typeof(T) == typeof(TimeSpan))
            {
                DateTime dt;
                return DateTime.TryParse(s, CultureInfo.CurrentCulture, DateTimeStyles.None, out dt);
            }

            if (typeof(T) == typeof(int))
            {
                int i;
                return int.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out i);
            }

            throw new NotImplementedException(typeof(T).Name + " is not supported.");
        }

        /// <summary>
        /// Проверяет является ли значение пустым
        /// </summary>
        /// <param name="value">значение</param>
        /// <returns>результат проверки (true - пустое; false - непустое)</returns>
        public static bool IsNullOrEmpty(object value)
        {
            return !(value?.ToString().Length > 0);
        }

        /// <summary>
        /// Преобразует пустую строку в null
        /// </summary>
        /// <param name="value">значение</param>
        /// <returns>преобразованное значение</returns>
        public static string ToNull(string value)
        {
            if (value != null)
            {
                var processedValue = value.Trim().ToLower();
                if (processedValue.Length == 0 || processedValue == "null")
                {
                    return null;
                }

                return value;
            }

            return null;
        }

        /// <summary>
        /// Преобразует объект в строку
        /// </summary>
        /// <param name="value">значение</param>
        /// <returns>преобразованное значение</returns>
        public static string ToString(object value)
        {
            return ToString(value, "");
        }

        /// <summary>
        /// Преобразует объект в строку
        /// </summary>
        /// <param name="value">значение</param>
        /// <param name="defaultValue">значение по умолчанию</param>
        /// <returns>преобразованное значение</returns>
        public static string ToString(object value, string defaultValue)
        {
            var result = defaultValue;

            if (value != null)
            {
                result = value.ToString();
            }

            return result;
        }

        /// <summary>
        /// Преобразует объект в значение типа Int32
        /// </summary>
        /// <param name="value">значение</param>
        /// <returns>преобразованное значение</returns>
        public static int ToInt32(object value)
        {
            return ToInt32(value, 0);
        }

        /// <summary>
        /// Преобразует объект в значение типа Int32
        /// </summary>
        /// <param name="value">значение</param>
        /// <param name="defaultValue">значение по умолчанию</param>
        /// <returns>преобразованное значение</returns>
        public static int ToInt32(object value, int defaultValue)
        {
            var result = defaultValue;
            var rawResult = ToNullableInt32(value);

            if (rawResult != null)
            {
                result = (int)rawResult;
            }

            return result;
        }

        /// <summary>
        /// Преобразует объект в значение типа Int64
        /// </summary>
        /// <param name="value">значение</param>
        /// <returns>преобразованное значение</returns>
        public static long ToInt64(object value)
        {
            return ToInt64(value, 0);
        }

        /// <summary>
        /// Преобразует объект в значение типа Int64
        /// </summary>
        /// <param name="value">значение</param>
        /// <param name="defaultValue">значение по умолчанию</param>
        /// <returns>преобразованное значение</returns>
        public static long ToInt64(object value, long defaultValue)
        {
            var result = defaultValue;
            var rawResult = ToNullableInt64(value);

            if (rawResult != null)
            {
                result = (long)rawResult;
            }

            return result;
        }

        /// <summary>
		/// Преобразует объект в значение типа Decimal
        /// </summary>
		/// <param name="value">значение</param>
		/// <returns>преобразованное значение</returns>
        public static decimal ToDecimal(object value)
        {
            return ToDecimal(value, 0);
        }

        /// <summary>
        /// Преобразует объект в значение типа Decimal
        /// </summary>
        /// <param name="value">значение</param>
        /// <param name="defaultValue">значение по умолчанию</param>
        /// <returns>преобразованное значение</returns>
        public static decimal ToDecimal(object value, decimal defaultValue)
        {
            var result = defaultValue;
            var rawResult = ToNullableDecimal(value);

            if (rawResult != null)
            {
                result = (decimal)rawResult;
            }

            return result;
        }

        /// <summary>
        /// Преобразует объект в значение типа Double
        /// </summary>
        /// <param name="value">значение</param>
        /// <returns>преобразованное значение</returns>
        public static double ToDouble(object value)
        {
            return ToDouble(value, 0);
        }

        /// <summary>
        /// Преобразует объект в значение типа Double
        /// </summary>
        /// <param name="value">значение</param>
        /// <param name="defaultValue">значение по умолчанию</param>
        /// <returns>преобразованное значение</returns>
        public static double ToDouble(object value, double defaultValue)
        {
            var result = defaultValue;

            if (!IsNullOrEmpty(value))
            {
                var processedValue = value.ToString().Trim();
                if (string.Equals(processedValue, "true", StringComparison.InvariantCultureIgnoreCase))
                {
                    return 1;
                }

                if (string.Equals(processedValue, "false", StringComparison.InvariantCultureIgnoreCase))
                {
                    return 0;
                }

                try
                {
                    result = double.Parse(processedValue);
                }
                catch
                {
                    result = defaultValue;
                }
            }

            return result;
        }

        /// <summary>
        /// Преобразует объект в значение типа Float
        /// </summary>
        /// <param name="value">значение</param>
        /// <returns>преобразованное значение</returns>
        public static float ToFloat(object value)
        {
            return ToFloat(value, 0);
        }

        /// <summary>
        /// Преобразует объект в значение типа Float
        /// </summary>
        /// <param name="value">значение</param>
        /// <param name="defaultValue">значение по умолчанию</param>
        /// <returns>преобразованное значение</returns>
        public static float ToFloat(object value, float defaultValue)
        {
            var result = defaultValue;

            if (!IsNullOrEmpty(value))
            {
                var processedValue = value.ToString().Trim();
                if (string.Equals(processedValue, "true", StringComparison.InvariantCultureIgnoreCase))
                {
                    return 1;
                }

                if (string.Equals(processedValue, "false", StringComparison.InvariantCultureIgnoreCase))
                {
                    return 0;
                }

                try
                {
                    result = float.Parse(processedValue);
                }
                catch
                {
                    result = defaultValue;
                }
            }

            return result;
        }

        /// <summary>
		/// Преобразует объект в значение типа Boolean
        /// </summary>
		/// <param name="value">значение</param>
		/// <returns>преобразованное значение</returns>
        public static bool ToBoolean(object value)
        {
            return ToBoolean(value, false);
        }

        /// <summary>
        /// Преобразует объект в значение типа Boolean
        /// </summary>
        /// <param name="value">значение</param>
        /// <param name="defaultValue">значение по умолчанию</param>
        /// <returns>преобразованное значение</returns>
        public static bool ToBoolean(object value, bool defaultValue)
        {
            var result = defaultValue;
            var rawResult = ToNullableBoolean(value);

            if (rawResult != null)
            {
                result = (bool)rawResult;
            }

            return result;
        }

        /// <summary>
		/// Преобразует объект в объект типа DateTime
        /// </summary>
		/// <param name="value">значение</param>
		/// <returns>преобразованное значение</returns>
        public static DateTime ToDateTime(object value)
        {
            return ToDateTime(value, DateTime.MinValue);
        }

        /// <summary>
        /// Преобразует объект в объект типа DateTime
        /// </summary>
        /// <param name="value">значение</param>
        /// <param name="defaultValue">значение по умолчанию</param>
        /// <returns>преобразованное значение</returns>
        public static DateTime ToDateTime(object value, DateTime defaultValue)
        {
            var result = defaultValue;
            var rawResult = ToNullableDateTime(value);

            if (rawResult != null)
            {
                result = (DateTime)rawResult;
            }

            return result;
        }

        /// <summary>
		/// Преобразует объект типа DateTime в дату без времени
        /// </summary>
		/// <param name="value">значение</param>
		/// <returns>преобразованное значение</returns>
        public static DateTime ToDate(object value)
        {
            return ToDate(value, DateTime.MinValue);
        }

        /// <summary>
        /// Преобразует объект типа DateTime в дату без времени
        /// </summary>
        /// <param name="value">значение</param>
        /// <param name="defaultValue">значение по умолчанию</param>
        /// <returns>преобразованное значение</returns>
        public static DateTime ToDate(object value, DateTime defaultValue)
        {
            var result = ToDateTime(value, defaultValue);
            if (result != DateTime.MinValue)
            {
                result = ToDate(result);
            }

            return result;
        }

        /// <summary>
        /// Преобразует объект типа DateTime в дату без времени
        /// </summary>
        /// <param name="value">значение</param>
        /// <returns>преобразованное значение</returns>
        public static DateTime ToDate(DateTime value)
        {
            return new DateTime(value.Year, value.Month, value.Day);
        }

        /// <summary>
        /// Преобразует массив значений типа Int32 в строковый массив
        /// </summary>
        /// <param name="value">массив значений типа Int32</param>
        /// <returns>строковый массив</returns>
        public static string[] ToStringCollection(int[] value)
        {
            var result = new string[value.Length];

            for (var i = 0; i < value.Length; i++)
            {
                result[i] = value[i].ToString();
            }

            return result;
        }

        /// <summary>
        /// Преобразует строку в строковый массив
        /// </summary>
        /// <param name="value">строковое значение</param>
        /// <param name="separator">разделитель</param>
        /// <returns>строковый массив</returns>
        public static string[] ToStringCollection(string value, char separator)
        {
            return ToStringCollection(value, separator, false);
        }

        /// <summary>
        /// Преобразует строку в строковый массив
        /// </summary>
        /// <param name="value">строковое значение</param>
        /// <param name="separator">разделитель</param>
        /// <param name="removeEmptyEntries">признак, разрешающий удалять пустые значения</param>
        /// <returns>строковый массив</returns>
        public static string[] ToStringCollection(string value, char separator, bool removeEmptyEntries)
        {
            return ToStringCollection(value, separator, removeEmptyEntries, "");
        }

        /// <summary>
        /// Преобразует строку в строковый массив
        /// </summary>
        /// <param name="value">строковое значение</param>
        /// <param name="separator">разделитель</param>
        /// <param name="removeEmptyEntries">признак, разрешающий удалять пустые значения</param>
        /// <param name="defaultValue">значение по умолчанию</param>
        /// <returns>строковый массив</returns>
        public static string[] ToStringCollection(string value, char separator, bool removeEmptyEntries, string defaultValue)
        {
            var result = new string[0];
            var rawString = ToString(value).Trim();

            if (rawString.Length > 0)
            {
                var rawCollection = rawString.Split(separator); // коллекция необработанных элементов
                var rawCount = rawCollection.Length; // количество элементов

                if (rawCount > 0)
                {
                    var processedCollection = new string[rawCount]; // коллекция обработанных элементов
                    var nonEmptyElementsCount = 0; // количество не пустых элементов

                    for (var rawIndex = 0; rawIndex < rawCount; rawIndex++)
                    {
                        processedCollection[rawIndex] = rawCollection[rawIndex].Trim();
                        if (processedCollection[rawIndex].Length > 0)
                        {
                            nonEmptyElementsCount++;
                        }
                    }

                    if (removeEmptyEntries)
                    {
                        result = new string[nonEmptyElementsCount];

                        for (var processedIndex = 0; processedIndex < nonEmptyElementsCount; processedIndex++)
                        {
                            if (processedCollection[processedIndex].Length > 0)
                            {
                                result[processedIndex] = processedCollection[processedIndex];
                            }
                        }
                    }
                    else
                    {
                        result = new string[rawCount];

                        for (var processedIndex = 0; processedIndex < rawCount; processedIndex++)
                        {
                            if (processedCollection[processedIndex].Length > 0)
                            {
                                result[processedIndex] = processedCollection[processedIndex];
                            }
                            else
                            {
                                result[processedIndex] = defaultValue;
                            }
                        }
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Преобразует строковый массив в массив значений типа Int32
        /// </summary>
        /// <param name="value">строковый массив</param>
        /// <returns>массив значений типа Int32</returns>
        public static int[] ToInt32Collection(string[] value)
        {
            var result = new int[value.Length];

            if (value.Length > 0)
            {
                for (var i = 0; i < value.Length; i++)
                {
                    result[i] = ToInt32(value[i]);
                }
            }

            return result;
        }

        /// <summary>
        /// Преобразует строку в массив значений типа Int32
        /// </summary>
        /// <param name="value">строковый массив</param>
        /// <param name="separator">разделитель</param>
        /// <returns>массив значений типа Int32</returns>
        public static int[] ToInt32Collection(string value, char separator)
        {
            return ToInt32Collection(value, separator, false);
        }

        /// <summary>
        /// Преобразует строку в массив значений типа Int32
        /// </summary>
        /// <param name="value">строковый массив</param>
        /// <param name="separator">разделитель</param>
        /// <param name="removeEmptyEntries">признак, разрешающий удалять пустые значения</param>
        /// <returns>массив значений типа Int32</returns>
        public static int[] ToInt32Collection(string value, char separator, bool removeEmptyEntries)
        {
            return ToInt32Collection(value, separator, removeEmptyEntries, 0);
        }

        /// <summary>
        /// Преобразует строку в массив значений типа Int32
        /// </summary>
        /// <param name="value">строковый массив</param>
        /// <param name="separator">разделитель</param>
        /// <param name="removeEmptyEntries">признак, разрешающий удалять пустые значения</param>
        /// <param name="defaultValue">значение по умолчанию</param>
        /// <returns>массив значений типа Int32</returns>
        public static int[] ToInt32Collection(string value, char separator, bool removeEmptyEntries, int defaultValue)
        {
            var result = new int[0];

            var processedCollection = ToStringCollection(value, separator, removeEmptyEntries);
            var processedCount = processedCollection.Length;

            if (processedCount > 0)
            {
                result = new int[processedCount];

                for (var processedIndex = 0; processedIndex < processedCount; processedIndex++)
                {
                    result[processedIndex] = ToInt32(processedCollection[processedIndex], defaultValue);
                }
            }

            return result;
        }

        /// <summary>
        /// Преобразует массив значений типа Decimal в массив типа Int32
        /// </summary>
        /// <param name="values">массив значений типа Int32</param>
        /// <returns>массив значений типа Decimal</returns>
        public static IEnumerable<int> ToInt32Collection(IEnumerable<decimal> values)
        {
            return values.Select(v => ToInt32(v)).ToArray();
        }

        /// <summary>
        /// Преобразует строку в массив значений типа Int64
        /// </summary>
        /// <param name="value">строковый массив</param>
        /// <param name="separator">разделитель</param>
        /// <returns>массив значений типа Int64</returns>
        public static long[] ToInt64Collection(string value, char separator)
        {
            return ToInt64Collection(value, separator, false);
        }

        /// <summary>
        /// Преобразует строку в массив значений типа Int64
        /// </summary>
        /// <param name="value">строковый массив</param>
        /// <param name="separator">разделитель</param>
        /// <param name="removeEmptyEntries">признак, разрешающий удалять пустые значения</param>
        /// <returns>массив значений типа Int64</returns>
        public static long[] ToInt64Collection(string value, char separator, bool removeEmptyEntries)
        {
            return ToInt64Collection(value, separator, removeEmptyEntries, 0);
        }

        /// <summary>
        /// Преобразует строку в массив значений типа Int64
        /// </summary>
        /// <param name="value">строковый массив</param>
        /// <param name="separator">разделитель</param>
        /// <param name="removeEmptyEntries">признак, разрешающий удалять пустые значения</param>
        /// <param name="defaultValue">значение по умолчанию</param>
        /// <returns>массив значений типа Int64</returns>
        public static long[] ToInt64Collection(string value, char separator, bool removeEmptyEntries, long defaultValue)
        {
            var result = new long[0];

            var processedCollection = ToStringCollection(value, separator, removeEmptyEntries);
            var processedCount = processedCollection.Length;

            if (processedCount > 0)
            {
                result = new long[processedCount];

                for (var processedIndex = 0; processedIndex < processedCount; processedIndex++)
                {
                    result[processedIndex] = ToInt64(processedCollection[processedIndex], defaultValue);
                }
            }

            return result;
        }

        /// <summary>
        /// Преобразует строку в массив значений типа Decimal
        /// </summary>
        /// <param name="value">строковый массив</param>
        /// <param name="separator">разделитель</param>
        /// <returns>массив значений типа Decimal</returns>
        public static decimal[] ToDecimalCollection(string value, char separator)
        {
            return ToDecimalCollection(value, separator, false);
        }

        /// <summary>
        /// Преобразует строку в массив значений типа Decimal
        /// </summary>
        /// <param name="value">строковый массив</param>
        /// <param name="separator">разделитель</param>
        /// <param name="removeEmptyEntries">признак, разрешающий удалять пустые значения</param>
        /// <returns>массив значений типа Decimal</returns>
        public static decimal[] ToDecimalCollection(string value, char separator, bool removeEmptyEntries)
        {
            return ToDecimalCollection(value, separator, removeEmptyEntries, 0);
        }

        /// <summary>
        /// Преобразует строку в массив значений типа Decimal
        /// </summary>
        /// <param name="value">строковый массив</param>
        /// <param name="separator">разделитель</param>
        /// <param name="removeEmptyEntries">признак, разрешающий удалять пустые значения</param>
        /// <param name="defaultValue">значение по умолчанию</param>
        /// <returns>массив значений типа Decimal</returns>
        public static decimal[] ToDecimalCollection(string value, char separator, bool removeEmptyEntries, decimal defaultValue)
        {
            var result = new decimal[0];

            var processedCollection = ToStringCollection(value, separator, removeEmptyEntries);
            var processedCount = processedCollection.Length;

            if (processedCount > 0)
            {
                result = new decimal[processedCount];

                for (var processedIndex = 0; processedIndex < processedCount; processedIndex++)
                {
                    result[processedIndex] = ToDecimal(processedCollection[processedIndex], defaultValue);
                }
            }

            return result;
        }

        /// <summary>
        /// Преобразует массив значений типа Int32 в массив типа Decimal
        /// </summary>
        /// <param name="values">массив значений типа Int32</param>
        /// <returns>массив значений типа Decimal</returns>
        public static IEnumerable<decimal> ToDecimalCollection(IEnumerable<int> values)
        {
            //List<decimal> result = new List<decimal>();

            //foreach (int value in values)
            //{
            //    result.Add((decimal)value);
            //}

            //return result.ToArray();
            return values.Select(v => ToDecimal(v)).ToArray();
        }

        /// <summary>
        /// Преобразует строку в массив значений типа Double
        /// </summary>
        /// <param name="value">строковый массив</param>
        /// <param name="separator">разделитель</param>
        /// <returns>массив значений типа Double</returns>
        public static double[] ToDoubleCollection(string value, char separator)
        {
            return ToDoubleCollection(value, separator, false);
        }

        /// <summary>
        /// Преобразует строку в массив значений типа Double
        /// </summary>
        /// <param name="value">строковый массив</param>
        /// <param name="separator">разделитель</param>
        /// <param name="removeEmptyEntries">признак, разрешающий удалять пустые значения</param>
        /// <returns>массив значений типа Double</returns>
        public static double[] ToDoubleCollection(string value, char separator, bool removeEmptyEntries)
        {
            return ToDoubleCollection(value, separator, removeEmptyEntries, 0);
        }

        /// <summary>
        /// Преобразует строку в массив значений типа Double
        /// </summary>
        /// <param name="value">строковый массив</param>
        /// <param name="separator">разделитель</param>
        /// <param name="removeEmptyEntries">признак, разрешающий удалять пустые значения</param>
        /// <param name="defaultValue">значение по умолчанию</param>
        /// <returns>массив значений типа Double</returns>
        public static double[] ToDoubleCollection(string value, char separator, bool removeEmptyEntries, double defaultValue)
        {
            var result = new double[0];

            var processedCollection = ToStringCollection(value, separator, removeEmptyEntries);
            var processedCount = processedCollection.Length;

            if (processedCount > 0)
            {
                result = new double[processedCount];

                for (var processedIndex = 0; processedIndex < processedCount; processedIndex++)
                {
                    result[processedIndex] = ToDouble(processedCollection[processedIndex], defaultValue);
                }
            }

            return result;
        }

        /// <summary>
        /// Преобразует строку в массив значений типа Float
        /// </summary>
        /// <param name="value">строковый массив</param>
        /// <param name="separator">разделитель</param>
        /// <returns>массив значений типа Float</returns>
        public static float[] ToFloatCollection(string value, char separator)
        {
            return ToFloatCollection(value, separator, false);
        }

        /// <summary>
        /// Преобразует строку в массив значений типа Float
        /// </summary>
        /// <param name="value">строковый массив</param>
        /// <param name="separator">разделитель</param>
        /// <param name="removeEmptyEntries">признак, разрешающий удалять пустые значения</param>
        /// <returns>массив значений типа Float</returns>
        public static float[] ToFloatCollection(string value, char separator, bool removeEmptyEntries)
        {
            return ToFloatCollection(value, separator, removeEmptyEntries, 0);
        }

        /// <summary>
        /// Преобразует строку в массив значений типа Float
        /// </summary>
        /// <param name="value">строковый массив</param>
        /// <param name="separator">разделитель</param>
        /// <param name="removeEmptyEntries">признак, разрешающий удалять пустые значения</param>
        /// <param name="defaultValue">значение по умолчанию</param>
        /// <returns>массив значений типа Float</returns>
        public static float[] ToFloatCollection(string value, char separator, bool removeEmptyEntries, float defaultValue)
        {
            var result = new float[0];

            var processedCollection = ToStringCollection(value, separator, removeEmptyEntries);
            var processedCount = processedCollection.Length;

            if (processedCount > 0)
            {
                result = new float[processedCount];

                for (var processedIndex = 0; processedIndex < processedCount; processedIndex++)
                {
                    result[processedIndex] = ToFloat(processedCollection[processedIndex], defaultValue);
                }
            }

            return result;
        }

        /// <summary>
        /// Преобразует строку в массив значений типа Boolean
        /// </summary>
        /// <param name="value">строковый массив</param>
        /// <param name="separator">разделитель</param>
        /// <returns>массив значений типа Boolean</returns>
        public static bool[] ToBooleanCollection(string value, char separator)
        {
            return ToBooleanCollection(value, separator, false);
        }

        /// <summary>
        /// Преобразует строку в массив значений типа Boolean
        /// </summary>
        /// <param name="value">строковый массив</param>
        /// <param name="separator">разделитель</param>
        /// <param name="removeEmptyEntries">признак, разрешающий удалять пустые значения</param>
        /// <returns>массив значений типа Boolean</returns>
        public static bool[] ToBooleanCollection(string value, char separator, bool removeEmptyEntries)
        {
            return ToBooleanCollection(value, separator, removeEmptyEntries, false);
        }

        /// <summary>
        /// Преобразует строку в массив значений типа Boolean
        /// </summary>
        /// <param name="value">строковый массив</param>
        /// <param name="separator">разделитель</param>
        /// <param name="removeEmptyEntries">признак, разрешающий удалять пустые значения</param>
        /// <param name="defaultValue">значение по умолчанию</param>
        /// <returns>массив значений типа Boolean</returns>
        public static bool[] ToBooleanCollection(string value, char separator, bool removeEmptyEntries, bool defaultValue)
        {
            var result = new bool[0];

            var processedCollection = ToStringCollection(value, separator, removeEmptyEntries);
            var processedCount = processedCollection.Length;

            if (processedCount > 0)
            {
                result = new bool[processedCount];

                for (var processedIndex = 0; processedIndex < processedCount; processedIndex++)
                {
                    result[processedIndex] = ToBoolean(processedCollection[processedIndex], defaultValue);
                }
            }

            return result;
        }

        /// <summary>
        /// Преобразует строку в массив значений типа DateTime
        /// </summary>
        /// <param name="value">строковый массив</param>
        /// <param name="separator">разделитель</param>
        /// <returns>массив значений типа DateTime</returns>
        public static DateTime[] ToDateTimeCollection(string value, char separator)
        {
            return ToDateTimeCollection(value, separator, false);
        }

        /// <summary>
        /// Преобразует строку в массив значений типа DateTime
        /// </summary>
        /// <param name="value">строковый массив</param>
        /// <param name="separator">разделитель</param>
        /// <param name="removeEmptyEntries">признак, разрешающий удалять пустые значения</param>
        /// <returns>массив значений типа DateTime</returns>
        public static DateTime[] ToDateTimeCollection(string value, char separator, bool removeEmptyEntries)
        {
            return ToDateTimeCollection(value, separator, removeEmptyEntries, DateTime.MinValue);
        }

        /// <summary>
        /// Преобразует строку в массив значений типа DateTime
        /// </summary>
        /// <param name="value">строковый массив</param>
        /// <param name="separator">разделитель</param>
        /// <param name="removeEmptyEntries">признак, разрешающий удалять пустые значения</param>
        /// <param name="defaultValue">значение по умолчанию</param>
        /// <returns>массив значений типа DateTime</returns>
        public static DateTime[] ToDateTimeCollection(string value, char separator, bool removeEmptyEntries, DateTime defaultValue)
        {
            var result = new DateTime[0];

            var processedCollection = ToStringCollection(value, separator, removeEmptyEntries);
            var processedCount = processedCollection.Length;

            if (processedCount > 0)
            {
                result = new DateTime[processedCount];

                for (var processedIndex = 0; processedIndex < processedCount; processedIndex++)
                {
                    result[processedIndex] = ToDateTime(processedCollection[processedIndex], defaultValue);
                }
            }

            return result;
        }

        /// <summary>
        /// Преобразует строку в массив значений типа DateTime без времени
        /// </summary>
        /// <param name="value">строковый массив</param>
        /// <param name="separator">разделитель</param>
        /// <returns>массив значений типа DateTime без времени</returns>
        public static DateTime[] ToDateCollection(string value, char separator)
        {
            return ToDateCollection(value, separator, false);
        }

        /// <summary>
        /// Преобразует строку в массив значений типа DateTime без времени
        /// </summary>
        /// <param name="value">строковый массив</param>
        /// <param name="separator">разделитель</param>
        /// <param name="removeEmptyEntries">признак, разрешающий удалять пустые значения</param>
        /// <returns>массив значений типа DateTime без времени</returns>
        public static DateTime[] ToDateCollection(string value, char separator, bool removeEmptyEntries)
        {
            return ToDateCollection(value, separator, removeEmptyEntries, DateTime.MinValue);
        }

        /// <summary>
        /// Преобразует строку в массив значений типа DateTime без времени
        /// </summary>
        /// <param name="value">строковый массив</param>
        /// <param name="separator">разделитель</param>
        /// <param name="removeEmptyEntries">признак, разрешающий удалять пустые значения</param>
        /// <param name="defaultValue">значение по умолчанию</param>
        /// <returns>массив значений типа DateTime без времени</returns>
        public static DateTime[] ToDateCollection(string value, char separator, bool removeEmptyEntries, DateTime defaultValue)
        {
            var result = new DateTime[0];

            var processedCollection = ToStringCollection(value, separator, removeEmptyEntries);
            var processedCount = processedCollection.Length;

            if (processedCount > 0)
            {
                result = new DateTime[processedCount];

                for (var processedIndex = 0; processedIndex < processedCount; processedIndex++)
                {
                    result[processedIndex] = ToDate(processedCollection[processedIndex], defaultValue);
                }
            }

            return result;
        }

        /// <summary>
        /// Производит динамическое приведение значения к заданному типу
        /// </summary>
        /// <param name="value">значение</param>
        /// <param name="conversionType">тип данных</param>
        /// <returns>значение, приведенное к заданному типу</returns>
        public static object ChangeType(object value, Type conversionType)
        {
            object result;
            if (conversionType == typeof(char?))
            {
                result = (char?)value;
            }
            else if (conversionType == typeof(short?))
            {
                result = (short?)value;
            }
            else if (conversionType == typeof(int?))
            {
                result = (int?)value;
            }
            else if (conversionType == typeof(long?))
            {
                result = (long?)value;
            }
            else if (conversionType == typeof(ushort?))
            {
                result = (ushort?)value;
            }
            else if (conversionType == typeof(uint?))
            {
                result = (uint?)value;
            }
            else if (conversionType == typeof(ulong?))
            {
                result = (ulong?)value;
            }
            else if (conversionType == typeof(decimal?))
            {
                result = (decimal?)value;
            }
            else if (conversionType == typeof(double?))
            {
                result = (double?)value;
            }
            else if (conversionType == typeof(float?))
            {
                result = (float?)value;
            }
            else if (conversionType == typeof(bool?))
            {
                result = (bool?)value;
            }
            else if (conversionType == typeof(sbyte?))
            {
                result = (sbyte?)value;
            }
            else if (conversionType == typeof(byte?))
            {
                result = (byte?)value;
            }
            else if (conversionType == typeof(DateTime?))
            {
                result = (DateTime?)value;
            }
            else if (conversionType == typeof(TimeSpan?))
            {
                result = (TimeSpan?)value;
            }
            else if (conversionType == typeof(Guid?))
            {
                result = (Guid?)value;
            }
            else
            {
                result = Convert.ChangeType(value, conversionType);
            }

            return result;
        }

        /// <summary>
        /// Преобразует тип в NotNullable-тип
        /// </summary>
        /// <param name="type">тип</param>
        /// <returns>NotNullable-тип</returns>
        public static Type ToNotNullableType(Type type)
        {
            var newType = type;

            if (newType == typeof(char?))
            {
                newType = typeof(char);
            }
            else if (newType == typeof(short?))
            {
                newType = typeof(short);
            }
            else if (newType == typeof(int?))
            {
                newType = typeof(int);
            }
            else if (newType == typeof(long?))
            {
                newType = typeof(long);
            }
            else if (newType == typeof(ushort?))
            {
                newType = typeof(ushort);
            }
            else if (newType == typeof(uint?))
            {
                newType = typeof(uint);
            }
            else if (newType == typeof(ulong?))
            {
                newType = typeof(ulong);
            }
            else if (newType == typeof(decimal?))
            {
                newType = typeof(decimal);
            }
            else if (newType == typeof(double?))
            {
                newType = typeof(double);
            }
            else if (newType == typeof(float?))
            {
                newType = typeof(float);
            }
            else if (newType == typeof(bool?))
            {
                newType = typeof(bool);
            }
            else if (newType == typeof(sbyte?))
            {
                newType = typeof(sbyte);
            }
            else if (newType == typeof(byte?))
            {
                newType = typeof(byte);
            }
            else if (newType == typeof(DateTime?))
            {
                newType = typeof(DateTime);
            }
            else if (newType == typeof(TimeSpan?))
            {
                newType = typeof(TimeSpan);
            }
            else if (newType == typeof(Guid?))
            {
                newType = typeof(Guid);
            }

            return newType;
        }

        /// <summary>
        /// Возвращает код типа данных JavaScript, соответствующий
        /// указанному .Net-типу
        /// </summary>
        /// <param name="type">тип данных</param>
        /// <returns>код типа данных JavaScript</returns>
        public static string GetJsTypeCodeByType(Type type)
        {
            var typeCode = JsTypeCode.Undefined;

            if (type == typeof(string))
            {
                typeCode = JsTypeCode.String;
            }
            if (type == typeof(short) || type == typeof(int) || type == typeof(long)
                || type == typeof(ushort) || type == typeof(uint) || type == typeof(ulong))
            {
                typeCode = JsTypeCode.Int;
            }
            else if (type == typeof(decimal) || type == typeof(double) || type == typeof(float))
            {
                typeCode = JsTypeCode.Float;
            }
            else if (type == typeof(bool))
            {
                typeCode = JsTypeCode.Boolean;
            }
            else if (type == typeof(DateTime))
            {
                typeCode = JsTypeCode.Date;
            }

            return typeCode;
        }

        ///<summary>
        /// Преобразует строку в JS-строку
        ///</summary>
        ///<param name="value">значение</param>
        ///<returns>JS-строка</returns>
        public static string JsEncode(string value)
        {
            var result = value;

            if (!string.IsNullOrWhiteSpace(value))
            {
                result = result.Replace("\\", "\\\\");
                result = result.Replace("/", "\\/");
                result = result.Replace("\"", "\\\"");
                result = result.Replace("\'", "\\\'");
            }

            return result;
        }

        /// <summary>
        /// Преобразует значение типа Boolean в JS-строку
        /// </summary>
        /// <param name="value">значение</param>
        /// <returns>JS-строка</returns>
        public static string ToJsString(bool value)
        {
            return value.ToString().ToLowerInvariant();
        }

        /// <summary>
        /// Преобразует ip-адрес в значение типа Int64
        /// </summary>
        /// <param name="ip">ip-адрес</param>
        /// <returns>целочисленное представление ip-адреса</returns>
        public static long IpToInt64(string ip)
        {
            var processedIp = ip;
            if (string.Equals(processedIp, "::1", StringComparison.InvariantCultureIgnoreCase))
            {
                processedIp = "127.0.0.1";
            }
            var arr = processedIp.Split('.');
            long number = 0;

            if (arr.Length == 4)
            {
                number = long.Parse(arr[0]) * 16777216 +
                    long.Parse(arr[1]) * 65536 +
                    long.Parse(arr[2]) * 256 +
                    long.Parse(arr[3]);
            }

            return number;
        }

        /// <summary>
		/// Преобразует значение типа Int64 в ip-адрес
        /// </summary>
        /// <param name="number">целочисленное представление ip-адреса</param>
        /// <returns>ip-адрес</returns>
        [SuppressMessage("ReSharper", "PossibleLossOfFraction")]
        public static string Int64ToIp(long number)
        {
            var arr = new double[4];
            arr[0] = Math.Floor((double)(number / 16777216));
            arr[1] = Math.Floor((number - arr[0] * 16777216) / 65536);
            arr[2] = Math.Floor((number - arr[0] * 16777216 - arr[1] * 65536) / 256);
            arr[3] = number - arr[0] * 16777216 - arr[1] * 65536 - arr[2] * 256;

            var ip = Convert.ToInt32(arr[0]) + "." +
                Convert.ToInt32(arr[1]) + "." +
                Convert.ToInt32(arr[2]) + "." +
                Convert.ToInt32(arr[3]);

            return ip;
        }

        /// <summary>
        /// Преобразует объект в значение типа Nullable
        /// </summary>
        /// <param name="value">значение</param>
        /// <returns>преобразованное значение</returns>
        public static int? ToNullableInt32(object value)
        {
            return ToNullableInt32(value, null);
        }

        /// <summary>
        /// Преобразует объект в значение типа Nullable
        /// </summary>
        /// <param name="value">значение</param>
        /// <param name="defaultValue">значение по умолчанию</param>
        /// <returns>преобразованное значение</returns>
        public static int? ToNullableInt32(object value, int? defaultValue)
        {
            int? result;
            if (!IsNullOrEmpty(value))
            {
                var processedValue = value.ToString().Trim();
                if (string.Equals(processedValue, "true", StringComparison.InvariantCultureIgnoreCase))
                {
                    return 1;
                }

                if (string.Equals(processedValue, "false", StringComparison.InvariantCultureIgnoreCase))
                {
                    return 0;
                }

                try
                {
                    result = int.Parse(processedValue);
                }
                catch
                {
                    result = defaultValue;
                }
            }
            else
            {
                result = null;
            }

            return result;
        }

        /// <summary>
        /// Преобразует значение типа Nullable в значение типа Nullable
        /// </summary>
        /// <param name="value">значение</param>
        /// <returns>преобразованное значение</returns>
        public static int? ToNullableInt32(decimal? value)
        {
            if (value == null)
            {
                return null;
            }

            return (int)(decimal)value;
        }

        /// <summary>
        /// Преобразует объект в значение типа Nullable
        /// </summary>
        /// <param name="value">значение</param>
        /// <returns>преобразованное значение</returns>
        public static long? ToNullableInt64(object value)
        {
            return ToNullableInt64(value, null);
        }

        /// <summary>
        /// Преобразует объект в значение типа Nullable
        /// </summary>
        /// <param name="value">значение</param>
        /// <param name="defaultValue">значение по умолчанию</param>
        /// <returns>преобразованное значение</returns>
        public static long? ToNullableInt64(object value, long? defaultValue)
        {
            long? result;
            if (!IsNullOrEmpty(value))
            {
                var processedValue = value.ToString().Trim();
                if (string.Equals(processedValue, "true", StringComparison.InvariantCultureIgnoreCase))
                {
                    return 1;
                }

                if (string.Equals(processedValue, "false", StringComparison.InvariantCultureIgnoreCase))
                {
                    return 0;
                }

                try
                {
                    result = long.Parse(processedValue);
                }
                catch
                {
                    result = defaultValue;
                }
            }
            else
            {
                result = null;
            }

            return result;
        }

        /// <summary>
        /// Преобразует объект в значение типа Nullable
        /// </summary>
        /// <param name="value">значение</param>
        /// <returns>преобразованное значение</returns>
        public static decimal? ToNullableDecimal(object value)
        {
            return ToNullableDecimal(value, null);
        }

        /// <summary>
        /// Преобразует объект в значение типа Nullable
        /// </summary>
        /// <param name="value">значение</param>
        /// <param name="defaultValue">значение по умолчанию</param>
        /// <returns>преобразованное значение</returns>
        public static decimal? ToNullableDecimal(object value, decimal? defaultValue)
        {
            decimal? result;
            if (!IsNullOrEmpty(value))
            {
                var processedValue = value.ToString().Trim();
                if (string.Equals(processedValue, "true", StringComparison.InvariantCultureIgnoreCase))
                {
                    return 1;
                }

                if (string.Equals(processedValue, "false", StringComparison.InvariantCultureIgnoreCase))
                {
                    return 0;
                }

                try
                {
                    result = decimal.Parse(processedValue);
                }
                catch
                {
                    result = defaultValue;
                }
            }
            else
            {
                result = null;
            }

            return result;
        }

        /// <summary>
        /// Преобразует объект в значение типа Nullable
        /// </summary>
        /// <param name="value">значение</param>
        /// <returns>преобразованное значение</returns>
        public static double? ToNullableDouble(object value)
        {
            return IsNullOrEmpty(value) ? null : (double?)ToDouble(value);
        }

        /// <summary>
        /// Преобразует объект в значение типа Nullable
        /// </summary>
        /// <param name="value">значение</param>
        /// <returns>преобразованное значение</returns>
        public static bool? ToNullableBoolean(object value)
        {
            return ToNullableBoolean(value, null);
        }

        /// <summary>
        /// Преобразует объект в значение типа Nullable
        /// </summary>
        /// <param name="value">значение</param>
        /// <param name="defaultValue">значение по умолчанию</param>
        /// <returns>преобразованное значение</returns>
        public static bool? ToNullableBoolean(object value, bool? defaultValue)
        {
            bool? result;
            if (!IsNullOrEmpty(value))
            {
                var processedValue = value.ToString().Trim().ToLower();
                if (processedValue == "true")
                {
                    result = true;
                }
                else if (processedValue == "false")
                {
                    result = false;
                }
                else if (processedValue == "1")
                {
                    result = true;
                }
                else if (processedValue == "0")
                {
                    result = false;
                }
                else
                {
                    try
                    {
                        result = bool.Parse(processedValue);
                    }
                    catch
                    {
                        result = defaultValue;
                    }
                }
            }
            else
            {
                result = null;
            }

            return result;
        }


        /// <summary>
        /// Преобразует объект в объект типа Nullable
        /// </summary>
        /// <param name="value">значение</param>
        /// <param name="defaultValue">значение по умолчанию</param>
        /// <returns>преобразованное значение</returns>
        public static DateTime? ToNullableDateTime(object value, DateTime? defaultValue = null)
        {
            DateTime? result;
            if (!IsNullOrEmpty(value))
            {
                var processedValue = value.ToString().Trim();
                DateTime dt;
                result = DateTime.TryParse(processedValue, out dt) ? dt : defaultValue;
            }
            else
            {
                result = null;
            }

            return result;
        }

        /// <summary>
        /// Преобразует строку с датой в определенном формате в формат даты для SQLServer (yyyyMMdd HH:mm:ss)
        /// </summary>
        public static bool TryConvertToSqlDateString(string dateString, TimeSpan? time, out string sqlDateString, out DateTime? dateTime, string format = null)
        {
            DateTime dt;
            var parseResult = string.IsNullOrEmpty(format)
                ? DateTime.TryParse(dateString, out dt)
                : DateTime.TryParseExact(dateString, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out dt);

            if (parseResult)
            {
                if (time.HasValue)
                {
                    dt = dt.Date.Add(time.Value);
                }
                sqlDateString = dt.ToString("yyyyMMdd HH:mm:ss");
                dateTime = dt;
                return true;
            }

            sqlDateString = string.Empty;
            dateTime = null;
            return false;
        }

        /// <summary>
        /// Преобразует строку с временем в формат времени для SQLServer (HH:mm:ss)
        /// </summary>
        public static bool TryConvertToSqlTimeString(string timeString, out string sqlTimeString, out TimeSpan? timeSpan, string format = null)
        {
            DateTime dt;
            var parseResult = string.IsNullOrEmpty(format)
                ? DateTime.TryParse(timeString, out dt)
                : DateTime.TryParseExact(timeString, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out dt);

            if (parseResult)
            {
                sqlTimeString = dt.ToString("HH:mm:ss");
                timeSpan = dt.TimeOfDay;
                return true;
            }

            sqlTimeString = string.Empty;
            timeSpan = null;
            return false;
        }

        /// <summary>
        /// Преобразует строку с датой в определенном формате в формат даты для SQLServer (yyyyMMdd HH:mm:ss)
        /// </summary>
        public static bool TryConvertToSqlDateTimeString(string dateTimeString, out string sqlDateString, out DateTime? dateTime, string format = null, string sqlFormat = "yyyyMMdd HH:mm:ss")
        {
            DateTime dt;
            var parseResult = string.IsNullOrEmpty(format)
                ? DateTime.TryParse(dateTimeString, out dt)
                : DateTime.TryParseExact(dateTimeString, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out dt);

            if (parseResult)
            {
                sqlDateString = dt.ToString(sqlFormat);
                dateTime = dt;
                return true;
            }

            sqlDateString = string.Empty;
            dateTime = null;
            return false;
        }

        /// <summary>
        /// Конвертирует строку с датой в текущей локали в формат yyyy-MM-dd HH:mm:ss
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string ToDbDateTimeString(string value)
        {
            return ToDbDateTimeString(ToNullableDateTime(value));
        }

        /// <summary>
        /// Конвертирует дату в формат yyyy-MM-dd HH:mm:ss
        /// </summary>
        public static string ToDbDateTimeString(DateTime? dt)
        {
            return dt?.ToString("yyyy-MM-dd HH:mm:ss");
        }

        /// <summary>
        /// Конвертирует время в формат yyyy-MM-dd HH:mm:ss
        /// дата - текущая
        /// </summary>
        public static string ToDbDateTimeString(TimeSpan? t)
        {
            return t.HasValue ? DateTime.Now.Date.Add(t.Value).ToString("yyyy-MM-dd HH:mm:ss") : null;
        }

        /// <summary>
        /// Конвертирует строку с числом в текущей локали в InvariantCulture-формат
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string ToDbNumericString(string value)
        {
            return ToDbNumericString(ToNullableDouble(value));
        }

        /// <summary>
        /// Конвертирует строку с числом в текущей локали в InvariantCulture-формат
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string ToDbNumericString(double? value)
        {
            return value?.ToString(CultureInfo.InvariantCulture);
        }

        public static SqlXml ToSqlXml(XElement elem)
        {
            SqlXml result = null;
            if (elem != null)
            {
                using (var sr = new StringReader(elem.ToString()))
                {
                    var xtr = new XmlTextReader(sr);
                    result = new SqlXml(xtr);
                }
            }
            return result;
        }


        public static int[] ToIdArray(string idCommaList)
        {
            return string.IsNullOrEmpty(idCommaList) ? new int[0] : idCommaList.Split(',').Select(n => ToInt32(n, 0)).Where(n => n != 0).ToArray();
        }

        public static string ToIdCommaList(IEnumerable<int> idEnumerable)
        {
            return idEnumerable == null ? string.Empty : string.Join(",", idEnumerable);
        }

        /// <summary>
        /// Конвертирует объект DataRow в произвольную модель
        /// </summary>
        /// <typeparam name="TModel"></typeparam>
        /// <param name="row"></param>
        /// <returns></returns>
        public static TModel ToModelFromDataRow<TModel>(DataRow row)
        where TModel : class
        {
            try
            {
                var model = Activator.CreateInstance<TModel>();

                foreach (var property in typeof(TModel).GetProperties())
                {
                    foreach (DataColumn key in row.Table.Columns)
                    {
                        var columnName = key.ColumnName;
                        if (!string.IsNullOrEmpty(row[columnName].ToString()))
                        {
                            if (property.Name == columnName)
                            {
                                var t = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;
                                var safeValue = row[columnName] == null ? null : Convert.ChangeType(row[columnName], t);
                                property.SetValue(model, safeValue, null);
                            }
                        }
                    }
                }

                return model;
            }
            catch (MissingMethodException)
            {
                return null;
            }
        }
    }
}
