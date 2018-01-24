using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using QP8.Infrastructure;
using Quantumart.QP8.DAL;
using Quantumart.QP8.Utils;

namespace Quantumart.QP8.BLL.Repository.ArticleRepositories.SearchParsers
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
        public string GetFilter(IList<ArticleSearchQueryParam> searchQueryParams, IList<SqlParameter> sqlParams)
        {
            if (searchQueryParams == null || !searchQueryParams.Any())
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
            }.Contains(p.SearchType)).ToList();

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
            Ensure.NotNull(p);
            Ensure.That(p.SearchType == ArticleFieldSearchType.Identifier);

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

            // ReSharper disable MergeSequentialChecks
            var numberFrom = p.QueryParams[1] is int || p.QueryParams[1] == null ? (int?)p.QueryParams[1] : (long?)p.QueryParams[1];
            var numberTo = p.QueryParams[2] is int || p.QueryParams[2] == null ? (int?)p.QueryParams[2] : (long?)p.QueryParams[2];

            // ReSharper restore MergeSequentialChecks

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
                return inverse
                    ? $"({GetTableAlias(p)}.[{p.FieldColumn.ToLower()}] < {numberFrom})"
                    : $"({GetTableAlias(p)}.[{p.FieldColumn.ToLower()}] >= {numberFrom})";
            }

            if (!numberFrom.HasValue)
            {
                return inverse
                    ? $"({GetTableAlias(p)}.[{p.FieldColumn.ToLower()}] > {numberTo})"
                    : $"({GetTableAlias(p)}.[{p.FieldColumn.ToLower()}] <= {numberTo})";
            }

            if (numberFrom.Value < numberTo.Value)
            {
                return $"({GetTableAlias(p)}.[{p.FieldColumn.ToLower()}] {(inverse ? "NOT" : "")} BETWEEN {numberFrom} AND {numberTo})";
            }

            return numberFrom.Value > numberTo.Value
                ? $"({GetTableAlias(p)}.[{p.FieldColumn.ToLower()}] {(inverse ? "NOT" : "")} BETWEEN {numberTo} AND {numberFrom})"
                : inverse
                    ? $"({GetTableAlias(p)}.[{p.FieldColumn.ToLower()}] <> {numberFrom})"
                    : $"({GetTableAlias(p)}.[{p.FieldColumn.ToLower()}] = {numberFrom})";
        }

        /// <summary>
        /// Парсинг параметра поиска по тексту
        /// </summary>
        private static string ParseTextParam(ArticleSearchQueryParam p)
        {
            Ensure.NotNull(p);
            Ensure.That(p.SearchType == ArticleFieldSearchType.Text || p.SearchType == ArticleFieldSearchType.StringEnum);

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
                return $"({GetTableAlias(p)}.[{p.FieldColumn.ToLower()}] IS {(inverse ? "NOT " : "")}NULL)";
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
                return $"({GetTableAlias(p)}.[{p.FieldColumn.ToLower()}] {(inverse ? "<> " : "=")} '{value}')";
            }

            return startFromBegin
                ? $"({GetTableAlias(p)}.[{p.FieldColumn.ToLower()}] LIKE '{(inverse ? "%" + value : value + "%")}')"
                : $"({GetTableAlias(p)}.[{p.FieldColumn.ToLower()}] {(inverse ? "NOT " : "")}LIKE '%{value}%')";
        }

        private static string ParseDateRangeParam(ArticleSearchQueryParam p)
        {
            Ensure.NotNull(p);
            Ensure.That(p.SearchType == ArticleFieldSearchType.DateRange);

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
                return $"({GetTableAlias(p)}.[{p.FieldColumn.ToLower()}] IS NULL)";
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

                return $"({GetTableAlias(p)}.[{p.FieldColumn.ToLower()}] BETWEEN '{sqlDateFromString}' AND '{sqlDateToString}')";
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

                return $"({GetTableAlias(p)}.[{p.FieldColumn.ToLower()}] >= '{sqlDateFromString}')";
            }

            // дата "от" пустая а "до" не пустая
            if (string.IsNullOrWhiteSpace(dateFromString) && !string.IsNullOrWhiteSpace(dateToString))
            {
                if (!Converter.TryConvertToSqlDateString(dateToString, new TimeSpan(23, 59, 59), out sqlDateToString, out dateTo))
                {
                    throw new FormatException("date To");
                }

                return $"({GetTableAlias(p)}.[{p.FieldColumn.ToLower()}] <= '{sqlDateToString}')";
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
            // ReSharper disable PossibleInvalidOperationException
            if (dateFrom.Value.Date < dateTo.Value.Date)
            {
                Converter.TryConvertToSqlDateString(dateFromString, null, out sqlDateFromString, out dateFrom);
                Converter.TryConvertToSqlDateString(dateToString, new TimeSpan(23, 59, 59), out sqlDateToString, out dateTo);
                return $"({GetTableAlias(p)}.[{p.FieldColumn.ToLower()}] BETWEEN '{sqlDateFromString}' AND '{sqlDateToString}')";
            }

            Converter.TryConvertToSqlDateString(dateFromString, new TimeSpan(23, 59, 59), out sqlDateFromString, out dateFrom);
            Converter.TryConvertToSqlDateString(dateToString, null, out sqlDateToString, out dateTo);

            return $"({GetTableAlias(p)}.[{p.FieldColumn.ToLower()}] BETWEEN '{sqlDateToString}' AND '{sqlDateFromString}')";
        }

        private static string ParseDateTimeRangeParam(ArticleSearchQueryParam p)
        {
            Ensure.NotNull(p);
            Ensure.That(p.SearchType == ArticleFieldSearchType.DateTimeRange);

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
                return $"({GetTableAlias(p)}.[{p.FieldColumn.ToLower()}] IS NULL)";
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

                return $"({GetTableAlias(p)}.[{p.FieldColumn.ToLower()}] = '{sqlDateTimeFromString}')";
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

                return $"({GetTableAlias(p)}.[{p.FieldColumn.ToLower()}] >= '{sqlDateTimeFromString}')";
            }

            // дата "от" пустая а "до" не пустая
            if (string.IsNullOrWhiteSpace(datetimeFromString) && !string.IsNullOrWhiteSpace(datetimeToString))
            {
                if (!Converter.TryConvertToSqlDateString(datetimeToString, new TimeSpan(23, 59, 59), out sqlDateTimeToString, out datetimeTo))
                {
                    throw new FormatException("datetime To");
                }

                return $"({GetTableAlias(p)}.[{p.FieldColumn.ToLower()}] <= '{sqlDateTimeToString}')";
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
            // ReSharper disable PossibleInvalidOperationException
            return datetimeFrom.Value < datetimeTo.Value
                ? $"({GetTableAlias(p)}.[{p.FieldColumn.ToLower()}] BETWEEN '{sqlDateTimeFromString}' AND '{sqlDateTimeToString}')"
                : $"({GetTableAlias(p)}.[{p.FieldColumn.ToLower()}] BETWEEN '{sqlDateTimeToString}' AND '{sqlDateTimeFromString}')";
            // ReSharper restore PossibleInvalidOperationException
        }

        private static string ParseTimeRangeParam(ArticleSearchQueryParam p)
        {
            Ensure.NotNull(p);
            Ensure.That(p.SearchType == ArticleFieldSearchType.TimeRange);

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

            TimeSpan? timeFrom = null;
            TimeSpan? timeTo = null;

            // isnull == true
            if (isNull)
            {
                return $"({GetTableAlias(p)}.[{p.FieldColumn.ToLower()}] IS NULL)";
            }

            if (isByValue)
            {
                if (string.IsNullOrWhiteSpace(timeFromString))
                {
                    return null;
                }

                if (!Converter.TryConvertToSqlTimeString(timeFromString, out _, out timeFrom))
                {
                    throw new FormatException("time From");
                }

                // ReSharper disable once PossibleInvalidOperationException
                return $"([dbo].[qp_abs_time_seconds]({GetTableAlias(p)}.[{p.FieldColumn.ToLower()}]) = {timeFrom.Value.TotalSeconds})";
            }

            // если обе даты пустые - то возвращаем null
            if (string.IsNullOrWhiteSpace(timeFromString) && string.IsNullOrWhiteSpace(timeToString))
            {
                return null;
            }

            if (!string.IsNullOrWhiteSpace(timeFromString))
            {
                if (!Converter.TryConvertToSqlTimeString(timeFromString, out _, out timeFrom))
                {
                    throw new FormatException("time From");
                }
            }

            if (!string.IsNullOrWhiteSpace(timeToString))
            {
                if (!Converter.TryConvertToSqlTimeString(timeToString, out var _, out timeTo))
                {
                    throw new FormatException("time To");
                }
            }

            // дата "до" пустая а "от" не пустая
            if (!string.IsNullOrWhiteSpace(timeFromString) && string.IsNullOrWhiteSpace(timeToString))
            {
                // ReSharper disable once PossibleInvalidOperationException
                return $"([dbo].[qp_abs_time_seconds]({GetTableAlias(p)}.[{p.FieldColumn.ToLower()}]) >= {timeFrom.Value.TotalSeconds})";
            }

            // дата "от" пустая а "до" не пустая
            if (string.IsNullOrWhiteSpace(timeFromString) && !string.IsNullOrWhiteSpace(timeToString))
            {
                // ReSharper disable once PossibleInvalidOperationException
                return $"([dbo].[qp_abs_time_seconds]({GetTableAlias(p)}.[{p.FieldColumn.ToLower()}]) <= {timeTo.Value.TotalSeconds})";
            }

            // обе границы диапазона не пустые
            // From < To
            // ReSharper disable PossibleInvalidOperationException
            if (timeFrom.Value < timeTo.Value)
            {
                return $"([dbo].[qp_abs_time_seconds]({GetTableAlias(p)}.[{p.FieldColumn.ToLower()}]) BETWEEN {timeFrom.Value.TotalSeconds} AND {timeTo.Value.TotalSeconds})";
            }

            // ReSharper restore PossibleInvalidOperationException

            return timeFrom.Value > timeTo.Value
                ? $"([dbo].[qp_abs_time_seconds]({GetTableAlias(p)}.[{p.FieldColumn.ToLower()}]) BETWEEN {timeTo.Value.TotalSeconds} AND {timeFrom.Value.TotalSeconds})"
                : $"([dbo].[qp_abs_time_seconds]({GetTableAlias(p)}.[{p.FieldColumn.ToLower()}]) = {timeFrom.Value.TotalSeconds})";
        }

        private static string ParseNumericRangeParam(ArticleSearchQueryParam p)
        {
            Ensure.NotNull(p);
            Ensure.That(p.SearchType == ArticleFieldSearchType.NumericRange);

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
                return $"({GetTableAlias(p)}.[{p.FieldColumn.ToLower()}] IS {(inverse ? "NOT " : "")}NULL)";
            }

            // ReSharper disable MergeSequentialChecks
            var numberFrom = p.QueryParams[1] is int || p.QueryParams[1] == null ? (int?)p.QueryParams[1] : (long?)p.QueryParams[1];
            var numberTo = p.QueryParams[2] is int || p.QueryParams[2] == null ? (int?)p.QueryParams[2] : (long?)p.QueryParams[2];

            if (isByValue)
            {
                return !numberFrom.HasValue ? null : $"({GetTableAlias(p)}.[{p.FieldColumn.ToLower()}] {(inverse ? "<>" : "=")} {numberFrom})";
            }

            if (!numberFrom.HasValue && !numberTo.HasValue)
            {
                return null;
            }

            if (numberFrom.HasValue && !numberTo.HasValue)
            {
                return $"({GetTableAlias(p)}.[{p.FieldColumn.ToLower()}] {(inverse ? "<" : ">=")} {numberFrom})";
            }
            if (!numberFrom.HasValue)
            {
                return $"({GetTableAlias(p)}.[{p.FieldColumn.ToLower()}] {(inverse ? ">" : "<=")} {numberTo})";
            }

            if (numberFrom.Value < numberTo.Value)
            {
                return $"({GetTableAlias(p)}.[{p.FieldColumn.ToLower()}] {(inverse ? "NOT " : "")}BETWEEN {numberFrom} AND {numberTo})";
            }

            return numberFrom.Value > numberTo.Value
                ? $"({GetTableAlias(p)}.[{p.FieldColumn.ToLower()}] {(inverse ? "NOT " : "")}BETWEEN {numberTo} AND {numberFrom})"
                : $"({GetTableAlias(p)}.[{p.FieldColumn.ToLower()}] {(inverse ? "<>" : "=")} {numberFrom})";
        }

        private static string ParseO2MRelationParam(ArticleSearchQueryParam p, ICollection<SqlParameter> sqlParams)
        {
            Ensure.NotNull(p);
            Ensure.That(p.SearchType == ArticleFieldSearchType.O2MRelation || p.SearchType == ArticleFieldSearchType.Classifier);

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
            var inverseString = inverse ? "NOT" : string.Empty;
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
            var fieldId = p.FieldID ?? string.Empty;
            var paramName = "@field" + fieldId.Replace("-", "_");

            if (values.Length == 1)
            {
                sqlParams.Add(new SqlParameter(paramName, values[0]));
                return inverse
                    ? $"({GetTableAlias(p)}.[{p.FieldColumn.ToLower()}] <> {paramName} OR {GetTableAlias(p)}.[{p.FieldColumn.ToLower()}] IS NULL)"
                    : $"({GetTableAlias(p)}.[{p.FieldColumn.ToLower()}] = {paramName})";
            }

            sqlParams.Add(new SqlParameter(paramName, SqlDbType.Structured) { TypeName = "Ids", Value = Common.IdsToDataTable(values) });
            return inverse
                ? $"({GetTableAlias(p)}.[{p.FieldColumn.ToLower()}] NOT IN (select id from {paramName}) OR {GetTableAlias(p)}.[{p.FieldColumn.ToLower()}] IS NULL)"
                : $"({GetTableAlias(p)}.[{p.FieldColumn.ToLower()}] IN (select id from {paramName}))";
        }

        private static string ParseBooleanParam(ArticleSearchQueryParam p)
        {
            Ensure.NotNull(p);
            Ensure.That(p.SearchType == ArticleFieldSearchType.Boolean);

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

            return (bool)p.QueryParams[0]
                ? $"({GetTableAlias(p)}.[{p.FieldColumn.ToLower()}] IS NULL)"
                : $"({GetTableAlias(p)}.[{p.FieldColumn.ToLower()}] = {((bool)p.QueryParams[1] ? 1 : 0)})";
        }

        private static string GetTableAlias(ArticleSearchQueryParam p)
        {
            if (string.IsNullOrEmpty(p.ContentID))
            {
                return ContentTableAlias;
            }

            return string.IsNullOrEmpty(p.ReferenceFieldID)
                ? ContentTableAlias + "_" + p.ContentID
                : ContentTableAlias + "_" + p.ContentID + "_" + p.ReferenceFieldID;
        }
    }
}
