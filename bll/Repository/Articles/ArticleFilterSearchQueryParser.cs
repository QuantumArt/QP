using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics.Contracts;
using System.Linq;
using Quantumart.QP8.DAL;
using Quantumart.QP8.Utils;

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
        private const string ContentTableAlias = "c";

        /// <summary>
        /// Возвращает значение параметра filter
        /// </summary>
        public string GetFilter(IEnumerable<ArticleSearchQueryParam> searchQueryParams, IList<SqlParameter> sqlParams)
        {
            if (searchQueryParams == null)
            {
                return null;
            }

            if (!searchQueryParams.Any())
            {
                return null;
            }

            // оставляем параметры только тех типов которые обрабатываються данным методом
            var processedSqlParams = searchQueryParams.Where(p => new[]
            {
                ArticleFieldSearchType.Identifier,
                ArticleFieldSearchType.Text,
                ArticleFieldSearchType.Boolean,
                ArticleFieldSearchType.DateRange,
                ArticleFieldSearchType.TimeRange,
                ArticleFieldSearchType.DateTimeRange,
                ArticleFieldSearchType.NumericRange,
                ArticleFieldSearchType.O2MRelation,
                ArticleFieldSearchType.Classifier,
                ArticleFieldSearchType.StringEnum
            }.Contains(p.SearchType));

            // если нет обрабатываемых параметров - то возвращаем null
            if (!processedSqlParams.Any())
            {
                return null;
            }

            // формируем результат
            var result = SqlFilterComposer.Compose(processedSqlParams.Select(p =>
            {
                switch (p.SearchType)
                {
                    case ArticleFieldSearchType.Identifier:
                        return ParseIdentifierParam(p, sqlParams);
                    case ArticleFieldSearchType.Text:
                    case ArticleFieldSearchType.StringEnum:
                        return ParseTextParam(p);
                    case ArticleFieldSearchType.DateRange:
                        return ParseDateRangeParam(p);
                    case ArticleFieldSearchType.DateTimeRange:
                        return ParseDateTimeRangeParam(p);
                    case ArticleFieldSearchType.TimeRange:
                        return ParseTimeRangeParam(p);
                    case ArticleFieldSearchType.NumericRange:
                        return ParseNumericRangeParam(p);
                    case ArticleFieldSearchType.O2MRelation:
                    case ArticleFieldSearchType.Classifier:
                        return ParseO2MRelationParam(p, sqlParams);
                    case ArticleFieldSearchType.Boolean:
                        return ParseBooleanParam(p);
                }

                return null;
            }));

            // если результат - пустая строка - то возвращаем null
            return string.IsNullOrWhiteSpace(result) ? null : result;
        }

        private static string ParseIdentifierParam(ArticleSearchQueryParam p, ICollection<SqlParameter> sqlParams)
        {
            Contract.Requires(p != null);
            Contract.Requires(p.SearchType == ArticleFieldSearchType.NumericRange);

            if (string.IsNullOrWhiteSpace(p.FieldColumn))
            {
                throw new ArgumentException("FieldColumn");
            }

            // параметры не пустые и их не меньше 4х (используем 1й, 2й, 3й и 4й - остальные отбрасываем)
            if (p.QueryParams == null || p.QueryParams.Length < 4)
            {
                throw new ArgumentException();
            }

            // первый параметр должен быть bool
            if (!(p.QueryParams[0] is bool))
            {
                throw new InvalidCastException();
            }

            // второй параметр должен быть int или long или null
            if (p.QueryParams[1] != null && !(p.QueryParams[1] is int || p.QueryParams[1] is long))
            {
                throw new InvalidCastException();
            }

            // третий параметр должен быть int или long или null
            if (p.QueryParams[2] != null && !(p.QueryParams[2] is int || p.QueryParams[2] is long))
            {
                throw new InvalidCastException();
            }

            // четвертый параметр должен быть bool
            if (!(p.QueryParams[3] is bool))
            {
                throw new InvalidCastException();
            }

            // пятый параметр должен быть null или object[]
            if (p.QueryParams[4] != null && !(p.QueryParams[4] is object[]))
            {
                throw new InvalidCastException();
            }

            // шестой параметр должен быть bool
            if (!(p.QueryParams[5] is bool))
            {
                throw new InvalidCastException();
            }

            var inverse = (bool)p.QueryParams[0];
            var isByValue = (bool)p.QueryParams[3];
            var isByText = (bool)p.QueryParams[5];

            if (isByText)
            {
                // Если массив null или пустой - то возвращаем null
                if (p.QueryParams[4] == null || ((object[])p.QueryParams[4]).Length == 0)
                {
                    return null;
                }

                var fieldId = p.FieldID ?? string.Empty;
                var paramName = "@field" + fieldId.Replace("-", "_");

                var values = ((object[])p.QueryParams[4]).Select(n => int.Parse(n.ToString())).ToArray();

                if (values.Length == 1)
                {
                    sqlParams.Add(new SqlParameter(paramName, values[0]));
                    return string.Format(inverse ? "({1}.[{0}] <> {2} OR {1}.[{0}] IS NULL)" : "({1}.[{0}] = {2})", p.FieldColumn.ToLower(), GetTableAlias(p), paramName);
                }

                sqlParams.Add(new SqlParameter(paramName, SqlDbType.Structured) { TypeName = "Ids", Value = Common.IdsToDataTable(values) });
                return string.Format(inverse ? "({1}.[{0}] NOT IN (select id from {2}) OR {1}.[{0}] IS NULL)" : "({1}.[{0}] IN (select id from {2}))", p.FieldColumn.ToLower(), GetTableAlias(p), paramName);
            }

            var numberFrom = p.QueryParams[1] is int || p.QueryParams[1] == null ? (int?)p.QueryParams[1] : (long?)p.QueryParams[1];
            var numberTo = p.QueryParams[2] is int || p.QueryParams[2] == null ? (int?)p.QueryParams[2] : (long?)p.QueryParams[2];

            if (isByValue)
            {
                return !numberFrom.HasValue ? null : string.Format(inverse ? "({1}.[{0}] <> {2} OR {1}.[{0}] IS NULL)" : "({1}.[{0}] = {2})", p.FieldColumn.ToLower(), GetTableAlias(p), numberFrom);
            }

            if (!numberFrom.HasValue && !numberTo.HasValue)
            {
                return null;
            }

            if (numberFrom.HasValue && !numberTo.HasValue)
            {
                return string.Format(inverse ? "({1}.[{0}] < {2})" : "({1}.[{0}] >= {2})", p.FieldColumn.ToLower(), GetTableAlias(p), numberFrom);
            }
            if (!numberFrom.HasValue)
            {
                return string.Format(inverse ? "({1}.[{0}] > {2})" : "({1}.[{0}] <= {2})", p.FieldColumn.ToLower(), GetTableAlias(p), numberTo);
            }

            if (numberFrom.Value < numberTo.Value)
            {
                return string.Format("({1}.[{0}] {4} BETWEEN {2} AND {3})", p.FieldColumn.ToLower(), GetTableAlias(p), numberFrom, numberTo, inverse ? "NOT" : "");
            }

            if (numberFrom.Value > numberTo.Value)
            {
                return string.Format("({1}.[{0}] {4} BETWEEN {2} AND {3})", p.FieldColumn.ToLower(), GetTableAlias(p), numberTo, numberFrom, inverse ? "NOT" : "");
            }

            return string.Format(inverse ? "({1}.[{0}] <> {2})" : "({1}.[{0}] = {2})", p.FieldColumn.ToLower(), GetTableAlias(p), numberFrom);
        }

        /// <summary>
        /// Парсинг параметра поиска по тексту
        /// </summary>
        private static string ParseTextParam(ArticleSearchQueryParam p)
        {
            Contract.Requires(p != null);
            Contract.Requires(p.SearchType == ArticleFieldSearchType.Text);

            if (string.IsNullOrWhiteSpace(p.FieldColumn))
            {
                throw new ArgumentException("FieldColumn is empty");
            }

            // параметры не пустые и их не меньше 2х (используем 1, 2, опциоально - 3)
            if (p.QueryParams == null)
            {
                throw new ArgumentException("QueryParams is null");
            }
            if (p.QueryParams.Length < 2)
            {
                throw new ArgumentException("QueryParams length < 2");
            }

            // первый параметр должен быть bool
            if (!(p.QueryParams[0] is bool))
            {
                throw new InvalidCastException("param 0");
            }

            // второй параметр должен быть строкой или null
            if (p.QueryParams[1] != null && !(p.QueryParams[1] is string))
            {
                throw new InvalidCastException("param 1");
            }

            var isNull = (bool)p.QueryParams[0];
            var inverse = p.QueryParams.Length > 2 && p.QueryParams[2] is bool && (bool)p.QueryParams[2];

            var exactMatch = p.QueryParams.Length > 3 && p.QueryParams[3] is bool && (bool)p.QueryParams[3];
            var startFromBegin = p.QueryParams.Length > 4 && p.QueryParams[4] is bool && (bool)p.QueryParams[4];

            // isnull == true
            if (isNull)
            {
                return string.Format("({1}.[{0}] IS {2}NULL)", p.FieldColumn.ToLower(), GetTableAlias(p), inverse ? "NOT " : "");
            }

            // isnull == false  строка пустая
            if (string.IsNullOrEmpty((string)p.QueryParams[1]))
            {
                return null;
            }

            // Иначе формируем результат
            var value = Cleaner.ToSafeSqlLikeCondition(((string)p.QueryParams[1]).Trim());
            if (exactMatch)
            {
                return string.Format("({2}.[{0}] {3} '{1}')", p.FieldColumn.ToLower(), value, GetTableAlias(p), inverse ? "<> " : "=");
            }

            if (startFromBegin)
            {
                return string.Format("({1}.[{0}] LIKE '{2}')", p.FieldColumn.ToLower(), GetTableAlias(p), inverse ? "%" + value : value + "%");
            }

            return string.Format("({2}.[{0}] {3}LIKE '%{1}%')", p.FieldColumn.ToLower(), value, GetTableAlias(p), inverse ? "NOT " : "");
        }

        private static string ParseDateRangeParam(ArticleSearchQueryParam p)
        {
            Contract.Requires(p != null);
            Contract.Requires(p.SearchType == ArticleFieldSearchType.DateRange);

            if (string.IsNullOrWhiteSpace(p.FieldColumn))
            {
                throw new ArgumentException("FieldColumn");
            }

            // параметры не пустые и их не меньше 4х (используем 1й, 2й, 3й и 4й остальные отбрасываем)
            if (p.QueryParams == null || p.QueryParams.Length < 4)
            {
                throw new ArgumentException();
            }

            // первый параметр должен быть bool
            if (!(p.QueryParams[0] is bool))
            {
                throw new InvalidCastException();
            }

            // второй параметр должен быть строкой или null
            if (p.QueryParams[1] != null && !(p.QueryParams[1] is string))
            {
                throw new InvalidCastException();
            }

            // третий параметр должен быть строкой или null
            if (p.QueryParams[2] != null && !(p.QueryParams[2] is string))
            {
                throw new InvalidCastException();
            }

            // четвертый параметр должен быть bool
            if (!(p.QueryParams[3] is bool))
            {
                throw new InvalidCastException();
            }

            var isNull = (bool)p.QueryParams[0];
            var dateFromString = (string)p.QueryParams[1];
            var dateToString = (string)p.QueryParams[2];
            var isByValue = (bool)p.QueryParams[3];

            string sqlDateFromString;
            DateTime? dateFrom;
            string sqlDateToString;
            DateTime? dateTo;

            if (isNull)
            {
                return string.Format("({1}.[{0}] IS NULL)", p.FieldColumn.ToLower(), GetTableAlias(p));
            }

            if (isByValue)
            {
                if (string.IsNullOrWhiteSpace(dateFromString))
                {
                    return null;
                }

                if (!Converter.TryConvertToSqlDateString(dateFromString, null, out sqlDateFromString, out dateFrom))
                {
                    throw new FormatException("date From");
                }
                if (!Converter.TryConvertToSqlDateString(dateFromString, new TimeSpan(23, 59, 59), out sqlDateToString, out dateTo))
                {
                    throw new FormatException("date From");
                }

                return string.Format("({1}.[{0}] BETWEEN '{2}' AND '{3}')", p.FieldColumn.ToLower(), GetTableAlias(p), sqlDateFromString, sqlDateToString);
            }

            // если обе даты пустые - то возвращаем null
            if (string.IsNullOrWhiteSpace(dateFromString) && string.IsNullOrWhiteSpace(dateToString))
            {
                return null;
            }

            // дата "до" пустая а "от" не пустая
            if (!string.IsNullOrWhiteSpace(dateFromString) && string.IsNullOrWhiteSpace(dateToString))
            {
                if (!Converter.TryConvertToSqlDateString(dateFromString, null, out sqlDateFromString, out dateFrom))
                {
                    throw new FormatException("date From");
                }

                return string.Format("({1}.[{0}] >= '{2}')", p.FieldColumn.ToLower(), GetTableAlias(p), sqlDateFromString);
            }

            // дата "от" пустая а "до" не пустая
            if (string.IsNullOrWhiteSpace(dateFromString) && !string.IsNullOrWhiteSpace(dateToString))
            {
                if (!Converter.TryConvertToSqlDateString(dateToString, new TimeSpan(23, 59, 59), out sqlDateToString, out dateTo))
                {
                    throw new FormatException("date To");
                }

                return string.Format("({1}.[{0}] <= '{2}')", p.FieldColumn.ToLower(), GetTableAlias(p), sqlDateToString);
            }

            // обе границы диапазона не пустые
            if (!Converter.TryConvertToSqlDateString(dateFromString, null, out sqlDateFromString, out dateFrom))
            {
                throw new FormatException("date From");
            }
            if (!Converter.TryConvertToSqlDateString(dateToString, null, out sqlDateToString, out dateTo))
            {
                throw new FormatException("date To");
            }

            // From < To
            if (dateFrom.Value.Date < dateTo.Value.Date)
            {
                Converter.TryConvertToSqlDateString(dateFromString, null, out sqlDateFromString, out dateFrom);
                Converter.TryConvertToSqlDateString(dateToString, new TimeSpan(23, 59, 59), out sqlDateToString, out dateTo);

                return string.Format("({1}.[{0}] BETWEEN '{2}' AND '{3}')", p.FieldColumn.ToLower(), GetTableAlias(p), sqlDateFromString, sqlDateToString);
            }

            Converter.TryConvertToSqlDateString(dateFromString, new TimeSpan(23, 59, 59), out sqlDateFromString, out dateFrom);
            Converter.TryConvertToSqlDateString(dateToString, null, out sqlDateToString, out dateTo);

            return string.Format("({1}.[{0}] BETWEEN '{2}' AND '{3}')", p.FieldColumn.ToLower(), GetTableAlias(p), sqlDateToString, sqlDateFromString);
        }

        private static string ParseDateTimeRangeParam(ArticleSearchQueryParam p)
        {
            Contract.Requires(p != null);
            Contract.Requires(p.SearchType == ArticleFieldSearchType.DateTimeRange);

            if (string.IsNullOrWhiteSpace(p.FieldColumn))
            {
                throw new ArgumentException("FieldColumn");
            }

            // параметры не пустые и их не меньше 4х (используем 1й, 2й, 3й и 4й остальные отбрасываем)
            if (p.QueryParams == null || p.QueryParams.Length < 4)
            {
                throw new ArgumentException();
            }

            // первый параметр должен быть bool
            if (!(p.QueryParams[0] is bool))
            {
                throw new InvalidCastException();
            }

            // второй параметр должен быть строкой или null
            if (p.QueryParams[1] != null && !(p.QueryParams[1] is string))
            {
                throw new InvalidCastException();
            }

            // третий параметр должен быть строкой или null
            if (p.QueryParams[2] != null && !(p.QueryParams[2] is string))
            {
                throw new InvalidCastException();
            }

            // четвертый параметр должен быть bool
            if (!(p.QueryParams[3] is bool))
            {
                throw new InvalidCastException();
            }

            var isNull = (bool)p.QueryParams[0];
            var datetimeFromString = (string)p.QueryParams[1];
            var datetimeToString = (string)p.QueryParams[2];
            var isByValue = (bool)p.QueryParams[3];

            string sqlDateTimeFromString;
            DateTime? datetimeFrom;
            string sqlDateTimeToString;
            DateTime? datetimeTo;

            if (isNull)
            {
                return string.Format("({1}.[{0}] IS NULL)", p.FieldColumn.ToLower(), GetTableAlias(p));
            }

            if (isByValue)
            {
                if (string.IsNullOrWhiteSpace(datetimeFromString))
                {
                    return null;
                }

                if (!Converter.TryConvertToSqlDateString(datetimeFromString, null, out sqlDateTimeFromString, out datetimeFrom))
                {
                    throw new FormatException("datetime From");
                }

                return string.Format("({1}.[{0}] = '{2}')", p.FieldColumn.ToLower(), GetTableAlias(p), sqlDateTimeFromString);
            }

            // если обе даты пустые - то возвращаем null
            if (string.IsNullOrWhiteSpace(datetimeFromString) && string.IsNullOrWhiteSpace(datetimeToString))
            {
                return null;
            }

            // дата "до" пустая а "от" не пустая
            if (!string.IsNullOrWhiteSpace(datetimeFromString) && string.IsNullOrWhiteSpace(datetimeToString))
            {
                if (!Converter.TryConvertToSqlDateString(datetimeFromString, null, out sqlDateTimeFromString, out datetimeFrom))
                {
                    throw new FormatException("datetime From");
                }

                return string.Format("({1}.[{0}] >= '{2}')", p.FieldColumn.ToLower(), GetTableAlias(p), sqlDateTimeFromString);
            }

            // дата "от" пустая а "до" не пустая
            if (string.IsNullOrWhiteSpace(datetimeFromString) && !string.IsNullOrWhiteSpace(datetimeToString))
            {
                if (!Converter.TryConvertToSqlDateString(datetimeToString, new TimeSpan(23, 59, 59), out sqlDateTimeToString, out datetimeTo))
                {
                    throw new FormatException("datetime To");
                }

                return string.Format("({1}.[{0}] <= '{2}')", p.FieldColumn.ToLower(), GetTableAlias(p), sqlDateTimeToString);
            }

            // обе границы диапазона не пустые
            if (!Converter.TryConvertToSqlDateString(datetimeFromString, null, out sqlDateTimeFromString, out datetimeFrom))
            {
                throw new FormatException("datetime From");
            }
            if (!Converter.TryConvertToSqlDateString(datetimeToString, null, out sqlDateTimeToString, out datetimeTo))
            {
                throw new FormatException("datetime To");
            }

            // From < To
            if (datetimeFrom.Value < datetimeTo.Value)
            {
                return string.Format("({1}.[{0}] BETWEEN '{2}' AND '{3}')", p.FieldColumn.ToLower(), GetTableAlias(p), sqlDateTimeFromString, sqlDateTimeToString);
            }

            return string.Format("({1}.[{0}] BETWEEN '{2}' AND '{3}')", p.FieldColumn.ToLower(), GetTableAlias(p), sqlDateTimeToString, sqlDateTimeFromString);
        }

        private static string ParseTimeRangeParam(ArticleSearchQueryParam p)
        {
            Contract.Requires(p != null);
            Contract.Requires(p.SearchType == ArticleFieldSearchType.TimeRange);

            if (string.IsNullOrWhiteSpace(p.FieldColumn))
            {
                throw new ArgumentException("FieldColumn");
            }

            // параметры не пустые и их не меньше 3х (используем 1й, 2й, 3й и 4й- остальные отбрасываем)
            if (p.QueryParams == null || p.QueryParams.Length < 4)
            {
                throw new ArgumentException();
            }

            // первый параметр должен быть bool
            if (!(p.QueryParams[0] is bool))
            {
                throw new InvalidCastException();
            }

            // второй параметр должен быть строкой или null
            if (p.QueryParams[1] != null && !(p.QueryParams[1] is string))
            {
                throw new InvalidCastException();
            }

            // третий параметр должен быть строкой или null
            if (p.QueryParams[2] != null && !(p.QueryParams[2] is string))
            {
                throw new InvalidCastException();
            }

            // четвертый параметр должен быть bool
            if (!(p.QueryParams[3] is bool))
            {
                throw new InvalidCastException();
            }

            var isNull = (bool)p.QueryParams[0];
            var timeFromString = (string)p.QueryParams[1];
            var timeToString = (string)p.QueryParams[2];
            var isByValue = (bool)p.QueryParams[3];

            string sqlTimeFromString;
            TimeSpan? timeFrom = null;
            TimeSpan? timeTo = null;

            // isnull == true
            if (isNull)
            {
                return string.Format("({1}.[{0}] IS NULL)", p.FieldColumn.ToLower(), GetTableAlias(p));
            }

            if (isByValue)
            {
                if (string.IsNullOrWhiteSpace(timeFromString))
                {
                    return null;
                }

                if (!Converter.TryConvertToSqlTimeString(timeFromString, out sqlTimeFromString, out timeFrom))
                {
                    throw new FormatException("time From");
                }

                return string.Format("([dbo].[qp_abs_time_seconds]({1}.[{0}]) = {2})", p.FieldColumn.ToLower(), GetTableAlias(p), timeFrom.Value.TotalSeconds);
            }

            // если обе даты пустые - то возвращаем null
            if (string.IsNullOrWhiteSpace(timeFromString) && string.IsNullOrWhiteSpace(timeToString))
            {
                return null;
            }

            if (!string.IsNullOrWhiteSpace(timeFromString))
            {
                if (!Converter.TryConvertToSqlTimeString(timeFromString, out sqlTimeFromString, out timeFrom))
                {
                    throw new FormatException("time From");
                }
            }
            if (!string.IsNullOrWhiteSpace(timeToString))
            {
                string sqlTimeToString;
                if (!Converter.TryConvertToSqlTimeString(timeToString, out sqlTimeToString, out timeTo))
                {
                    throw new FormatException("time To");
                }
            }

            // дата "до" пустая а "от" не пустая
            if (!string.IsNullOrWhiteSpace(timeFromString) && string.IsNullOrWhiteSpace(timeToString))
            {
                return string.Format("([dbo].[qp_abs_time_seconds]({1}.[{0}]) >= {2})", p.FieldColumn.ToLower(), GetTableAlias(p), timeFrom.Value.TotalSeconds);
            }

            // дата "от" пустая а "до" не пустая
            if (string.IsNullOrWhiteSpace(timeFromString) && !string.IsNullOrWhiteSpace(timeToString))
            {
                return string.Format("([dbo].[qp_abs_time_seconds]({1}.[{0}]) <= {2})", p.FieldColumn.ToLower(), GetTableAlias(p), timeTo.Value.TotalSeconds);
            }

            // обе границы диапазона не пустые
            // From < To
            if (timeFrom.Value < timeTo.Value)
            {
                return string.Format("([dbo].[qp_abs_time_seconds]({1}.[{0}]) BETWEEN {2} AND {3})", p.FieldColumn.ToLower(), GetTableAlias(p), timeFrom.Value.TotalSeconds, timeTo.Value.TotalSeconds);
            }

            if (timeFrom.Value > timeTo.Value)
            {
                return string.Format("([dbo].[qp_abs_time_seconds]({1}.[{0}]) BETWEEN {2} AND {3})", p.FieldColumn.ToLower(), GetTableAlias(p), timeTo.Value.TotalSeconds, timeFrom.Value.TotalSeconds);
            }

            return string.Format("([dbo].[qp_abs_time_seconds]({1}.[{0}]) = {2})", p.FieldColumn.ToLower(), GetTableAlias(p), timeFrom.Value.TotalSeconds);
        }

        private static string ParseNumericRangeParam(ArticleSearchQueryParam p)
        {
            Contract.Requires(p != null);
            Contract.Requires(p.SearchType == ArticleFieldSearchType.NumericRange);

            if (string.IsNullOrWhiteSpace(p.FieldColumn))
            {
                throw new ArgumentException("FieldColumn");
            }

            // параметры не пустые и их не меньше 4х (используем 1й, 2й, 3й и 4й, 5-й опционально)
            if (p.QueryParams == null)
            {
                throw new ArgumentException("QueryParams is null");
            }
            if (p.QueryParams.Length < 4)
            {
                throw new ArgumentException("QueryParams length < 4");
            }

            // первый параметр должен быть bool
            if (!(p.QueryParams[0] is bool))
            {
                throw new InvalidCastException("param 0");
            }

            // второй параметр должен быть int или long или null
            if (p.QueryParams[1] != null && !(p.QueryParams[1] is int || p.QueryParams[1] is long))
            {
                throw new InvalidCastException("param 1");
            }

            // третий параметр должен быть int или long или null
            if (p.QueryParams[2] != null && !(p.QueryParams[2] is int || p.QueryParams[2] is long))
            {
                throw new InvalidCastException("param 2");
            }

            // четвертый параметр должен быть bool
            if (!(p.QueryParams[3] is bool))
            {
                throw new InvalidCastException("param 3");
            }

            var isNull = (bool)p.QueryParams[0];
            var isByValue = (bool)p.QueryParams[3];
            var inverse = p.QueryParams.Length > 4 && p.QueryParams[4] is bool && (bool)p.QueryParams[4];

            if (isNull)
            {
                return string.Format("({1}.[{0}] IS {2}NULL)", p.FieldColumn.ToLower(), GetTableAlias(p), inverse ? "NOT " : "");
            }

            var numberFrom = p.QueryParams[1] is int || p.QueryParams[1] == null ? (int?)p.QueryParams[1] : (long?)p.QueryParams[1];
            var numberTo = p.QueryParams[2] is int || p.QueryParams[2] == null ? (int?)p.QueryParams[2] : (long?)p.QueryParams[2];

            if (isByValue)
            {
                return !numberFrom.HasValue ? null : string.Format("({1}.[{0}] {3} {2})", p.FieldColumn.ToLower(), GetTableAlias(p), numberFrom, inverse ? "<>" : "=");
            }

            if (!numberFrom.HasValue && !numberTo.HasValue)
            {
                return null;
            }

            if (numberFrom.HasValue && !numberTo.HasValue)
            {
                return string.Format("({1}.[{0}] {3} {2})", p.FieldColumn.ToLower(), GetTableAlias(p), numberFrom, inverse ? "<" : ">=");
            }
            if (!numberFrom.HasValue)
            {
                return string.Format("({1}.[{0}] {3} {2})", p.FieldColumn.ToLower(), GetTableAlias(p), numberTo, inverse ? ">" : "<=");
            }

            if (numberFrom.Value < numberTo.Value)
            {
                return string.Format("({1}.[{0}] {4}BETWEEN {2} AND {3})", p.FieldColumn.ToLower(), GetTableAlias(p), numberFrom, numberTo, inverse ? "NOT " : "");
            }

            if (numberFrom.Value > numberTo.Value)
            {
                return string.Format("({1}.[{0}] {4}BETWEEN {2} AND {3})", p.FieldColumn.ToLower(), GetTableAlias(p), numberTo, numberFrom, inverse ? "NOT " : "");
            }

            return string.Format("({1}.[{0}] {3} {2})", p.FieldColumn.ToLower(), GetTableAlias(p), numberFrom, inverse ? "<>" : "=");
        }

        private static string ParseO2MRelationParam(ArticleSearchQueryParam p, ICollection<SqlParameter> sqlParams)
        {
            Contract.Requires(p != null);
            Contract.Requires(p.SearchType == ArticleFieldSearchType.O2MRelation || p.SearchType == ArticleFieldSearchType.Classifier);

            if (string.IsNullOrWhiteSpace(p.FieldColumn))
            {
                throw new ArgumentException("FieldColumn");
            }

            // параметры не пустые и их не меньше 2х (используем 1й и 2й - остальные отбрасываем)
            if (p.QueryParams == null || p.QueryParams.Length < 3)
            {
                throw new ArgumentException();
            }

            // первый параметр должен быть null или object[]
            if (p.QueryParams[0] != null && !(p.QueryParams[0] is object[]))
            {
                throw new InvalidCastException();
            }

            // второй параметр должен быть bool
            if (!(p.QueryParams[1] is bool))
            {
                throw new InvalidCastException();
            }

            if (!(p.QueryParams[2] is bool))
            {
                throw new InvalidCastException();
            }

            var isNull = (bool)p.QueryParams[1];
            var inverse = (bool)p.QueryParams[2];

            var inverseString = inverse ? "NOT" : "";

            if (isNull)
            {
                return string.Format("({1}.[{0}] IS {2} NULL)", p.FieldColumn.ToLower(), GetTableAlias(p), inverseString);
            }

            // Если массив null или пустой - то возвращаем null
            if (p.QueryParams[0] == null || ((object[])p.QueryParams[0]).Length == 0)
            {
                return null;
            }

            var values = ((object[])p.QueryParams[0]).Select(n => int.Parse(n.ToString())).ToArray();
            var fieldId = p.FieldID ?? "";
            var paramName = "@field" + fieldId.Replace("-", "_");

            if (values.Length == 1)
            {
                sqlParams.Add(new SqlParameter(paramName, values[0]));
                return string.Format(inverse ? "({1}.[{0}] <> {2} OR {1}.[{0}] IS NULL)" : "({1}.[{0}] = {2})", p.FieldColumn.ToLower(), GetTableAlias(p), paramName);
            }

            sqlParams.Add(new SqlParameter(paramName, SqlDbType.Structured) { TypeName = "Ids", Value = Common.IdsToDataTable(values) });
            return string.Format(inverse ? "({1}.[{0}] NOT IN (select id from {2}) OR {1}.[{0}] IS NULL)" : "({1}.[{0}] IN (select id from {2}))", p.FieldColumn.ToLower(), GetTableAlias(p), paramName);
        }

        private static string ParseBooleanParam(ArticleSearchQueryParam p)
        {
            Contract.Requires(p != null);
            Contract.Requires(p.SearchType == ArticleFieldSearchType.Boolean);

            if (string.IsNullOrWhiteSpace(p.FieldColumn))
            {
                throw new ArgumentException("FieldColumn");
            }

            // параметры не пустые и их не меньше 2х (используем 1й, 2й - остальные отбрасываем)
            if (p.QueryParams == null || p.QueryParams.Length < 2)
            {
                throw new ArgumentException();
            }

            // первый параметр должен быть bool
            if (!(p.QueryParams[0] is bool))
            {
                throw new InvalidCastException();
            }

            // второй параметр должен быть bool
            if (!(p.QueryParams[1] is bool))
            {
                throw new InvalidCastException();
            }

            var isNull = (bool)p.QueryParams[0];

            if (isNull)
            {
                return string.Format("({1}.[{0}] IS NULL)", p.FieldColumn.ToLower(), GetTableAlias(p));
            }

            var value = (bool)p.QueryParams[1];
            return string.Format("({1}.[{0}] = {2})", p.FieldColumn.ToLower(), GetTableAlias(p), value ? 1 : 0);

        }

        private static string GetTableAlias(ArticleSearchQueryParam p)
        {
            if (string.IsNullOrEmpty(p.ContentID))
            {
                return ContentTableAlias;
            }

            if (string.IsNullOrEmpty(p.ReferenceFieldID))
            {
                return ContentTableAlias + "_" + p.ContentID;
            }

            return ContentTableAlias + "_" + p.ContentID + "_" + p.ReferenceFieldID;
        }
    }
}
