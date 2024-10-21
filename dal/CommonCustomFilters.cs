using Newtonsoft.Json.Linq;
using Quantumart.QP8.Constants;
using Quantumart.QP8.DAL.DTO;
using Quantumart.QP8.Utils;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Globalization;
using System.Linq;

namespace Quantumart.QP8.DAL
{
    public static class CommonCustomFilters
    {
        private const string NumericType = "NUMERIC";
        private const string DateTimeType = "DATETIME";
        private static readonly string[] _stringTypes = new[] { "NVARCHAR", "NTEXT" };

        /// <summary>
        /// Фильтрация кастомных фильтров
        /// </summary>
        /// <param name="sqlConnection">sql соединение</param>
        /// <param name="parameters">параметры sql запроса, каждый фильтр может добавлять туда свои параметры</param>
        /// <param name="dbType">тип базы данных</param>
        /// <param name="parentId">идентификатор родительской сущности для фильтра. В частности это может быть id контента или id сайта</param>
        /// <param name="filters">массив кастомных фильтров</param>
        /// <returns>sql выражение с условиями фильтрации кастомных фильтров</returns>
        public static string GetFilterQuery(
            DbConnection sqlConnection,
            List<DbParameter> parameters,
            DatabaseType dbType,
            string entityTypeCode,
            int parentId,
            CustomFilter[] filters,
            bool useNativeEfTypes = false)
        {
            if (filters == null || filters.Length == 0)
            {
                return string.Empty;
            }

            var queries = filters
                .Select((item, index) => GetFilterQuery(
                    sqlConnection, parameters, dbType, entityTypeCode, parentId, item, index, useNativeEfTypes
                ))
                .Where(query => !string.IsNullOrEmpty(query))
                .ToArray();

            return SqlFilterComposer.Compose(queries);
        }

        private static string GetFilterQuery(
            DbConnection sqlConnection,
            List<DbParameter> parameters,
            DatabaseType dbType,
            string entityTypeCode,
            int parentId,
            CustomFilter item,
            int index,
            bool useNativeEfTypes
        )
        {
            switch (item.Filter)
            {
                case CustomFilter.ArchiveFilter:
                    var useBool = useNativeEfTypes && entityTypeCode == EntityTypeCode.Article
                        && dbType == DatabaseType.Postgres;
                    object archiveValue = useBool ? GetBoolValue(item.Value) : GetIntValue(item.Value);
                    return GetArchiveFilter(parameters, dbType, archiveValue);
                case CustomFilter.VirtualTypeFilter:
                    return GetVirtualTypeFilter(GetIntValue(item.Value));
                case CustomFilter.RelationFilter:
                    return GetRelationFilter(sqlConnection, GetIntValue(item.Value));
                case CustomFilter.BackwardFilter:
                    return GetBackwardFilter(sqlConnection, parameters, dbType, item.Value);
                case CustomFilter.FieldFilter:
                    return GetFieldFilter(sqlConnection, parameters, dbType, parentId, item.Field, item.AllowNull, item.Value, index);
                case CustomFilter.M2MFilter:
                    return GetM2MFilter(parameters, dbType, item.Value, index);
                case CustomFilter.FalseFilter:
                    return GetFalseFilter();
                default:
                    throw new NotImplementedException($"filter {item.Filter} is not implemented");
            }
        }

        private static string GetArchiveFilter(List<DbParameter> parameters, DatabaseType dbType, object archive)
        {
            parameters.AddWithValue("@archive", archive, dbType);
            return "c.archive = @archive";
        }

        private static string GetVirtualTypeFilter(int type)
        {
            return type == 0 ? "c.virtual_type = 0" : "c.virtual_type <> 0";
        }

        private static string GetBackwardFilter(DbConnection sqlConnection, List<DbParameter> parameters, DatabaseType dbType, object value)
        {
            int articleId;
            int fieldId;

            if (TryGetIntValues(value, out var array))
            {
                articleId = array[0];
                fieldId = array[1];
            }
            else
            {
                throw new ArgumentException("array expected", nameof(value));
            }

            var query = "SELECT ca.ATTRIBUTE_NAME, c.USE_NATIVE_EF_TYPES " +
                " FROM CONTENT_ATTRIBUTE ca " +
                " INNER JOIN CONTENT c ON c.CONTENT_ID = ca.CONTENT_ID " +
                " WHERE ATTRIBUTE_ID = @attributeId";

            using (var cmd = DbCommandFactory.Create(query, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@attributeId", fieldId);

                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read() && !DBNull.Value.Equals(reader[0]))
                    {
                        parameters.AddWithValue("@currentArticleId", articleId, dbType);
                        var escapedField = SqlQuerySyntaxHelper.EscapeEntityName(dbType, (string)reader[0]);
                        var useNativeBool = (bool)reader[1];
                        var archiveFilter = useNativeBool ? "not c.archive" : "c.archive = 0";
                        return $"(c.{escapedField} = @currentArticleId OR c.{escapedField} IS NULL) AND {archiveFilter}";
                    }
                    return null;
                }
            }
        }

        private static string GetRelationFilter(DbConnection sqlConnection, int relationFieldId)
        {
            var query = "SELECT RELATION_CONDITION FROM CONTENT_ATTRIBUTE WHERE ATTRIBUTE_ID = @attributeId";

            using (var cmd = DbCommandFactory.Create(query, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@attributeId", relationFieldId);

                var filter = cmd.ExecuteScalar();
                return DBNull.Value.Equals(filter) ? null : (string)filter;
            }
        }

        private static string GetFieldFilter(DbConnection sqlConnection, List<DbParameter> parameters, DatabaseType dbType, int contentId, string field, bool allowNull, object value, int index)
        {
            string fieldType = string.Empty;

            if (FieldName.ContentItemId.Equals(field, StringComparison.InvariantCultureIgnoreCase))
            {
                fieldType = NumericType;
            }
            else
            {
                fieldType = GetAttributeType(sqlConnection, dbType, contentId, field);
            }

            var fieldName = SqlQuerySyntaxHelper.EscapeEntityName(dbType, field);
            var paramName = $"@fieldValue{index}";
            var defaultParamName = $"@defaultFieldValue{index}";
            var isAtomic = true;

            if (fieldType == null)
            {
                throw new ArgumentException($"Field {field} not found", nameof(field));
            }

            if (_stringTypes.Contains(fieldType))
            {
                if (value is string stringValue)
                {
                    parameters.AddWithValue(paramName, stringValue, dbType);
                }
                else
                {
                    throw new ArgumentException($"Value {value} for field {field} must be string", nameof(value));
                }
            }

            if (fieldType == NumericType)
            {
                if (value is int intValue)
                {
                    parameters.AddWithValue(paramName, intValue, dbType);
                }
                if (value is long longValue)
                {
                    parameters.AddWithValue(paramName, longValue, dbType);
                }
                else if (value is decimal numericValue)
                {
                    parameters.AddWithValue(paramName, numericValue, dbType);
                }
                else if (value is string stringValue)
                {
                    if (int.TryParse(stringValue, out intValue))
                    {
                        parameters.AddWithValue(paramName, intValue, dbType);
                    }
                    else if (long.TryParse(stringValue, out longValue))
                    {
                        parameters.AddWithValue(paramName, longValue, dbType);
                    }
                    else if (decimal.TryParse(stringValue, NumberStyles.Number, CultureInfo.InvariantCulture, out decimal decimalValue))
                    {
                        parameters.AddWithValue(paramName, decimalValue, dbType);
                    }
                    else
                    {
                        throw new ArgumentException($"Value {value} for field {field} must be number", nameof(value));
                    }
                }
                else if (TryGetIntValues(value, out var ids))
                {
                    parameters.AddWithValue(paramName, ids, dbType);

                    if (allowNull)
                    {
                        parameters.AddWithValue(defaultParamName, ids.FirstOrDefault(), dbType);
                    }

                    isAtomic = false;
                }
            }

            if (fieldType == DateTimeType)
            {
                if (value is string stringValue)
                {
                    if (DateTime.TryParse(stringValue, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime date))
                    {
                        parameters.AddWithValue(paramName, date, dbType);
                    }
                }

                throw new ArgumentException($"Value {value} for field {field} must be date", nameof(value));
            }

            var fieldExpression = allowNull ? $"COALESCE(c.{fieldName}, {(isAtomic ? paramName : defaultParamName)})" : $"c.{fieldName}";

            return isAtomic ? $"{fieldExpression} = {paramName}" : $"{fieldExpression} in (select id from {dbType.GetIdTable(paramName)})";
        }

        private static string GetAttributeType(DbConnection sqlConnection, DatabaseType dbType, int contentId, string field)
        {
            var comparisonOperator = dbType == DatabaseType.Postgres ? "iLIKE" : "LIKE";

            var query =
                $@"SELECT t.DATABASE_TYPE from CONTENT_ATTRIBUTE a
                JOIN ATTRIBUTE_TYPE t ON a.ATTRIBUTE_TYPE_ID = t.ATTRIBUTE_TYPE_ID
                WHERE CONTENT_ID = @contentId AND ATTRIBUTE_NAME {comparisonOperator} @field";

            using (var cmd = DbCommandFactory.Create(query, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@contentId", contentId);
                cmd.Parameters.AddWithValue("@field", field);

                var fieldType = cmd.ExecuteScalar();
                return DBNull.Value.Equals(fieldType) ? null : (string)fieldType;
            }
        }

        private static string GetM2MFilter(List<DbParameter> parameters, DatabaseType dbType, object value, int index)
        {
            var query = "c.content_item_id in (select linked_item_id from item_link where item_id";

            if (TryGetIntValue(value, out var id))
            {
                var paramName = $"@fieldValue{index}";
                parameters.AddWithValue(paramName, id, dbType);
                return $"{query} = {paramName})";
            }
            else if(TryGetIntValues(value, out var ids))
            {
                var paramName = $"@fieldIds{index}";
                parameters.AddWithValue(paramName, ids, dbType);
                return $"{query} in (select id from {dbType.GetIdTable(paramName)}))";
            }

            throw new ArgumentException("Not supported argument type", nameof(value));
        }

        private static string GetFalseFilter() => "1 = 0";

        private static bool GetBoolValue(object value)
        {
            return Convert.ToBoolean(value);
        }

        private static int GetIntValue(object value)
        {
            if (value is int intResult)
            {
                return intResult;
            }
            else if (value is long longResult)
            {
                if (!longResult.IsInRange(int.MinValue, int.MaxValue, true))
                {
                    throw new ArgumentException("Value is out of range of int values", nameof(value));
                }

                return (int)longResult;
            }

            throw new ArgumentException("Value can't be casted to int value", nameof(value));
        }

        private static string GetStringValue(object value)
        {
            if (value is string result)
            {
                return result;
            }

            throw new ArgumentException("Value can't be casted to string value", nameof(value));
        }

        private static bool TryGetIntValue(object value, out int id)
        {
            if (value is string stringValue)
            {
                if (int.TryParse(stringValue, out var parsedResult))
                {
                    id = parsedResult;
                    return true;
                }
                else
                {
                    id = 0;
                    return false;
                }
            }
            if (value is int intResult)
            {
                id = intResult;
                return true;
            }
            else if (value is long longResult)
            {
                if (!longResult.IsInRange(int.MinValue, int.MaxValue, true))
                {
                    id = 0;
                    return false;
                }

                id = (int)longResult;
                return true;
            }

            id = 0;
            return false;
        }

        private static bool TryGetIntValues(object value, out int[] ids)
        {
            if (value is string stringValue)
            {
                ids = stringValue
                    .Split(',', StringSplitOptions.RemoveEmptyEntries & StringSplitOptions.TrimEntries)
                    .Select(number => int.TryParse(number, out var id) ? id : 0)
                    .ToArray();

                if (ids.All(number => number != 0))
                {
                    return true;
                }
            }
            if (value is JArray jarray)
            {
                ids = jarray.ToObject<int[]>();
                return ids != null;
            }
            else if(value is int[] array)
            {
                ids = array;
                return true ;
            }

            ids = null;
            return false;
        }
    }
}
