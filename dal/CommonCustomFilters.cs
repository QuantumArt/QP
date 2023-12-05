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
            int parentId,
            CustomFilter[] filters)
        {
            if (filters == null || filters.Length == 0)
            {
                return string.Empty;
            }

            var queries = filters
                .Select((item, index) => GetFilterQuery(sqlConnection, parameters, dbType, parentId, item, index))
                .Where(query => !string.IsNullOrEmpty(query))
                .ToArray();

            return SqlFilterComposer.Compose(queries);
        }

        private static string GetFilterQuery(DbConnection sqlConnection, List<DbParameter> parameters, DatabaseType dbType, int parentId, CustomFilter item, int index) => item.Filter switch
        {
            CustomFilter.ArchiveFilter => GetArchiveFilter(parameters, dbType, GetIntValue(item.Value)),
            CustomFilter.VirtualTypeFilter => GetVirtualTypeFilter(GetIntValue(item.Value)),
            CustomFilter.RelationFilter => GetRelationFilter(sqlConnection, GetIntValue(item.Value)),
            CustomFilter.BackwardFilter => GetBackwardFilter(sqlConnection, parameters, dbType, item.Value),
            CustomFilter.FieldFilter => GetFieldFilter(sqlConnection, parameters, dbType, parentId, item.Field, item.Value, index),
            CustomFilter.M2MFilter => GetM2MFilter(parameters, dbType, item.Value, index),
            _ => throw new NotImplementedException($"filter {item.Filter} is not implemented")
        };

        private static string GetArchiveFilter(List<DbParameter> parameters, DatabaseType dbType, int archive)
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

            if (value is JArray array)
            {
                var values = array.ToObject<int[]>();
                articleId = values[0];
                fieldId = values[1];
            }
            else
            {
                throw new ArgumentException("array expected", nameof(value));
            }

            var query = "SELECT ATTRIBUTE_NAME FROM CONTENT_ATTRIBUTE WHERE ATTRIBUTE_ID = @attributeId";

            using (var cmd = DbCommandFactory.Create(query, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@attributeId", fieldId);

                var field = cmd.ExecuteScalar();

                if (DBNull.Value.Equals(field))
                {
                    return null;
                }
                else
                {
                    parameters.AddWithValue("@currentArticleId", articleId, dbType);
                    var escapedField = SqlQuerySyntaxHelper.EscapeEntityName(dbType, (string)field);
                    return $"(c.{escapedField} = @currentArticleId OR c.{escapedField} IS NULL) AND c.archive = 0";
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

        private static string GetFieldFilter(DbConnection sqlConnection, List<DbParameter> parameters, DatabaseType dbType, int contentId, string field, object value, int index)
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

            var fieldExpr = SqlQuerySyntaxHelper.EscapeEntityName(dbType, field);
            var paramName = $"@fieldValue{index}";
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
                    if (int.TryParse(stringValue, NumberStyles.Number, CultureInfo.InvariantCulture, out intValue))
                    {
                        parameters.AddWithValue(paramName, intValue, dbType);
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
                if (value is JArray intArray)
                {
                    var ids = intArray.ToObject<int[]>();
                    parameters.AddWithValue(paramName, ids, dbType);
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

            return isAtomic ? $"c.{fieldExpr} = {paramName}" : $"c.{fieldExpr} in (select id from {dbType.GetIdTable(paramName)})";
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

            if (value is int)
            {
                var paramName = $"@fieldValue{index}";
                parameters.AddWithValue(paramName, value, dbType);
                return $"{query} = {paramName})";
            }

            if (value is JArray array)
            {
                var ids = array.ToObject<int[]>();
                var paramName = $"@fieldIds{index}";
                parameters.AddWithValue(paramName, ids, dbType);
                return $"{query} in (select id from {dbType.GetIdTable(paramName)}))";
            }

            throw new ArgumentException("Not supported argument type", nameof(value));
        }

        private static int GetIntValue(object value)
        {
            if (value is long result)
            {
                if (!result.IsInRange(int.MinValue, int.MaxValue, true))
                {
                    throw new ArgumentException("Value is out of range of int values", nameof(value));
                }

                return (int)result;
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
    }
}
