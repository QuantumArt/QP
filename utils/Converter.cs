using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;
using System.Data.SqlTypes;
using System.Xml.Linq;
using System.IO;
using System.Xml;
using System.Data;

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
			else if (typeof(T) == typeof(Int32))
			{
				Int32 i;
				return Int32.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out i);
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
            bool result = true;

            if (value != null)
            {
                if (value.ToString().Length > 0)
                {
                    result = false;
                }
            }

            return result;
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
				string processedValue = value.Trim().ToLower();
				if (processedValue.Length == 0 || processedValue == "null")
				{
					return null;
				}
				else
				{
					return value;
				}
			}
			else
			{
				return null;
			}
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
            string result = defaultValue;

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
			int result = defaultValue;
			int? rawResult = ToNullableInt32(value);

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
			long result = defaultValue;
			long? rawResult = ToNullableInt64(value);

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
			decimal result = defaultValue;
			decimal? rawResult = ToNullableDecimal(value);

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
            double result = defaultValue;

            if (!IsNullOrEmpty(value))
            {
				string processedValue = value.ToString().Trim();
				if (String.Equals(processedValue, "true", StringComparison.InvariantCultureIgnoreCase))
				{
					return 1;
				}
				else if (String.Equals(processedValue, "false", StringComparison.InvariantCultureIgnoreCase))
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
            float result = defaultValue;

            if (!IsNullOrEmpty(value))
            {
				string processedValue = value.ToString().Trim();
				if (String.Equals(processedValue, "true", StringComparison.InvariantCultureIgnoreCase))
				{
					return 1;
				}
				else if (String.Equals(processedValue, "false", StringComparison.InvariantCultureIgnoreCase))
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
            bool result = defaultValue;
			bool? rawResult = ToNullableBoolean(value);

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
            DateTime result = defaultValue;
			DateTime? rawResult = ToNullableDateTime(value);

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
            DateTime result = defaultValue;

            result = ToDateTime(value, defaultValue);
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
            DateTime result = DateTime.MinValue;
            int year = value.Year;
            int month = value.Month;
            int day = value.Day;

            result = new DateTime(year, month, day);

            return result;
        }
		
		/// <summary>
		/// Преобразует массив значений типа Int32 в строковый массив
		/// </summary>
		/// <param name="value">массив значений типа Int32</param>
		/// <returns>строковый массив</returns>
		public static string[] ToStringCollection(int[] value)
		{
			string[] result = new string[value.Length];

			for (int i = 0; i < value.Length; i++)
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
			string[] result = new string[0];
			string rawString = Converter.ToString(value).Trim();

			if (rawString.Length > 0)
			{
				string[] rawCollection = rawString.Split(separator); // коллекция необработанных элементов
				int rawCount = rawCollection.Length; // количество элементов

				if (rawCount > 0)
				{
					string[] processedCollection = new string[rawCount]; // коллекция обработанных элементов
					int nonEmptyElementsCount = 0; // количество не пустых элементов

					for (int rawIndex = 0; rawIndex < rawCount; rawIndex++)
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

						for (int processedIndex = 0; processedIndex < nonEmptyElementsCount; processedIndex++)
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

						for (int processedIndex = 0; processedIndex < rawCount; processedIndex++)
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
			int[] result = new int[value.Length];

			if (value.Length > 0)
			{
				for (int i = 0; i < value.Length; i++)
				{
					result[i] = Converter.ToInt32(value[i]);
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
			int[] result = new int[0];

			string[] processedCollection = ToStringCollection(value, separator, removeEmptyEntries);
			int processedCount = processedCollection.Length;

			if (processedCount > 0)
			{
				result = new int[processedCount];

				for (int processedIndex = 0; processedIndex < processedCount; processedIndex++)
				{
					result[processedIndex] = Converter.ToInt32(processedCollection[processedIndex], defaultValue);
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
			long[] result = new long[0];

			string[] processedCollection = ToStringCollection(value, separator, removeEmptyEntries);
			int processedCount = processedCollection.Length;

			if (processedCount > 0)
			{
				result = new long[processedCount];

				for (int processedIndex = 0; processedIndex < processedCount; processedIndex++)
				{
					result[processedIndex] = Converter.ToInt64(processedCollection[processedIndex], defaultValue);
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
			decimal[] result = new decimal[0];

			string[] processedCollection = ToStringCollection(value, separator, removeEmptyEntries);
			int processedCount = processedCollection.Length;

			if (processedCount > 0)
			{
				result = new decimal[processedCount];

				for (int processedIndex = 0; processedIndex < processedCount; processedIndex++)
				{
					result[processedIndex] = Converter.ToDecimal(processedCollection[processedIndex], defaultValue);
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
			double[] result = new double[0];

			string[] processedCollection = ToStringCollection(value, separator, removeEmptyEntries);
			int processedCount = processedCollection.Length;

			if (processedCount > 0)
			{
				result = new double[processedCount];

				for (int processedIndex = 0; processedIndex < processedCount; processedIndex++)
				{
					result[processedIndex] = Converter.ToDouble(processedCollection[processedIndex], defaultValue);
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
			float[] result = new float[0];

			string[] processedCollection = ToStringCollection(value, separator, removeEmptyEntries);
			int processedCount = processedCollection.Length;

			if (processedCount > 0)
			{
				result = new float[processedCount];

				for (int processedIndex = 0; processedIndex < processedCount; processedIndex++)
				{
					result[processedIndex] = Converter.ToFloat(processedCollection[processedIndex], defaultValue);
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
			bool[] result = new bool[0];

			string[] processedCollection = ToStringCollection(value, separator, removeEmptyEntries);
			int processedCount = processedCollection.Length;

			if (processedCount > 0)
			{
				result = new bool[processedCount];

				for (int processedIndex = 0; processedIndex < processedCount; processedIndex++)
				{
					result[processedIndex] = Converter.ToBoolean(processedCollection[processedIndex], defaultValue);
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
			DateTime[] result = new DateTime[0];

			string[] processedCollection = ToStringCollection(value, separator, removeEmptyEntries);
			int processedCount = processedCollection.Length;

			if (processedCount > 0)
			{
				result = new DateTime[processedCount];

				for (int processedIndex = 0; processedIndex < processedCount; processedIndex++)
				{
					result[processedIndex] = Converter.ToDateTime(processedCollection[processedIndex], defaultValue);
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
			DateTime[] result = new DateTime[0];

			string[] processedCollection = ToStringCollection(value, separator, removeEmptyEntries);
			int processedCount = processedCollection.Length;

			if (processedCount > 0)
			{
				result = new DateTime[processedCount];

				for (int processedIndex = 0; processedIndex < processedCount; processedIndex++)
				{
					result[processedIndex] = Converter.ToDate(processedCollection[processedIndex], defaultValue);
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
			object result = null;

			if (conversionType == typeof(Nullable<Char>))
			{
				result = (Nullable<Char>)value;
			}
			else if (conversionType == typeof(Nullable<Int16>))
			{
				result = (Nullable<Int16>)value;
			}
			else if (conversionType == typeof(Nullable<Int32>))
			{
				result = (Nullable<Int32>)value;
			}
			else if (conversionType == typeof(Nullable<Int64>))
			{
				result = (Nullable<Int64>)value;
			}
			else if (conversionType == typeof(Nullable<UInt16>))
			{
				result = (Nullable<UInt16>)value;
			}
			else if (conversionType == typeof(Nullable<UInt32>))
			{
				result = (Nullable<UInt32>)value;
			}
			else if (conversionType == typeof(Nullable<UInt64>))
			{
				result = (Nullable<UInt64>)value;
			}
			else if (conversionType == typeof(Nullable<Decimal>))
			{
				result = (Nullable<Decimal>)value;
			}
			else if (conversionType == typeof(Nullable<Double>))
			{
				result = (Nullable<Double>)value;
			}
			else if (conversionType == typeof(Nullable<float>))
			{
				result = (Nullable<float>)value;
			}
			else if (conversionType == typeof(Nullable<Boolean>))
			{
				result = (Nullable<Boolean>)value;
			}
			else if (conversionType == typeof(Nullable<SByte>))
			{
				result = (Nullable<SByte>)value;
			}
			else if (conversionType == typeof(Nullable<Byte>))
			{
				result = (Nullable<Byte>)value;
			}
			else if (conversionType == typeof(Nullable<DateTime>))
			{
				result = (Nullable<DateTime>)value;
			}
			else if (conversionType == typeof(Nullable<TimeSpan>))
			{
				result = (Nullable<TimeSpan>)value;
			}
			else if (conversionType == typeof(Nullable<Guid>))
			{
				result = (Nullable<Guid>)value;
			}
			else
			{
				result = Convert.ChangeType(value, conversionType);
			}

			return result;
		}

		/// <summary>
		/// Производит динамическое приведение значения к заданному типу
		/// </summary>
		/// <typeparam name="T">тип данных</typeparam>
		/// <param name="value">значение</param>
		/// <param name="conversionType">тип данных</param>
		/// <returns>значение, приведенное к заданному типу</returns>
		public static T ChangeType<T>(this object value, T conversionType)
		{
			return ChangeType(value, conversionType);
		}

		/// <summary>
		/// Преобразует тип в NotNullable-тип
		/// </summary>
		/// <param name="type">тип</param>
		/// <returns>NotNullable-тип</returns>
		public static Type ToNotNullableType(Type type)
		{
			Type newType = type;

			if (newType == typeof(Nullable<Char>))
			{
				newType = typeof(Char);
			}
			else if (newType == typeof(Nullable<Int16>))
			{
				newType = typeof(Int16);
			}
			else if (newType == typeof(Nullable<Int32>))
			{
				newType = typeof(Int32);
			}
			else if (newType == typeof(Nullable<Int64>))
			{
				newType = typeof(Int64);
			}
			else if (newType == typeof(Nullable<UInt16>))
			{
				newType = typeof(UInt16);
			}
			else if (newType == typeof(Nullable<UInt32>))
			{
				newType = typeof(UInt32);
			}
			else if (newType == typeof(Nullable<UInt64>))
			{
				newType = typeof(UInt64);
			}
			else if (newType == typeof(Nullable<Decimal>))
			{
				newType = typeof(Decimal);
			}
			else if (newType == typeof(Nullable<Double>))
			{
				newType = typeof(Double);
			}
			else if (newType == typeof(Nullable<float>))
			{
				newType = typeof(float);
			}
			else if (newType == typeof(Nullable<Boolean>))
			{
				newType = typeof(Boolean);
			}
			else if (newType == typeof(Nullable<SByte>))
			{
				newType = typeof(SByte);
			}
			else if (newType == typeof(Nullable<Byte>))
			{
				newType = typeof(Byte);
			}
			else if (newType == typeof(Nullable<DateTime>))
			{
				newType = typeof(DateTime);
			}
			else if (newType == typeof(Nullable<TimeSpan>))
			{
				newType = typeof(TimeSpan);
			}
			else if (newType == typeof(Nullable<Guid>))
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
			string typeCode = Constants.JsTypeCode.Undefined;

			if (type == typeof(String))
			{
				typeCode = Constants.JsTypeCode.String;
			}
			if (type == typeof(Int16) || type == typeof(Int32) || type == typeof(Int64)
				|| type == typeof(UInt16) || type == typeof(UInt32) || type == typeof(UInt64))
			{
				typeCode = Constants.JsTypeCode.Int;
			}
			else if (type == typeof(Decimal) || type == typeof(Double) || type == typeof(float))
			{
				typeCode = Constants.JsTypeCode.Float;
			}
			else if (type == typeof(Boolean))
			{
				typeCode = Constants.JsTypeCode.Boolean;
			}
			else if (type == typeof(DateTime))
			{
				typeCode = Constants.JsTypeCode.Date;
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
			string result = value;

			if (!String.IsNullOrWhiteSpace(value))
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
			string processedIp = ip;
			if (string.Equals(processedIp, "::1", StringComparison.InvariantCultureIgnoreCase))
			{
				processedIp = "127.0.0.1";
			}
			string[] arr = processedIp.Split('.');
			long number = 0;

			if (arr.Length == 4)
			{
				number = Int64.Parse(arr[0]) * 16777216 +
					Int64.Parse(arr[1]) * 65536 +
					Int64.Parse(arr[2]) * 256 +
					Int64.Parse(arr[3]);
			}

			return number;
		}

        /// <summary>
		/// Преобразует значение типа Int64 в ip-адрес
        /// </summary>
        /// <param name="number">целочисленное представление ip-адреса</param>
        /// <returns>ip-адрес</returns>
        public static string Int64ToIp(long number)
        {
            double[] arr = new double[4];

            arr[0] = Math.Floor((double)(number / 16777216));
            arr[1] = Math.Floor((double)((number - arr[0] * 16777216) / 65536));
            arr[2] = Math.Floor((double)((number - arr[0] * 16777216 - arr[1] * 65536) / 256));
            arr[3] = (double)((double)(number - arr[0] * 16777216 - arr[1] * 65536 - arr[2] * 256));

            string ip = Convert.ToInt32(arr[0]).ToString() + "." +
                Convert.ToInt32(arr[1]).ToString() + "." +
                Convert.ToInt32(arr[2]).ToString() + "." +
                Convert.ToInt32(arr[3]).ToString();

            return ip;
        }

		/// <summary>
		/// Преобразует объект в значение типа Nullable<Int32>
		/// </summary>
		/// <param name="value">значение</param>
		/// <returns>преобразованное значение</returns>
		public static int? ToNullableInt32(object value)
		{			
			return ToNullableInt32(value, null);
		}

		/// <summary>
		/// Преобразует объект в значение типа Nullable<Int32>
		/// </summary>
		/// <param name="value">значение</param>
		/// <param name="defaultValue">значение по умолчанию</param>
		/// <returns>преобразованное значение</returns>
		public static int? ToNullableInt32(object value, int? defaultValue)
		{
			int? result = defaultValue;

			if (!IsNullOrEmpty(value))
			{
				string processedValue = value.ToString().Trim();
				if (String.Equals(processedValue, "true", StringComparison.InvariantCultureIgnoreCase))
				{
					return 1;
				}
				else if (String.Equals(processedValue, "false", StringComparison.InvariantCultureIgnoreCase))
				{
					return 0;
				}

				try
				{
					result = Int32.Parse(processedValue);
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
		/// Преобразует значение типа Nullable<Decimal> в значение типа Nullable<Int32>
		/// </summary>
		/// <param name="value">значение</param>
		/// <param name="defaultValue">значение по умолчанию</param>
		/// <returns>преобразованное значение</returns>
        public static int? ToNullableInt32(decimal? value)
        {
            if (value == null)
                return null;
            else
            {
                return (int)((decimal)value);
            }
        }

		/// <summary>
		/// Преобразует объект в значение типа Nullable<Int64>
		/// </summary>
		/// <param name="value">значение</param>
		/// <returns>преобразованное значение</returns>
		public static long? ToNullableInt64(object value)
		{
			return ToNullableInt64(value, null);
		}

		/// <summary>
		/// Преобразует объект в значение типа Nullable<Int64>
		/// </summary>
		/// <param name="value">значение</param>
		/// <param name="defaultValue">значение по умолчанию</param>
		/// <returns>преобразованное значение</returns>
		public static long? ToNullableInt64(object value, long? defaultValue)
		{
			long? result = defaultValue;

			if (!IsNullOrEmpty(value))
			{
				string processedValue = value.ToString().Trim();
				if (String.Equals(processedValue, "true", StringComparison.InvariantCultureIgnoreCase))
				{
					return 1;
				}
				else if (String.Equals(processedValue, "false", StringComparison.InvariantCultureIgnoreCase))
				{
					return 0;
				}

				try
				{
					result = Int64.Parse(processedValue);
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
		/// Преобразует объект в значение типа Nullable<Decimal>
		/// </summary>
		/// <param name="value">значение</param>
		/// <returns>преобразованное значение</returns>
		public static decimal? ToNullableDecimal(object value)
		{			
			return ToNullableDecimal(value, null);
		}

		/// <summary>
		/// Преобразует объект в значение типа Nullable<Decimal>
		/// </summary>
		/// <param name="value">значение</param>
		/// <param name="defaultValue">значение по умолчанию</param>
		/// <returns>преобразованное значение</returns>
		public static decimal? ToNullableDecimal(object value, decimal? defaultValue)
		{
			decimal? result = defaultValue;

			if (!IsNullOrEmpty(value))
			{
				string processedValue = value.ToString().Trim();
				if (String.Equals(processedValue, "true", StringComparison.InvariantCultureIgnoreCase))
				{
					return 1;
				}
				else if (String.Equals(processedValue, "false", StringComparison.InvariantCultureIgnoreCase))
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
		/// Преобразует значение типа Nullable<Int32> в значение типа Nullable<Decimal>
		/// </summary>
		/// <param name="value">значение</param>
		/// <returns>преобразованное значение</returns>
        public static decimal? ToNullableDecimal(int? value)
        {
			if (value != null)
			{
				return (decimal)((int)value);
			}
			else
			{
				return null;
			}
        }

		/// <summary>
		/// Преобразует объект в значение типа Nullable<Double>
		/// </summary>
		/// <param name="value">значение</param>
		/// <returns>преобразованное значение</returns>
		public static double? ToNullableDouble(object value)
		{			
			return IsNullOrEmpty(value) ? null : (double?)ToDouble(value);
		}

		/// <summary>
		/// Преобразует объект в значение типа Nullable<Boolean>
		/// </summary>
		/// <param name="value">значение</param>
		/// <returns>преобразованное значение</returns>
		public static bool? ToNullableBoolean(object value)
		{			
			return ToNullableBoolean(value, null);
		}

		/// <summary>
		/// Преобразует объект в значение типа Nullable<Boolean>
		/// </summary>
		/// <param name="value">значение</param>
		/// <param name="defaultValue">значение по умолчанию</param>
		/// <returns>преобразованное значение</returns>
		public static bool? ToNullableBoolean(object value, bool? defaultValue)
        {
            bool? result = defaultValue;

			if (!IsNullOrEmpty(value))
			{
				string processedValue = value.ToString().Trim().ToLower();

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
		/// Преобразует объект в объект типа Nullable<DateTime>
		/// </summary>
		/// <param name="value">значение</param>
		/// <param name="defaultValue">значение по умолчанию</param>
		/// <returns>преобразованное значение</returns>
		public static DateTime? ToNullableDateTime(object value, DateTime? defaultValue = null)
		{
			DateTime? result = defaultValue;

			if (!IsNullOrEmpty(value))
			{
				string processedValue = value.ToString().Trim();
				DateTime dt;
				if (DateTime.TryParse(processedValue, out dt))
					result = dt;
				else
					result = defaultValue;
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
        /// <param name="dateString"></param>
        /// <param name="format"></param>
        /// <param name="time"></param>
        /// <param name="sqlDateString"></param>
        /// <returns></returns>
        public static bool TryConvertToSqlDateString(string dateString, TimeSpan? time, out string sqlDateString, out DateTime? dateTime, string format = null)
        {
            DateTime dt;
			bool parseResult = String.IsNullOrEmpty(format) 
				? DateTime.TryParse(dateString, out dt) 
				: DateTime.TryParseExact(dateString, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out dt);
            if (parseResult)
            {
                if (time.HasValue)
                    dt = dt.Date.Add(time.Value);
                sqlDateString = dt.ToString("yyyyMMdd HH:mm:ss");
                dateTime = dt;
                return true;
            }
            else
            {
                sqlDateString = String.Empty;
                dateTime = null;
                return false;
            }
        }

        /// <summary>
        /// Преобразует строку с временем в формат времени для SQLServer (HH:mm:ss)
        /// </summary>
        /// <param name="p"></param>
        /// <param name="p_2"></param>
        /// <param name="sqlTimeString"></param>
        /// <returns></returns>
        public static bool TryConvertToSqlTimeString(string timeString, out string sqlTimeString, out TimeSpan? timeSpan, string format = null)
        {
            DateTime dt;
			bool parseResult = String.IsNullOrEmpty(format)
				? DateTime.TryParse(timeString, out dt)
				: DateTime.TryParseExact(timeString, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out dt);
            if (parseResult)
            {
                sqlTimeString = dt.ToString("HH:mm:ss");
                timeSpan = dt.TimeOfDay;
                return true;
            }
            else
            {
                sqlTimeString = String.Empty;
                timeSpan = null;
                return false;
            }
        }

		/// <summary>
		/// Преобразует строку с датой в определенном формате в формат даты для SQLServer (yyyyMMdd HH:mm:ss)
		/// </summary>
		/// <param name="dateTimeString"></param>
		/// <param name="format"></param>
		/// <param name="time"></param>
		/// <param name="sqlDateString"></param>
		/// <returns></returns>
		public static bool TryConvertToSqlDateTimeString(string dateTimeString, out string sqlDateString, out DateTime? dateTime, string format = null, string sqlFormat = "yyyyMMdd HH:mm:ss")
		{
			DateTime dt;
			bool parseResult = String.IsNullOrEmpty(format)
				? DateTime.TryParse(dateTimeString, out dt)
				: DateTime.TryParseExact(dateTimeString, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out dt);
			if (parseResult)
			{
				sqlDateString = dt.ToString(sqlFormat);
				dateTime = dt;
				return true;
			}
			else
			{
				sqlDateString = String.Empty;
				dateTime = null;
				return false;
			}
		}

		/// <summary>
		/// Конвертирует строку с датой в текущей локали в формат yyyy-MM-dd HH:mm:ss
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public static string ToDbDateTimeString(string value)
		{
			return ToDbDateTimeString(Converter.ToNullableDateTime(value));
		}

		/// <summary>
		/// Конвертирует дату в формат yyyy-MM-dd HH:mm:ss
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public static string ToDbDateTimeString(DateTime? dt)
		{			
			return dt.HasValue ? dt.Value.ToString("yyyy-MM-dd HH:mm:ss") : null;
		}

		/// <summary>
		/// Конвертирует время в формат yyyy-MM-dd HH:mm:ss
		/// дата - текущая
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
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
			return ToDbNumericString(Converter.ToNullableDouble(value));
		}

		/// <summary>
		/// Конвертирует строку с числом в текущей локали в InvariantCulture-формат
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public static string ToDbNumericString(double? value)
		{
			return value.HasValue ? value.Value.ToString(CultureInfo.InvariantCulture) : null;
		}


        public static SqlXml ToSqlXml(XElement elem)
        {
            SqlXml result = null;
            if (elem != null)
            {
                using (StringReader sr = new StringReader(elem.ToString()))
                {
                    XmlTextReader xtr = new XmlTextReader(sr);
                    result = new SqlXml(xtr);
                }
            }
            return result;
        }


        public static int[] ToIdArray(string idCommaList)
        {
            return (String.IsNullOrEmpty(idCommaList)) ? new int[0] : idCommaList.Split(',').Select(n => Utils.Converter.ToInt32(n, 0)).Where(n => n != 0).ToArray();
        }

        public static string ToIdCommaList(IEnumerable<int> idEnumerable)
        {
            return (idEnumerable == null) ? String.Empty : String.Join(",", idEnumerable);
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
						string columnName = key.ColumnName;
						if (!String.IsNullOrEmpty(row[columnName].ToString()))
						{
							if (property.Name == columnName)
							{
								Type t = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;
								object safeValue = (row[columnName] == null) ? null : Convert.ChangeType(row[columnName], t);
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