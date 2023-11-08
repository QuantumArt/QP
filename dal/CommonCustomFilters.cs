using Quantumart.QP8.Constants;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;

namespace Quantumart.QP8.DAL
{
    public static class CommonCustomFilters
    {
        public static string GetFilterQuety(DbConnection sqlConnection, List<DbParameter> parameters, DatabaseType dbType, Dictionary<string, object[]> map)
        {
            if (map == null || map.Count == 0)
            {
                return string.Empty;
            }

            var queries = map
                .Select(item => GetFilterQuety(sqlConnection, parameters, dbType, item))
                .Where(query => !string.IsNullOrEmpty(query))
                .ToArray();

            return $"(({string.Join(") AND (", queries)}))";
        }

        private static string GetFilterQuety(DbConnection sqlConnection, List<DbParameter> parameters, DatabaseType dbType, KeyValuePair<string, object[]> item) => item.Key switch
        {
            "Archive" => GetArchiveFilter(parameters, dbType, GetIntValue(item.Value)),
            "Relation" => GetRelationFilter(sqlConnection, GetIntValue(item.Value)),
            _ => throw new NotImplementedException($"filter {item.Key} is not implemented")
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

        private static int GetIntValue(object[] values)
        {
            if (values != null && values.Length == 1)
            {
                if (values[0] is long value)
                {
                    return (int)value;
                }
            }

            throw new ArgumentException("Values can't be casted to int value", nameof(values));
        }

        private static string GetStringValue(object[] values)
        {
            if (values != null && values.Length == 1)
            {
                if (values[0] is string value)
                {
                    return value;
                }
            }

            throw new ArgumentException("Values can't be casted to int value", nameof(values));
        }
    }
}
