using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
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
        public string GetFilter(IList<ArticleSearchQueryParam> searchQueryParams, IList<DbParameter> sqlParams)
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

        private static string ParseIdentifierParam(ArticleSearchQueryParam p, ICollection<DbParameter> sqlParams)
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
            var dbType = QPContext.DatabaseType;

            var escapedFieldColumnName = SqlQuerySyntaxHelper.EscapeEntityName(dbType, p.FieldColumn.ToLower());

            if (isByText)
            {
                // Если массив null или пустой - то возвращаем null
                if (p.QueryParams[4] == null || ((object[])p.QueryParams[4]).Length == 0)
                {
                    return null;
                }

                var fieldId = p.FieldId ?? string.Empty;
                var paramName = "@field" + fieldId.Replace("-", "_");
                var values = ((object[])p.QueryParams[4]).Select(n => int.Parse(n.ToString())).ToArray();
                if (values.Length == 1)
                {
                    sqlParams.Add(SqlQuerySyntaxHelper.CreateDbParameter(dbType, paramName, values[0]));
                    return string.Format(inverse ? "({1}.{0} <> {2} OR {1}.{0} IS NULL)" : "({1}.{0} = {2})", escapedFieldColumnName, GetTableAlias(p), paramName);
                }

                sqlParams.Add(SqlQuerySyntaxHelper.GetIdsDatatableParam(paramName, values, dbType));
                return string.Format(inverse ? "({1}.{0} NOT IN (select id from {2}) OR {1}.{0} IS NULL)" : "({1}.{0} IN (select id from {2}))",
                    escapedFieldColumnName, GetTableAlias(p), SqlQuerySyntaxHelper.IdList(dbType, paramName, "i"));
            }

            // ReSharper disable MergeSequentialChecks
            var numberFrom = p.QueryParams[1] is int || p.QueryParams[1] == null ? (int?)p.QueryParams[1] : (long?)p.QueryParams[1];
            var numberTo = p.QueryParams[2] is int || p.QueryParams[2] == null ? (int?)p.QueryParams[2] : (long?)p.QueryParams[2];

            // ReSharper restore MergeSequentialChecks

            if (isByValue)
            {
                return !numberFrom.HasValue ? null : string.Format(inverse ? "({1}.{0} <> {2} OR {1}.{0} IS NULL)" : "({1}.{0} = {2})", escapedFieldColumnName, GetTableAlias(p), numberFrom);
            }

            if (!numberFrom.HasValue && !numberTo.HasValue)
            {
                return null;
            }

            if (numberFrom.HasValue && !numberTo.HasValue)
            {
                return inverse
                    ? $"({GetTableAlias(p)}.{escapedFieldColumnName} < {numberFrom})"
                    : $"({GetTableAlias(p)}.{escapedFieldColumnName} >= {numberFrom})";
            }

            if (!numberFrom.HasValue)
            {
                return inverse
                    ? $"({GetTableAlias(p)}.{escapedFieldColumnName} > {numberTo})"
                    : $"({GetTableAlias(p)}.{escapedFieldColumnName} <= {numberTo})";
            }

            if (numberFrom.Value < numberTo.Value)
            {
                return $"({GetTableAlias(p)}.{escapedFieldColumnName} {(inverse ? "NOT" : "")} BETWEEN {numberFrom} AND {numberTo})";
            }

            return numberFrom.Value > numberTo.Value
                ? $"({GetTableAlias(p)}.{escapedFieldColumnName} {(inverse ? "NOT" : "")} BETWEEN {numberTo} AND {numberFrom})"
                : inverse
                    ? $"({GetTableAlias(p)}.{escapedFieldColumnName} <> {numberFrom})"
                    : $"({GetTableAlias(p)}.{escapedFieldColumnName} = {numberFrom})";
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
            var listTexts = (p.QueryParams.Length > 5 && p.QueryParams[5] is object[]) ? (object[])p.QueryParams[5] : null;

            var dbType = QPContext.DatabaseType;
            var escapedFieldColumnName = SqlQuerySyntaxHelper.EscapeEntityName(dbType, p.FieldColumn.ToLower());
            // isnull == true

            if (isNull)
            {
                return $"({GetTableAlias(p)}.{escapedFieldColumnName} IS {(inverse ? "NOT " : "")}NULL)";
            }

            // isnull == false  строка пустая
            if (string.IsNullOrEmpty((string)p.QueryParams[1]))
            {
                return null;
            }

            // Иначе формируем результат
            var value = exactMatch ?
                Cleaner.ToSafeSqlString(((string)p.QueryParams[1]).Trim()) :
                Cleaner.ToSafeSqlLikeCondition(dbType, ((string)p.QueryParams[1]).Trim());

            if (exactMatch)
            {
                return $"({GetTableAlias(p)}.{escapedFieldColumnName} {(inverse ? "<> " : "=")} '{value}')";
            }

            return startFromBegin
                ? $"({GetTableAlias(p)}.{escapedFieldColumnName} LIKE '{(inverse ? "%" + value : value + "%")}')"
                : $"({GetTableAlias(p)}.{escapedFieldColumnName} {(inverse ? "NOT " : "")}LIKE '%{value}%')";
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

            var dbType = QPContext.DatabaseType;
            var escapedFieldColumnName = SqlQuerySyntaxHelper.EscapeEntityName(dbType, p.FieldColumn.ToLower());
            if (isNull)
            {
                return $"({GetTableAlias(p)}.{escapedFieldColumnName} IS NULL)";
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

                return $"({GetTableAlias(p)}.{escapedFieldColumnName} BETWEEN '{sqlDateFromString}' AND '{sqlDateToString}')";
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

                return $"({GetTableAlias(p)}.{escapedFieldColumnName} >= '{sqlDateFromString}')";
            }

            // дата "от" пустая а "до" не пустая
            if (string.IsNullOrWhiteSpace(dateFromString) && !string.IsNullOrWhiteSpace(dateToString))
            {
                if (!Converter.TryConvertToSqlDateString(dateToString, new TimeSpan(23, 59, 59), out sqlDateToString, out dateTo))
                {
                    throw new FormatException("date To");
                }

                return $"({GetTableAlias(p)}.{escapedFieldColumnName} <= '{sqlDateToString}')";
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
                return $"({GetTableAlias(p)}.{escapedFieldColumnName} BETWEEN '{sqlDateFromString}' AND '{sqlDateToString}')";
            }

            Converter.TryConvertToSqlDateString(dateFromString, new TimeSpan(23, 59, 59), out sqlDateFromString, out dateFrom);
            Converter.TryConvertToSqlDateString(dateToString, null, out sqlDateToString, out dateTo);

            return $"({GetTableAlias(p)}.{escapedFieldColumnName} BETWEEN '{sqlDateToString}' AND '{sqlDateFromString}')";
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

            var dbType = QPContext.DatabaseType;

            var fieldName = SqlQuerySyntaxHelper.EscapeEntityName(dbType, p.FieldColumn.ToLower());
            if (isNull)
            {
                return $"({GetTableAlias(p)}.{fieldName} IS NULL)";
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

                return $"({GetTableAlias(p)}.{fieldName} = '{sqlDateTimeFromString}')";
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

                return $"({GetTableAlias(p)}.{fieldName} >= '{sqlDateTimeFromString}')";
            }

            // дата "от" пустая а "до" не пустая
            if (string.IsNullOrWhiteSpace(datetimeFromString) && !string.IsNullOrWhiteSpace(datetimeToString))
            {
                if (!Converter.TryConvertToSqlDateString(datetimeToString, new TimeSpan(23, 59, 59), out sqlDateTimeToString, out datetimeTo))
                {
                    throw new FormatException("datetime To");
                }

                return $"({GetTableAlias(p)}.{fieldName} <= '{sqlDateTimeToString}')";
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
                ? $"({GetTableAlias(p)}.{fieldName} BETWEEN '{sqlDateTimeFromString}' AND '{sqlDateTimeToString}')"
                : $"({GetTableAlias(p)}.{fieldName} BETWEEN '{sqlDateTimeToString}' AND '{sqlDateTimeFromString}')";
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

            var dbType = QPContext.DatabaseType;
            var fieldName = SqlQuerySyntaxHelper.EscapeEntityName(dbType, p.FieldColumn.ToLower());
            // isnull == true

            if (isNull)
            {
                return $"({GetTableAlias(p)}.{fieldName} IS NULL)";
            }

            var ns = SqlQuerySyntaxHelper.DbSchemaName(dbType);
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
                return $"({ns}.qp_abs_time_seconds({GetTableAlias(p)}.{fieldName}) = {timeFrom.Value.TotalSeconds})";
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
                return $"({ns}.qp_abs_time_seconds({GetTableAlias(p)}.{fieldName}) >= {timeFrom.Value.TotalSeconds})";
            }

            // дата "от" пустая а "до" не пустая
            if (string.IsNullOrWhiteSpace(timeFromString) && !string.IsNullOrWhiteSpace(timeToString))
            {
                // ReSharper disable once PossibleInvalidOperationException
                return $"({ns}.qp_abs_time_seconds({GetTableAlias(p)}.{fieldName}) <= {timeTo.Value.TotalSeconds})";
            }

            // обе границы диапазона не пустые
            // From < To
            // ReSharper disable PossibleInvalidOperationException
            if (timeFrom.Value < timeTo.Value)
            {
                return $"({ns}.qp_abs_time_seconds({GetTableAlias(p)}.{fieldName}) BETWEEN {timeFrom.Value.TotalSeconds} AND {timeTo.Value.TotalSeconds})";
            }

            // ReSharper restore PossibleInvalidOperationException

            return timeFrom.Value > timeTo.Value
                ? $"({ns}.qp_abs_time_seconds({GetTableAlias(p)}.{fieldName}) BETWEEN {timeTo.Value.TotalSeconds} AND {timeFrom.Value.TotalSeconds})"
                : $"({ns}.qp_abs_time_seconds({GetTableAlias(p)}.{fieldName}) = {timeFrom.Value.TotalSeconds})";
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

            var dbType = QPContext.DatabaseType;
            var fieldName = SqlQuerySyntaxHelper.EscapeEntityName(dbType, p.FieldColumn.ToLower());
            if (isNull)
            {
                return $"({GetTableAlias(p)}.{fieldName} IS {(inverse ? "NOT " : "")}NULL)";
            }

            // ReSharper disable MergeSequentialChecks
            var numberFrom = p.QueryParams[1] is int || p.QueryParams[1] == null ? (int?)p.QueryParams[1] : (long?)p.QueryParams[1];
            var numberTo = p.QueryParams[2] is int || p.QueryParams[2] == null ? (int?)p.QueryParams[2] : (long?)p.QueryParams[2];

            if (isByValue)
            {
                return !numberFrom.HasValue ? null : $"({GetTableAlias(p)}.{fieldName} {(inverse ? "<>" : "=")} {numberFrom})";
            }

            if (!numberFrom.HasValue && !numberTo.HasValue)
            {
                return null;
            }

            if (numberFrom.HasValue && !numberTo.HasValue)
            {
                return $"({GetTableAlias(p)}.{fieldName} {(inverse ? "<" : ">=")} {numberFrom})";
            }
            if (!numberFrom.HasValue)
            {
                return $"({GetTableAlias(p)}.{fieldName} {(inverse ? ">" : "<=")} {numberTo})";
            }

            if (numberFrom.Value < numberTo.Value)
            {
                return $"({GetTableAlias(p)}.{fieldName} {(inverse ? "NOT " : "")}BETWEEN {numberFrom} AND {numberTo})";
            }

            return numberFrom.Value > numberTo.Value
                ? $"({GetTableAlias(p)}.{fieldName} {(inverse ? "NOT " : "")}BETWEEN {numberTo} AND {numberFrom})"
                : $"({GetTableAlias(p)}.{fieldName} {(inverse ? "<>" : "=")} {numberFrom})";
        }

        private static string ParseO2MRelationParam(ArticleSearchQueryParam p, ICollection<DbParameter> sqlParams)
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

            var dbType = QPContext.DatabaseType;
            var fieldName = SqlQuerySyntaxHelper.EscapeEntityName(dbType, p.FieldColumn.ToLower());

            if (isNull)
            {
                return string.Format("({1}.{0} IS {2} NULL)", fieldName, GetTableAlias(p), inverseString);
            }

            // Если массив null или пустой - то возвращаем null
            if (p.QueryParams[0] == null || ((object[])p.QueryParams[0]).Length == 0)
            {
                return null;
            }

            var values = ((object[])p.QueryParams[0]).Select(n => int.Parse(n.ToString())).ToArray();
            var fieldId = p.FieldId ?? string.Empty;
            var paramName = "@field" + fieldId.Replace("-", "_");

            if (values.Length == 1)
            {
                sqlParams.Add(SqlQuerySyntaxHelper.CreateDbParameter(dbType, paramName, values[0]));
                return inverse
                    ? $"({GetTableAlias(p)}.{fieldName} <> {paramName} OR {GetTableAlias(p)}.{fieldName} IS NULL)"
                    : $"({GetTableAlias(p)}.{fieldName} = {paramName})";
            }

            sqlParams.Add(SqlQuerySyntaxHelper.GetIdsDatatableParam(paramName, values, dbType));
            return inverse
                ? $"({GetTableAlias(p)}.{fieldName} NOT IN (select id from {SqlQuerySyntaxHelper.IdList(dbType, paramName, "i")}) OR {GetTableAlias(p)}.{fieldName} IS NULL)"
                : $"({GetTableAlias(p)}.{fieldName} IN (select id from {SqlQuerySyntaxHelper.IdList(dbType, paramName, "i")}))";
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

            var dbType = QPContext.DatabaseType;
            var fieldName = SqlQuerySyntaxHelper.EscapeEntityName(dbType, p.FieldColumn.ToLower());
            return (bool)p.QueryParams[0]
                ? $"({GetTableAlias(p)}.{fieldName} IS NULL)"
                : $"({GetTableAlias(p)}.{fieldName} = {((bool)p.QueryParams[1] ? 1 : 0)})";
        }

        private static string GetTableAlias(ArticleSearchQueryParam p)
        {
            if (string.IsNullOrEmpty(p.ContentId))
            {
                return ContentTableAlias;
            }

            return string.IsNullOrEmpty(p.ReferenceFieldId)
                ? ContentTableAlias + "_" + p.ContentId
                : ContentTableAlias + "_" + p.ContentId + "_" + p.ReferenceFieldId;
        }
    }
}
