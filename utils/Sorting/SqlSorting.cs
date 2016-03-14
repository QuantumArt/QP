using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Quantumart.QP8.Utils.Sorting
{
	/// <summary>
	/// Направление сортировки
	/// </summary>
	public enum SortDirection : int
	{
		Ascending,
		Descending
	}

	/// <summary>
	/// Сортировочная информация
	/// </summary>
	public class SortingInformation
	{
		public SortingInformation(string fieldName, SortDirection direction)
		{
			this.FieldName = fieldName;
			this.Direction = direction;
		}

		/// <summary>
		/// название поля, по которому осуществляется сортировка
		/// </summary>
		public string FieldName
		{
			get;
			set;
		}

		/// <summary>
		/// направление сортировки
		/// </summary>
		public SortDirection Direction
		{
			get;
			set;
		}
	}

	public static class SortingInformationExtensions
	{
		/// <summary>
		/// Возвращает настройки сортировки в виде SQL-кода
		/// </summary>
		/// <param name="sortInfoList">настройки сортировки</param>
		/// <returns>настройки сортировки в виде SQL-кода</returns>
		public static string ToSqlSortExpression(this IList<SortingInformation> sortInfoList)
		{
			StringBuilder sqlSortExpression = new StringBuilder();
			int sortInfoCount = sortInfoList.Count;

			for (int sortInfoIndex = 0; sortInfoIndex < sortInfoCount; sortInfoIndex++)
			{
				SortingInformation sortInfo = sortInfoList[sortInfoIndex];

				string sortColumnName = sortInfo.FieldName;
				string sortDirection = (sortInfo.Direction == SortDirection.Descending) ? "DESC" : "ASC";

				if (sortInfoIndex > 0)
				{
					sqlSortExpression.Append(", ");
				}
				sqlSortExpression.AppendFormat("{0} {1}", sortColumnName, sortDirection);
			}

			return sqlSortExpression.ToString();
		}
	}

	public static class SqlSorting
	{
		// Константы
		private const string SQL_DIRECTION_ASCENDING = "ASC";
		private const string SQL_DIRECTION_DESCENDING = "DESC";

		// Регулярные выражения
		private static Regex _sqlDirectionRegExp = new Regex(@"\b(ASC|DESC)\b",
			RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Compiled);
		private static Regex _sqlAscendingDirectionRegExp = new Regex(@"\b(ASC)\b",
			RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Compiled);
		private static Regex _sqlDescendingDirectionRegExp = new Regex(@"\b(DESC)\b",
			RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Compiled);
		private static Regex _sqlAscendingDirectionWithIndentsRegExp = new Regex(@"\s+(ASC)\s*",
			RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Compiled);
		private static Regex _sqlDescendingDirectionWithIndentsRegExp = new Regex(@"\s+(DESC)\s*",
			RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Compiled);
		private static Regex _squareBracketsRegExp = new Regex(@"[\[|\]]", 
			RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Compiled);

		/// <summary>
		/// Изменяет направление сортировки на противоположное
		/// </summary>
		/// <param name="sortExpression">SQL-код сортировки</param>
		/// <returns>альтернативные SQL-код сортировки</returns>
		public static string ReverseSortExpression(string sortExpression)
		{
			string alternateSortExpression = sortExpression.Trim();

			if (alternateSortExpression.Length > 0
				&& alternateSortExpression.IndexOf(',') != -1)
			{
				string[] sortParameterList = Utils.Converter.ToStringCollection(alternateSortExpression, ',', true);
				int sortParameterCount = sortParameterList.Length;

				if (sortParameterCount > 0)
				{
					alternateSortExpression = "";

					for (int i = 0; i < sortParameterCount; i++)
					{
						string sortParameter = sortParameterList[i].Trim();
						string alternateSortParameter = sortParameter;

						if (_sqlDescendingDirectionRegExp.IsMatch(sortParameter))
						{
							alternateSortParameter = _sqlDescendingDirectionRegExp.Replace(
								sortParameter, SQL_DIRECTION_ASCENDING);
						}
						else
						{
							if (_sqlAscendingDirectionRegExp.IsMatch(sortParameter))
							{
								alternateSortParameter = _sqlAscendingDirectionRegExp.Replace(
									sortParameter, SQL_DIRECTION_DESCENDING);
							}
							else
							{
								sortParameter += " " + SQL_DIRECTION_DESCENDING;
							}
						}

						if (i > 0)
						{
							alternateSortExpression += ", ";
						}
						alternateSortExpression += alternateSortParameter;
					}
				}

				sortParameterList = null;
			}
			else
			{
				if (_sqlDescendingDirectionRegExp.IsMatch(alternateSortExpression))
				{
					alternateSortExpression = _sqlDescendingDirectionRegExp.Replace(alternateSortExpression, SQL_DIRECTION_ASCENDING);
				}
				else
				{
					if (_sqlAscendingDirectionRegExp.IsMatch(alternateSortExpression))
					{
						alternateSortExpression = _sqlAscendingDirectionRegExp.Replace(alternateSortExpression, SQL_DIRECTION_DESCENDING);
					}
					else
					{
						alternateSortExpression += " " + SQL_DIRECTION_DESCENDING;
					}
				}
			}

			return alternateSortExpression;
		}

		/// <summary>
		/// Определяет направление сортировки
		/// </summary>
		/// <param name="sortExpression">SQL-код сортировки</param>
		/// <returns>направление сортировки</returns>
		public static SortDirection RecognizeSortDirection(string sortExpression)
		{
			SortDirection result = SortDirection.Ascending;
			string processedSortExpression = sortExpression.Trim();

			if (processedSortExpression.Length > 0
				&& processedSortExpression.IndexOf(',') != -1)
			{
				string[] sortParameterList = Utils.Converter.ToStringCollection(processedSortExpression, ',', true);
				if (sortParameterList.Length > 0)
				{
					string firstSortParameter = sortParameterList[0];

					if (_sqlDescendingDirectionRegExp.IsMatch(firstSortParameter))
					{
						result = SortDirection.Descending;
					}
				}

				sortParameterList = null;
			}
			else
			{
				if (_sqlDescendingDirectionRegExp.IsMatch(processedSortExpression))
				{
					result = SortDirection.Descending;
				}
			}

			return result;
		}

		/// <summary>
		/// Сравнивает SQL-коды сортировки
		/// </summary>
		/// <param name="firstSortExpression">первый SQL-код сортировки</param>
		/// <param name="secondSortExpression">второй SQL-код сортировки</param>
		/// <returns>результат сравнения</returns>
		public static bool CompareSortExpressions(string firstSortExpression, string secondSortExpression)
		{
			return CompareSortExpressions(firstSortExpression, secondSortExpression, false);
		}

		/// <summary>
		/// Сравнивает SQL-коды сортировки
		/// </summary>
		/// <param name="firstSortExpression">первый SQL-код сортировки</param>
		/// <param name="secondSortExpression">второй SQL-код сортировки</param>
		/// <param name="ignoreSortDirection">разрешает не учитывать направление
		/// сортировки при сравнении</param>
		/// <returns>результат сравнения</returns>
		public static bool CompareSortExpressions(
			string firstSortExpression,
			string secondSortExpression,
			bool ignoreSortDirection)
		{
			bool result = false;

			string firstString = firstSortExpression.Trim();
			firstString = firstString.ToLower();
			firstString = Regex.Replace(firstString, @"\s+", " ", RegexOptions.Multiline);
			firstString = RemoveSquareBrackets(firstString);
			firstString = _sqlAscendingDirectionWithIndentsRegExp.Replace(firstString, "");
			if (ignoreSortDirection)
			{
				firstString = _sqlDescendingDirectionWithIndentsRegExp.Replace(firstString, "");
			}

			string secondString = secondSortExpression.Trim();
			secondString = secondString.ToLower();
			secondString = Regex.Replace(secondString, @"\s+", " ", RegexOptions.Multiline);
			secondString = RemoveSquareBrackets(secondString);
			secondString = _sqlAscendingDirectionWithIndentsRegExp.Replace(secondString, "");
			if (ignoreSortDirection)
			{
				secondString = _sqlDescendingDirectionWithIndentsRegExp.Replace(secondString, "");
			}

			result = (firstString == secondString);

			return result;
		}

		/// <summary>
		/// Удаляет информацию о направлении сортировки из SQL-кода
		/// </summary>
		/// <param name="sortExpression">SQL-код сортировки</param>
		/// <returns>SQL-код сортировки без информации о направлении сортировки</returns>
		public static string RemoveSortDirection(string sortExpression)
		{
			string processedSortExpression = sortExpression.Trim();

			if (processedSortExpression.Length > 0)
			{
				processedSortExpression = _sqlAscendingDirectionWithIndentsRegExp.Replace(
					processedSortExpression, "");
				processedSortExpression = _sqlDescendingDirectionWithIndentsRegExp.Replace(
					processedSortExpression, "");
			}

			return processedSortExpression;
		}

		/// <summary>
		/// Удаляет слово ASC из SQL-кода
		/// </summary>
		/// <param name="sortExpression">SQL-код сортировки</param>
		/// <returns>SQL-код сортировки без слова ASC</returns>
		public static string RemoveAscendingSortDirection(string sortExpression)
		{
			string processedSortExpression = sortExpression.Trim();

			if (processedSortExpression.Length > 0)
			{
				processedSortExpression = _sqlAscendingDirectionWithIndentsRegExp.Replace(
					processedSortExpression, "");
			}

			return processedSortExpression;
		}

		/// <summary>
		/// Проверяет содержит ли SQL-код сортировки квадратные скобки
		/// </summary>
		/// <param name="sortExpression">SQL-код сортировки</param>
		/// <returns>результат проверки (true – содержит; false – не содержит)</returns>
		public static bool ContainsSquareBrackets(string sortExpression)
		{
			return _squareBracketsRegExp.IsMatch(sortExpression);
		}

		/// <summary>
		/// Удаляет квадратные скобки из SQL-кода
		/// </summary>
		/// <param name="sortExpression">SQL-код сортировки</param>
		/// <returns>SQL-код сортировки без квадратных скобок</returns>
		public static string RemoveSquareBrackets(string sortExpression)
		{
			return _squareBracketsRegExp.Replace(sortExpression, "");
		}

		/// <summary>
		/// Изменяет направление сортировки на заданное
		/// </summary>
		/// <param name="sortExpression">SQL-код сортировки</param>
		/// <param name="sortDirection">направление сортировки</param>
		/// <returns>новый SQL-код сортировки</returns>
		public static string ChangeSortExpression(string sortExpression, SortDirection sortDirection)
		{
			string newSortExpression = sortExpression;
			SortDirection currentSortDirection = RecognizeSortDirection(sortExpression);

			if (currentSortDirection != sortDirection)
			{
				newSortExpression = ReverseSortExpression(sortExpression);
			}

			return newSortExpression;
		}

		/// <summary>
		/// Возвращает информацию о сортировке
		/// </summary>
		/// <param name="sortExpression">параметры сортировки</param>
		/// <returns>информация о сортировке</returns>
		public static List<SortingInformation> GetSortingInformations(string sortExpression)
		{
			List<SortingInformation> sortInfoList = new List<SortingInformation>();
			string processedSortExpression = Converter.ToString(sortExpression).Trim();

			if (processedSortExpression.Length > 0)
			{
				if (processedSortExpression.IndexOf(",") != -1)
				{
					string[] sortParameterList = Utils.Converter.ToStringCollection(processedSortExpression, ',', true);
					int sortParameterCount = sortParameterList.Length;

					if (sortParameterCount > 0)
					{
						for (int sortParameterIndex = 0; sortParameterIndex < sortParameterCount; sortParameterIndex++)
						{
							SortingInformation sortInfo = GetSortingInformation(sortParameterList[sortParameterIndex]);
							if (sortInfo != null)
							{
								sortInfoList.Add(sortInfo);
							}
						}
					}

					sortParameterList = null;
				}
				else
				{
					SortingInformation sortInfo = GetSortingInformation(processedSortExpression);
					if (sortInfo != null)
					{
						sortInfoList.Add(sortInfo);
					}
				}
			}

			return sortInfoList;
		}

		/// <summary>
		/// Возвращает информацию о сортировке
		/// </summary>
		/// <param name="sortParameter">параметр сортировки</param>
		/// <returns>информация о сортировке</returns>
		private static SortingInformation GetSortingInformation(string sortParameter)
		{
			SortingInformation sortInfo = null;
			string processedSortParameter = Converter.ToString(sortParameter).Trim();

			if (processedSortParameter.Length > 0)
			{
				string fieldName = String.Empty;
				SortDirection direction = SortDirection.Ascending;

				if (_sqlDirectionRegExp.IsMatch(processedSortParameter))
				{
					fieldName = RemoveSortDirection(processedSortParameter);
					if (_sqlDescendingDirectionRegExp.IsMatch(processedSortParameter))
					{
						direction = SortDirection.Descending;
					}
				}
				else
				{
					fieldName = processedSortParameter;
				}
				fieldName = RemoveSquareBrackets(fieldName);

				sortInfo = new SortingInformation(fieldName, direction);
			}

			return sortInfo;
		}
	}
}
