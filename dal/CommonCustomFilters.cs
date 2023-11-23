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
        public static string GetFilterQuety(DbConnection sqlConnection, List<DbParameter> parameters, DatabaseType dbType, CustomFilter[] filters)
        {
            if (filters == null || filters.Length == 0)
            {
                return string.Empty;
            }

            var queries = filters
                .Select(item => GetFilterQuety(sqlConnection, parameters, dbType, item))
                .Where(query => !string.IsNullOrEmpty(query))
                .ToArray();

            return SqlFilterComposer.Compose(queries);
        }

        private static string GetFilterQuety(DbConnection sqlConnection, List<DbParameter> parameters, DatabaseType dbType, CustomFilter item) => item.Filter switch
        {
            CustomFilter.ArchiveFilter => GetArchiveFilter(parameters, dbType, GetIntValue(item.Value)),
            CustomFilter.RelationFilter => GetRelationFilter(sqlConnection, GetIntValue(item.Value)),
            CustomFilter.FieldFilter => GetFieldFilter(sqlConnection, item.Field, item.Value),
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
            else if (value is int[] ids)
            {
                parameters.AddWithValue("@mtmIds", ids, dbType);
                return $"c.content_item_id in (select linked_item_id from item_link where item_id (select id from {dbType.GetIdTable("@mtmIds")})";
            }

            throw new ArgumentException("Not supported argument type", nameof(value));
        }

        private static string GetFieldFilter(DbConnection sqlConnection, string field, object value)
        {
            return null;
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
