using Quantumart.QP8.DAL;
using Quantumart.QP8.Utils;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Quantumart.QP8.BLL.Repository.Articles
{
    /// <summary>
    /// Парсер параметров запроса списка статей
    /// Обрабатывает параметры запроса следующих типов:
    /// ArticleFieldSearchType.Text,
    /// ArticleFieldSearchType.Boolean,
    /// ArticleFieldSearchType.DateRange,
    /// ArticleFieldSearchType.TimeRange,
    /// ArticleFieldSearchType.NumericRange,
    /// ArticleFieldSearchType.O2MRelation
    /// </summary>
    public class ArticleFilterSearchQueryParser
	{
		private const string CONTENT_TABLE_ALIAS = "c";

		/// <summary>
		/// Возвращает значение параметра filter
		/// </summary>
		public string GetFilter(IEnumerable<ArticleSearchQueryParam> searchQueryParams, IList<SqlParameter> sqlParams)
		{
			if (searchQueryParams == null)
				return null;
			if (!searchQueryParams.Any())
				return null;

			// оставляем параметры только тех типов которые обрабатываються данным методом
			var processedSqlParams = searchQueryParams.Where(p =>
					(new[]
					{
						ArticleFieldSearchType.Identifier,
						ArticleFieldSearchType.Text,
						ArticleFieldSearchType.Boolean,
						ArticleFieldSearchType.DateRange,
						ArticleFieldSearchType.TimeRange,
						ArticleFieldSearchType.NumericRange,
						ArticleFieldSearchType.O2MRelation,
                        ArticleFieldSearchType.Classifier
					}).Contains(p.SearchType));

			// если нет обрабатываемых параметров - то возвращаем null
			if (!processedSqlParams.Any())
				return null;
			// формируем результат
			var result = SqlFilterComposer.Compose(processedSqlParams.Select(p =>
				{
					if (p.SearchType == ArticleFieldSearchType.Identifier)
						return ParseIdentifierParam(p, sqlParams);
					if(p.SearchType == ArticleFieldSearchType.Text)
						return ParseTextParam(p);
					else if (p.SearchType == ArticleFieldSearchType.DateRange)
						return ParseDateRangeParam(p);
					else if (p.SearchType == ArticleFieldSearchType.TimeRange)
						return ParseTimeRangeParam(p);
					else if (p.SearchType == ArticleFieldSearchType.NumericRange)
						return ParseNumericRangeParam(p);
					else if (p.SearchType == ArticleFieldSearchType.O2MRelation || p.SearchType == ArticleFieldSearchType.Classifier)
						return ParseO2MRelationParam(p, sqlParams);
					else if (p.SearchType == ArticleFieldSearchType.Boolean)
						return ParseBooleanParam(p);
					else
						return null;
				}));

			// если результат - пустая строка - то возвращаем null
			if (String.IsNullOrWhiteSpace(result))
				return null;
			else
				return result;
		}

		private static string ParseIdentifierParam(ArticleSearchQueryParam p, ICollection<SqlParameter> sqlParams)
		{
			Contract.Requires(p != null);
			Contract.Requires(p.SearchType == ArticleFieldSearchType.NumericRange);

			if (String.IsNullOrWhiteSpace(p.FieldColumn))
				throw new ArgumentException("FieldColumn");
			// параметры не пустые и их не меньше 4х (используем 1й, 2й, 3й и 4й - остальные отбрасываем)
			if (p.QueryParams == null || p.QueryParams.Length < 4)
				throw new ArgumentException();
			// первый параметр должен быть bool
			if (!(p.QueryParams[0] is bool))
				throw new InvalidCastException();
			// второй параметр должен быть int или long или null
			if (p.QueryParams[1] != null && !(p.QueryParams[1] is int || p.QueryParams[1] is long))
				throw new InvalidCastException();
			// третий параметр должен быть int или long или null
			if (p.QueryParams[2] != null && !(p.QueryParams[2] is int || p.QueryParams[2] is long))
				throw new InvalidCastException();
			// четвертый параметр должен быть bool
			if (!(p.QueryParams[3] is bool))
				throw new InvalidCastException();
			// пятый параметр должен быть null или object[]
			if (p.QueryParams[4] != null && !(p.QueryParams[4] is object[]))
				throw new InvalidCastException();
			// шестой параметр должен быть bool
			if (!(p.QueryParams[5] is bool))
				throw new InvalidCastException();

			bool inverse = (bool)p.QueryParams[0];
			bool isByValue = (bool)p.QueryParams[3];
			bool isByText = (bool)p.QueryParams[5];

			if (isByText)
			{
				// Если массив null или пустой - то возвращаем null
				if (p.QueryParams[4] == null || ((object[])p.QueryParams[4]).Length == 0)
					return null;

				var fieldId = p.FieldID ?? "";
				string paramName = "@field" + fieldId.Replace("-", "_");

				var values = ((object[])p.QueryParams[4]).Select(n => Int32.Parse(n.ToString())).ToArray();

				if (values.Length == 1)
				{
					sqlParams.Add(new SqlParameter(paramName, values[0]));
					return String.Format((inverse ? "({1}.[{0}] <> {2} OR {1}.[{0}] IS NULL)" : "({1}.[{0}] = {2})"), p.FieldColumn.ToLower(), GetTableAlias(p), paramName);
				}
				else
				{
					sqlParams.Add(new SqlParameter(paramName, SqlDbType.Structured) { TypeName = "Ids", Value = Common.IdsToDataTable(values) });
					return String.Format((inverse ? "({1}.[{0}] NOT IN (select id from {2}) OR {1}.[{0}] IS NULL)" : "({1}.[{0}] IN (select id from {2}))"), p.FieldColumn.ToLower(), GetTableAlias(p), paramName);
				}
			}
			else
			{
				long? numberFrom = (p.QueryParams[1] is int || p.QueryParams[1] == null) ? (long?)(int?)p.QueryParams[1] : (long?)p.QueryParams[1];
				long? numberTo = (p.QueryParams[2] is int || p.QueryParams[2] == null) ? (long?)(int?)p.QueryParams[2] : (long?)p.QueryParams[2];

				if (isByValue)
				{
					if (!numberFrom.HasValue)
						return null;
					else
						return String.Format((inverse ? "({1}.[{0}] <> {2} OR {1}.[{0}] IS NULL)" : "({1}.[{0}] = {2})"), p.FieldColumn.ToLower(), GetTableAlias(p), numberFrom);
				}

				if (!numberFrom.HasValue && !numberTo.HasValue)
					return null;

				if (numberFrom.HasValue && !numberTo.HasValue)
					return String.Format(inverse ? "({1}.[{0}] < {2})" : "({1}.[{0}] >= {2})", p.FieldColumn.ToLower(), GetTableAlias(p), numberFrom);
				if (!numberFrom.HasValue && numberTo.HasValue)
					return String.Format(inverse ? "({1}.[{0}] > {2})" : "({1}.[{0}] <= {2})", p.FieldColumn.ToLower(), GetTableAlias(p), numberTo);

				if (numberFrom.Value < numberTo.Value)
					return String.Format("({1}.[{0}] {4} BETWEEN {2} AND {3})", p.FieldColumn.ToLower(), GetTableAlias(p), numberFrom, numberTo, inverse ? "NOT" : "");
				else if (numberFrom.Value > numberTo.Value)
					return String.Format("({1}.[{0}] {4} BETWEEN {2} AND {3})", p.FieldColumn.ToLower(), GetTableAlias(p), numberTo, numberFrom, inverse ? "NOT" : "");
				else
					return String.Format(inverse ? "({1}.[{0}] <> {2})" : "({1}.[{0}] = {2})", p.FieldColumn.ToLower(), GetTableAlias(p), numberFrom);
			}
		}

		/// <summary>
		/// Парсинг параметра поиска по тексту
		/// </summary>
		private static string ParseTextParam(ArticleSearchQueryParam p)
		{
			Contract.Requires(p != null);
			Contract.Requires(p.SearchType == ArticleFieldSearchType.Text);

			if (String.IsNullOrWhiteSpace(p.FieldColumn))
				throw new ArgumentException("FieldColumn is empty");
			// параметры не пустые и их не меньше 2х (используем 1, 2, опциоально - 3)
			if (p.QueryParams == null)
				throw new ArgumentException("QueryParams is null");
			if (p.QueryParams.Length < 2)
				throw new ArgumentException("QueryParams length < 2");
			// первый параметр должен быть bool
			if(!(p.QueryParams[0] is bool))
				throw new InvalidCastException("param 0");
			// второй параметр должен быть строкой или null
			if(p.QueryParams[1] != null && !(p.QueryParams[1] is string))
				throw new InvalidCastException("param 1");

			var isNull = (bool) p.QueryParams[0];
			var inverse = p.QueryParams.Length > 2 && (p.QueryParams[2] is bool) && (bool)p.QueryParams[2];

			// isnull == true
			if (isNull)
				return String.Format("({1}.[{0}] IS {2}NULL)", p.FieldColumn.ToLower(), GetTableAlias(p), (inverse) ? "NOT " : "");
			// isnull == false  строка пустая
			if (String.IsNullOrEmpty((string)p.QueryParams[1]))
				return null;

			// Иначе формируем результат
			var value = Cleaner.ToSafeSqlLikeCondition(((string)p.QueryParams[1]).Trim());
			return String.Format("({2}.[{0}] {3}LIKE '%{1}%')", p.FieldColumn.ToLower(), value, GetTableAlias(p), (inverse) ? "NOT " : "");
		}

		private static string ParseDateRangeParam(ArticleSearchQueryParam p)
		{
			Contract.Requires(p != null);
			Contract.Requires(p.SearchType == ArticleFieldSearchType.DateRange);

			if (String.IsNullOrWhiteSpace(p.FieldColumn))
				throw new ArgumentException("FieldColumn");
			// параметры не пустые и их не меньше 4х (используем 1й, 2й, 3й и 4й остальные отбрасываем)
			if (p.QueryParams == null || p.QueryParams.Length < 4)
				throw new ArgumentException();
			// первый параметр должен быть bool
			if (p.QueryParams[0] == null || !(p.QueryParams[0] is bool))
				throw new InvalidCastException();
			// второй параметр должен быть строкой или null
			if (p.QueryParams[1] != null && !(p.QueryParams[1] is string))
				throw new InvalidCastException();
			// третий параметр должен быть строкой или null
			if (p.QueryParams[2] != null && !(p.QueryParams[2] is string))
				throw new InvalidCastException();
			// четвертый параметр должен быть bool
			if (p.QueryParams[3] == null || !(p.QueryParams[3] is bool))
				throw new InvalidCastException();

			bool isNull = (bool)p.QueryParams[0];
			string dateFromString = (string)p.QueryParams[1];
			string dateToString = (string)p.QueryParams[2];
			bool isByValue = (bool)p.QueryParams[3];

			string sqlDateFromString = null;
			DateTime? dateFrom = null;
			string sqlDateToString = null;
			DateTime? dateTo = null;

			// isnull == true
			if (isNull)
				return String.Format("({1}.[{0}] IS NULL)", p.FieldColumn.ToLower(), GetTableAlias(p));

			if (isByValue)
			{
				if (String.IsNullOrWhiteSpace(dateFromString))
					return null;
				else
				{
					if (!Converter.TryConvertToSqlDateString(dateFromString, null, out sqlDateFromString, out dateFrom))
						throw new FormatException("date From");
					if (!Converter.TryConvertToSqlDateString(dateFromString, new TimeSpan(23, 59, 59), out sqlDateToString, out dateTo))
						throw new FormatException("date From");

					return String.Format("({1}.[{0}] BETWEEN '{2}' AND '{3}')", p.FieldColumn.ToLower(), GetTableAlias(p), sqlDateFromString, sqlDateToString);
				}
			}

			// если обе даты пустые - то возвращаем null
			if (String.IsNullOrWhiteSpace(dateFromString) && String.IsNullOrWhiteSpace(dateToString))
				return null;

			// дата "до" пустая а "от" не пустая
			if (!String.IsNullOrWhiteSpace(dateFromString) && String.IsNullOrWhiteSpace(dateToString))
			{
				if (!Converter.TryConvertToSqlDateString(dateFromString, null, out sqlDateFromString, out dateFrom))
					throw new FormatException("date From");
				return String.Format("({1}.[{0}] >= '{2}')", p.FieldColumn.ToLower(), GetTableAlias(p), sqlDateFromString);
			}

			// дата "от" пустая а "до" не пустая
			if (String.IsNullOrWhiteSpace(dateFromString) && !String.IsNullOrWhiteSpace(dateToString))
			{
				if (!Converter.TryConvertToSqlDateString(dateToString, new TimeSpan(23, 59, 59), out sqlDateToString, out dateTo))
					throw new FormatException("date To");
				return String.Format("({1}.[{0}] <= '{2}')", p.FieldColumn.ToLower(), GetTableAlias(p), sqlDateToString);
			}

			// обе границы диапазона не пустые
			if (!Converter.TryConvertToSqlDateString(dateFromString, null, out sqlDateFromString, out dateFrom))
				throw new FormatException("date From");
			if (!Converter.TryConvertToSqlDateString(dateToString, null, out sqlDateToString, out dateTo))
				throw new FormatException("date To");
			// From < To
			if (dateFrom.Value.Date < dateTo.Value.Date)
			{
				Converter.TryConvertToSqlDateString(dateFromString, null, out sqlDateFromString, out dateFrom);
				Converter.TryConvertToSqlDateString(dateToString, new TimeSpan(23, 59, 59), out sqlDateToString, out dateTo);

				return String.Format("({1}.[{0}] BETWEEN '{2}' AND '{3}')", p.FieldColumn.ToLower(), GetTableAlias(p), sqlDateFromString, sqlDateToString);
			}
			else
			{
				Converter.TryConvertToSqlDateString(dateFromString, new TimeSpan(23, 59, 59), out sqlDateFromString, out dateFrom);
				Converter.TryConvertToSqlDateString(dateToString, null, out sqlDateToString, out dateTo);

				return String.Format("({1}.[{0}] BETWEEN '{2}' AND '{3}')", p.FieldColumn.ToLower(), GetTableAlias(p), sqlDateToString, sqlDateFromString);
			}
		}

		private static string ParseTimeRangeParam(ArticleSearchQueryParam p)
		{
			Contract.Requires(p != null);
			Contract.Requires(p.SearchType == ArticleFieldSearchType.TimeRange);

			if (String.IsNullOrWhiteSpace(p.FieldColumn))
				throw new ArgumentException("FieldColumn");
			// параметры не пустые и их не меньше 3х (используем 1й, 2й, 3й и 4й- остальные отбрасываем)
			if (p.QueryParams == null || p.QueryParams.Length < 4)
				throw new ArgumentException();
			// первый параметр должен быть bool
			if (!(p.QueryParams[0] is bool))
				throw new InvalidCastException();
			// второй параметр должен быть строкой или null
			if (p.QueryParams[1] != null && !(p.QueryParams[1] is string))
				throw new InvalidCastException();
			// третий параметр должен быть строкой или null
			if (p.QueryParams[2] != null && !(p.QueryParams[2] is string))
				throw new InvalidCastException();
			// четвертый параметр должен быть bool
			if (!(p.QueryParams[3] is bool))
				throw new InvalidCastException();

			bool isNull = (bool)p.QueryParams[0];
			string timeFromString = (string)p.QueryParams[1];
			string timeToString = (string)p.QueryParams[2];
			bool isByValue = (bool)p.QueryParams[3];

			string sqlTimeFromString = null;
			TimeSpan? timeFrom = null;
		    TimeSpan? timeTo = null;

			// isnull == true
			if (isNull)
				return String.Format("({1}.[{0}] IS NULL)", p.FieldColumn.ToLower(), GetTableAlias(p));

			if (isByValue)
			{
				if (String.IsNullOrWhiteSpace(timeFromString))
					return null;
				else
				{
					if (!Converter.TryConvertToSqlTimeString(timeFromString, out sqlTimeFromString, out timeFrom))
						throw new FormatException("time From");
					return String.Format("([dbo].[qp_abs_time_seconds]({1}.[{0}]) = {2})", p.FieldColumn.ToLower(), GetTableAlias(p), timeFrom.Value.TotalSeconds);
				}
			}

			// если обе даты пустые - то возвращаем null
			if (String.IsNullOrWhiteSpace(timeFromString) && String.IsNullOrWhiteSpace(timeToString))
				return null;


			if (!String.IsNullOrWhiteSpace(timeFromString))
			{
				if (!Converter.TryConvertToSqlTimeString(timeFromString, out sqlTimeFromString, out timeFrom))
					throw new FormatException("time From");
			}
			if (!String.IsNullOrWhiteSpace(timeToString))
			{
			    string sqlTimeToString;
			    if (!Converter.TryConvertToSqlTimeString(timeToString, out sqlTimeToString, out timeTo))
					throw new FormatException("time To");
			}

		    // дата "до" пустая а "от" не пустая
			if (!String.IsNullOrWhiteSpace(timeFromString) && String.IsNullOrWhiteSpace(timeToString))
				return String.Format("([dbo].[qp_abs_time_seconds]({1}.[{0}]) >= {2})", p.FieldColumn.ToLower(), GetTableAlias(p), timeFrom.Value.TotalSeconds);

			// дата "от" пустая а "до" не пустая
			if (String.IsNullOrWhiteSpace(timeFromString) && !String.IsNullOrWhiteSpace(timeToString))
				return String.Format("([dbo].[qp_abs_time_seconds]({1}.[{0}]) <= {2})", p.FieldColumn.ToLower(), GetTableAlias(p), timeTo.Value.TotalSeconds);

			// обе границы диапазона не пустые
			// From < To
			if (timeFrom.Value < timeTo.Value)
				return String.Format("([dbo].[qp_abs_time_seconds]({1}.[{0}]) BETWEEN {2} AND {3})", p.FieldColumn.ToLower(), GetTableAlias(p), timeFrom.Value.TotalSeconds, timeTo.Value.TotalSeconds);
			else if (timeFrom.Value > timeTo.Value)
				return String.Format("([dbo].[qp_abs_time_seconds]({1}.[{0}]) BETWEEN {2} AND {3})", p.FieldColumn.ToLower(), GetTableAlias(p), timeTo.Value.TotalSeconds, timeFrom.Value.TotalSeconds);
			else
				return String.Format("([dbo].[qp_abs_time_seconds]({1}.[{0}]) = {2})", p.FieldColumn.ToLower(), GetTableAlias(p), timeFrom.Value.TotalSeconds);

		}

		private static string ParseNumericRangeParam(ArticleSearchQueryParam p)
		{
			Contract.Requires(p != null);
			Contract.Requires(p.SearchType == ArticleFieldSearchType.NumericRange);

			if (String.IsNullOrWhiteSpace(p.FieldColumn))
				throw new ArgumentException("FieldColumn");
			// параметры не пустые и их не меньше 4х (используем 1й, 2й, 3й и 4й, 5-й опционально)
			if (p.QueryParams == null)
				throw new ArgumentException("QueryParams is null");
			if (p.QueryParams.Length < 4)
				throw new ArgumentException("QueryParams length < 4");
			// первый параметр должен быть bool
			if (p.QueryParams[0] == null || !(p.QueryParams[0] is bool))
				throw new InvalidCastException("param 0");
			// второй параметр должен быть int или long или null
			if (p.QueryParams[1] != null && !(p.QueryParams[1] is int || p.QueryParams[1] is long))
				throw new InvalidCastException("param 1");
			// третий параметр должен быть int или long или null
			if (p.QueryParams[2] != null && !(p.QueryParams[2] is int || p.QueryParams[2] is long))
				throw new InvalidCastException("param 2");
			// четвертый параметр должен быть bool
			if (p.QueryParams[3] == null || !(p.QueryParams[3] is bool))
				throw new InvalidCastException("param 3");

			bool isNull = (bool)p.QueryParams[0];
			bool isByValue = (bool)p.QueryParams[3];
			var inverse = p.QueryParams.Length > 4 && (p.QueryParams[4] is bool) && (bool)p.QueryParams[4];

			if (isNull)
				return String.Format("({1}.[{0}] IS {2}NULL)", p.FieldColumn.ToLower(), GetTableAlias(p), (inverse) ? "NOT " : "");

			long? numberFrom = (p.QueryParams[1] is int || p.QueryParams[1] == null) ? (long?)(int?)p.QueryParams[1] : (long?)p.QueryParams[1];
			long? numberTo = (p.QueryParams[2] is int || p.QueryParams[2] == null) ? (long?)(int?)p.QueryParams[2] : (long?)p.QueryParams[2];

			if (isByValue)
			{
				return !numberFrom.HasValue ? null : String.Format("({1}.[{0}] {3} {2})", p.FieldColumn.ToLower(), GetTableAlias(p), numberFrom, (inverse) ? "<>" : "=");
			}

			if (!numberFrom.HasValue && !numberTo.HasValue)
				return null;

			if (numberFrom.HasValue && !numberTo.HasValue)
				return String.Format("({1}.[{0}] {3} {2})", p.FieldColumn.ToLower(), GetTableAlias(p), numberFrom, (inverse) ? "<" : ">=");
			if (!numberFrom.HasValue && numberTo.HasValue)
				return String.Format("({1}.[{0}] {3} {2})", p.FieldColumn.ToLower(), GetTableAlias(p), numberTo, (inverse) ? ">" : "<=");

			if (numberFrom.Value < numberTo.Value)
				return String.Format("({1}.[{0}] {4}BETWEEN {2} AND {3})", p.FieldColumn.ToLower(), GetTableAlias(p), numberFrom, numberTo, (inverse) ? "NOT " : "");
			else if (numberFrom.Value > numberTo.Value)
				return String.Format("({1}.[{0}] {4}BETWEEN {2} AND {3})", p.FieldColumn.ToLower(), GetTableAlias(p), numberTo, numberFrom, (inverse) ? "NOT " : "");
			else
				return String.Format("({1}.[{0}] {3} {2})", p.FieldColumn.ToLower(), GetTableAlias(p), numberFrom, (inverse) ? "<>" : "=");

		}

		private static string ParseO2MRelationParam(ArticleSearchQueryParam p, ICollection<SqlParameter> sqlParams)
		{
			Contract.Requires(p != null);
			Contract.Requires(p.SearchType == ArticleFieldSearchType.O2MRelation || p.SearchType == ArticleFieldSearchType.Classifier);

			if (String.IsNullOrWhiteSpace(p.FieldColumn))
				throw new ArgumentException("FieldColumn");

			// параметры не пустые и их не меньше 2х (используем 1й и 2й - остальные отбрасываем)
			if (p.QueryParams == null || p.QueryParams.Length < 3)
				throw new ArgumentException();

			// первый параметр должен быть null или object[]
			if (p.QueryParams[0] != null && !(p.QueryParams[0] is object[]))
				throw new InvalidCastException();

			// второй параметр должен быть bool
			if (!(p.QueryParams[1] is bool))
				throw new InvalidCastException();

			if (!(p.QueryParams[2] is bool))
				throw new InvalidCastException();

			bool isNull = (bool)p.QueryParams[1];
			bool inverse = (bool)p.QueryParams[2];

			string inverseString = (inverse) ? "NOT" : "";

			if (isNull)
				return String.Format("({1}.[{0}] IS {2} NULL)", p.FieldColumn.ToLower(), GetTableAlias(p), inverseString);

			// Если массив null или пустой - то возвращаем null
			if (p.QueryParams[0] == null || ((object[])p.QueryParams[0]).Length == 0)
				return null;

			var values = ((object[])p.QueryParams[0]).Select(n => Int32.Parse(n.ToString())).ToArray();
			var fieldId = p.FieldID ?? "";
			string paramName = "@field" + fieldId.Replace("-", "_");

			if (values.Length == 1)
			{
				sqlParams.Add(new SqlParameter(paramName, values[0]));
				return String.Format((inverse ? "({1}.[{0}] <> {2} OR {1}.[{0}] IS NULL)" : "({1}.[{0}] = {2})"), p.FieldColumn.ToLower(), GetTableAlias(p), paramName);
			}
			else
			{
				sqlParams.Add(new SqlParameter(paramName, SqlDbType.Structured) { TypeName = "Ids", Value = Common.IdsToDataTable(values) });
				return String.Format((inverse ? "({1}.[{0}] NOT IN (select id from {2}) OR {1}.[{0}] IS NULL)" : "({1}.[{0}] IN (select id from {2}))"), p.FieldColumn.ToLower(), GetTableAlias(p), paramName);
			}
		}

		private static string ParseBooleanParam(ArticleSearchQueryParam p)
		{
			Contract.Requires(p != null);
			Contract.Requires(p.SearchType == ArticleFieldSearchType.Boolean);

			if (String.IsNullOrWhiteSpace(p.FieldColumn))
				throw new ArgumentException("FieldColumn");
			// параметры не пустые и их не меньше 2х (используем 1й, 2й - остальные отбрасываем)
			if (p.QueryParams == null || p.QueryParams.Length < 2)
				throw new ArgumentException();
			// первый параметр должен быть bool
			if (!(p.QueryParams[0] is bool))
				throw new InvalidCastException();
			// второй параметр должен быть bool
			if (!(p.QueryParams[1] is bool))
				throw new InvalidCastException();

			bool isNull = (bool)p.QueryParams[0];

			if (isNull)
				return String.Format("({1}.[{0}] IS NULL)", p.FieldColumn.ToLower(), GetTableAlias(p));


			bool value = (bool)p.QueryParams[1];
			return String.Format("({1}.[{0}] = {2})", p.FieldColumn.ToLower(), GetTableAlias(p), (value ? 1 : 0));

		}

		private static string GetTableAlias(ArticleSearchQueryParam p)
		{
			if (string.IsNullOrEmpty(p.ContentID))
			{
				return CONTENT_TABLE_ALIAS;
			}
			else if (string.IsNullOrEmpty(p.ReferenceFieldID))
			{
				return CONTENT_TABLE_ALIAS + "_" + p.ContentID;
			}
			else
			{
				return CONTENT_TABLE_ALIAS + "_" + p.ContentID + "_" + p.ReferenceFieldID;
			}
		}
	}
}
