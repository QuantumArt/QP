using Newtonsoft.Json.Linq;
using Quantumart.QP8.Constants;
using Quantumart.QP8.DAL.DTO;
using Quantumart.QP8.Utils;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;

namespace Quantumart.QP8.DAL
{
    public static class CommonCustomFilters
    {
        public static string GetFilterQuery(DbConnection sqlConnection, List<DbParameter> parameters, DatabaseType dbType, CustomFilter[] filters)
        {
            if (filters == null || filters.Length == 0)
            {
                return string.Empty;
            }

            var queries = filters
                .Select((item, index) => GetFilterQuery(sqlConnection, parameters, dbType, item, index))
                .Where(query => !string.IsNullOrEmpty(query))
                .ToArray();

            return SqlFilterComposer.Compose(queries);
        }

        private static string GetFilterQuery(DbConnection sqlConnection, List<DbParameter> parameters, DatabaseType dbType, CustomFilter item, int index) => item.Filter switch
        {
            CustomFilter.ArchiveFilter => GetArchiveFilter(parameters, dbType, GetIntValue(item.Value)),
            CustomFilter.RelationFilter => GetRelationFilter(sqlConnection, GetIntValue(item.Value)),
            CustomFilter.FieldFilter => GetFieldFilter(parameters, dbType, item.Field, item.Value, index),
            CustomFilter.MtMFilter => GetMtMFilter(parameters, dbType, item.Field, item.Value, index),
            _ => throw new NotImplementedException($"filter {item.Filter} is not implemented")
        };

        private static string GetArchiveFilter(List<DbParameter> parameters, DatabaseType dbType, int archive)
        {
            parameters.AddWithValue("@archive", archive, dbType);
            return "c.archive = @archive";
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

        private static string GetMtMFilter(List<DbParameter> parameters, DatabaseType dbType, object value)
        {
            if (value is int id)
            {
                parameters.AddWithValue("@mtmId", id, dbType);
                return "c.content_item_id in (select linked_item_id from item_link where item_id = @mtmId";
            }
            if (value is JArray array)
            {
                var ids = array.ToObject<int[]>();
                parameters.AddWithValue("@mtmIds", ids, dbType);
                return $"c.content_item_id in (select linked_item_id from item_link where item_id (select id from {dbType.GetIdTable("@mtmIds")})";
            }

            throw new ArgumentException("Not supported argument type", nameof(value));
        }

        private static string GetFieldFilter(List<DbParameter> parameters, DatabaseType dbType, string field, object value, int index)
        {
            var fieldExpr = dbType switch
            {
                DatabaseType.Postgres => $"c.\"{field.ToLowerInvariant()}\"",
                DatabaseType.SqlServer => $"c.[{field}]",
                _ => throw new ArgumentException("Not supported DB type", nameof(dbType))
            };

            if (value is string || value is int)
            {
                var paramName = $"@fieldValue{index}";
                parameters.AddWithValue(paramName, value, dbType);
                return $"{fieldExpr} = {paramName}";
            }

            if (value is JArray array)
            {
                var ids = array.ToObject<int[]>();
                var paramName = $"@fieldIds{index}";
                parameters.AddWithValue(paramName, ids, dbType);
                return $"{fieldExpr} in (select id from {dbType.GetIdTable(paramName)})";
            }

            throw new ArgumentException("Not supported argument type", nameof(value));
        }

        private static string GetMtMFilter(List<DbParameter> parameters, DatabaseType dbType, string field, object value, int index)
        {
            var fieldExpr = dbType switch
            {
                DatabaseType.Postgres => $"c.\"{field.ToLowerInvariant()}\"",
                DatabaseType.SqlServer => $"c.[{field}]",
                _ => throw new ArgumentException("Not supported DB type", nameof(dbType))
            };

            var query = "c.content_item_id in (select linked_item_id from item_link where item_id";

            if (value is int)
            {
                var paramName = $"@fieldValue{index}";
                parameters.AddWithValue(paramName, value, dbType);
                return $"{query} = {paramName}";
            }

            if (value is JArray array)
            {
                var ids = array.ToObject<int[]>();
                var paramName = $"@fieldIds{index}";
                parameters.AddWithValue(paramName, ids, dbType);
                return $"{query} in (select id from {dbType.GetIdTable(paramName)})";
            }

            throw new ArgumentException("Not supported argument type", nameof(value));
        }

        private static int GetIntValue(object value)
        {
            if (value is long result)
            {
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
