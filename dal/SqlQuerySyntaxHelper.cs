using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using Npgsql;
using NpgsqlTypes;
using Quantumart.QP8.Constants;

namespace Quantumart.QP8.DAL
{
    public class SqlQuerySyntaxHelper
    {
        public static string CastToString(DatabaseType databaseType, string columnName)
        {
            switch (databaseType)
            {
                case DatabaseType.SqlServer:
                    return $"CAST({columnName} as nvarchar(255))";
                case DatabaseType.Postgres:
                    return $"{columnName.ToLower()}::varchar";
                default:
                    throw new ArgumentOutOfRangeException(nameof(databaseType), databaseType, null);
            }
        }

        public static string ToBoolSql(DatabaseType databaseType, bool boolValue)
        {
            switch (databaseType)
            {
                case DatabaseType.SqlServer:
                    return boolValue ? "cast(1 as bit)" : "cast(0 as bit)";
                case DatabaseType.Postgres:
                    return boolValue ? "TRUE" : "FALSE";
                default:
                    throw new ArgumentOutOfRangeException(nameof(databaseType), databaseType, null);
            }
        }

        public static string CastToBool(DatabaseType databaseType, string expression)
        {
            switch (databaseType)
            {
                case DatabaseType.SqlServer:
                    return $"cast({expression} as bit)";
                case DatabaseType.Postgres:
                    return $"(({expression})::int::boolean)";
                default:
                    throw new ArgumentOutOfRangeException(nameof(databaseType), databaseType, null);
            }
        }

        public static string CastToVarchar(DatabaseType databaseType, string expression)
        {
            switch (databaseType)
            {
                case DatabaseType.SqlServer:
                    return $"cast({expression} as nvarchar)";
                case DatabaseType.Postgres:
                    return $"cast({expression} as varchar)";
                default:
                    throw new ArgumentOutOfRangeException(nameof(databaseType), databaseType, null);
            }
        }

        public static string IsTrue(DatabaseType databaseType, string expression)
        {
            switch (databaseType)
            {
                case DatabaseType.SqlServer:
                    return $"{expression} = 1";
                case DatabaseType.Postgres:
                    return $"{expression}";
                default:
                    throw new ArgumentOutOfRangeException(nameof(databaseType), databaseType, null);
            }
        }

        public static string IsFalse(DatabaseType databaseType, string expression)
        {
            switch (databaseType)
            {
                case DatabaseType.SqlServer:
                    return $"{expression} = 0";
                case DatabaseType.Postgres:
                    return $"not {expression}";
                default:
                    throw new ArgumentOutOfRangeException(nameof(databaseType), databaseType, null);
            }
        }


        public static string DbSchemaName(DatabaseType databaseType) => databaseType == DatabaseType.Postgres ? "public" : "dbo";

        public static string WithNoLock(DatabaseType databaseType) => databaseType == DatabaseType.SqlServer ? "with(nolock) " : string.Empty;

        public static string WithRowLock(DatabaseType databaseType) => databaseType == DatabaseType.SqlServer ? "with(rowlock) " : string.Empty;

        public static string RecursiveCte(DatabaseType databaseType) => databaseType == DatabaseType.Postgres ? " RECURSIVE " : string.Empty;

        public static string NullableDbValue(DatabaseType databaseType, int? value)
        {
            switch (databaseType)
            {
                case DatabaseType.SqlServer:
                    return value.HasValue ? value.ToString() : "NULL";
                case DatabaseType.Postgres:
                    return value.HasValue ? value.ToString() : "NULL::numeric";
                default:
                    throw new ArgumentOutOfRangeException(nameof(databaseType), databaseType, null);
            }
        }

        public static string ConcatStrValues(DatabaseType databaseType, params string[] p)
        {
            switch (databaseType)
            {
                case DatabaseType.SqlServer:
                    return string.Join(" + ", p);
                case DatabaseType.Postgres:
                    return string.Join(" || ", p);
                default:
                    throw new ArgumentOutOfRangeException(nameof(databaseType), databaseType, null);
            }
        }

        public static string ConcatStrValuesWithSeparator(DatabaseType databaseType, string separator, params string[] p)
        {
            switch (databaseType)
            {
                case DatabaseType.SqlServer:
                    return string.Join($" + {separator} + ", p);
                case DatabaseType.Postgres:
                    return string.Join($" || {separator} ||", p);
                default:
                    throw new ArgumentOutOfRangeException(nameof(databaseType), databaseType, null);
            }
        }

        public static string EscapeEntityName(DatabaseType databaseType, string entityName)
        {
            switch (databaseType)
            {
                case DatabaseType.SqlServer:
                    return $"[{entityName}]";
                case DatabaseType.Postgres:
                    return $"\"{entityName.ToLower()}\"";
                default:
                    throw new ArgumentOutOfRangeException(nameof(databaseType), databaseType, null);
            }
        }

        public static string Now(DatabaseType databaseType)
        {
            switch (databaseType)
            {
                case DatabaseType.SqlServer:
                    return "getdate()";
                case DatabaseType.Postgres:
                    return "now()";
                default:
                    throw new ArgumentOutOfRangeException(nameof(databaseType), databaseType, null);
            }
        }

        public static string GetFieldLength(DatabaseType databaseType, string fieldName)
        {
            switch (databaseType)
            {
                case DatabaseType.SqlServer:
                    return $"LEN([{fieldName}])";
                case DatabaseType.Postgres:
                    return $"LENGTH(\"{fieldName.ToLower()}\")";
                default:
                    throw new ArgumentOutOfRangeException(nameof(databaseType), databaseType, null);
            }
        }

        public static string Returning(DatabaseType databaseType, string expression)
        {
            switch (databaseType)
            {
                case DatabaseType.SqlServer:
                    return $"OUTPUT inserted.{expression}";
                case DatabaseType.Postgres:
                    return $"returning {expression}";
                default:
                    throw new ArgumentOutOfRangeException(nameof(databaseType), databaseType, null);
            }
        }


        public static string IdList(DatabaseType databaseType, string name, string alias)
        {
            switch (databaseType)
            {
                case DatabaseType.SqlServer:
                    return $"{name} {alias}";
                case DatabaseType.Postgres:
                    return $"unnest({name}) {alias}(id)";
                default:
                    throw new ArgumentOutOfRangeException(nameof(databaseType), databaseType, null);
            }
        }

        public static string StrList(DatabaseType databaseType, string name, string alias)
        {
            switch (databaseType)
            {
                case DatabaseType.SqlServer:
                    return $"{name} {alias}";
                case DatabaseType.Postgres:
                    return $"unnest({name}) {alias}(value)";
                default:
                    throw new ArgumentOutOfRangeException(nameof(databaseType), databaseType, null);
            }
        }

        public static DbParameter GetIdsDatatableParam(string paramName, IEnumerable<int> ids, DatabaseType databaseType = DatabaseType.SqlServer)
        {
            switch (databaseType)
            {
                case DatabaseType.SqlServer:
                    return new SqlParameter(paramName, SqlDbType.Structured)
                    {
                        TypeName = "Ids",
                        Value = IdsToDataTable(ids)
                    };
                case DatabaseType.Postgres:
                    return new NpgsqlParameter(paramName, NpgsqlDbType.Array | NpgsqlDbType.Integer)
                    {
                        Value = ids?.ToArray() ?? new int[0]
                    };
                default:
                    throw new ArgumentOutOfRangeException(nameof(databaseType), databaseType, null);
            }
        }

        public static DbParameter GetStringsDatatableParam(string paramName, IEnumerable<string> strItems, DatabaseType databaseType = DatabaseType.SqlServer)
        {
            switch (databaseType)
            {
                case DatabaseType.SqlServer:
                    return new SqlParameter(paramName, SqlDbType.Structured)
                    {
                        TypeName = "Values",
                        Value = StringsToDataTable(strItems)
                    };
                case DatabaseType.Postgres:
                    return new NpgsqlParameter(paramName, NpgsqlDbType.Array | NpgsqlDbType.Text)
                    {
                        Value = strItems?.ToArray() ?? new string[0]
                    };
                default:
                    throw new ArgumentOutOfRangeException(nameof(databaseType), databaseType, null);
            }
        }

        public static DbParameter GetXmlParameter(string paramName, string xml, DatabaseType databaseType = DatabaseType.SqlServer)
        {
            if (databaseType == DatabaseType.SqlServer)
            {
                return new SqlParameter(paramName, SqlDbType.Xml) { Value = xml };
            }
            else
            {
                return new NpgsqlParameter(paramName, NpgsqlDbType.Xml) { Value = xml };
            }
        }

        private static DataTable IdsToDataTable(IEnumerable<int> ids)
        {
            var dt = new DataTable();
            dt.Columns.Add("id");
            foreach (var id in ids ?? Enumerable.Empty<int>())
            {
                dt.Rows.Add(id);
            }

            return dt;
        }

        private static DataTable StringsToDataTable(IEnumerable<string> strItems)
        {
            var dt = new DataTable();
            dt.Columns.Add("articleId");
            dt.Columns.Add("contentId");
            dt.Columns.Add("fieldId");
            dt.Columns.Add("value");
            var i = 0;
            foreach (var strItem in strItems ?? Enumerable.Empty<string>())
            {
                dt.Rows.Add(i, i, i, strItem);
                i++;
            }
            return dt;
        }

        public static DbParameter CreateDbParameter(DatabaseType dbType, string paramName, object value)
        {
            switch (dbType)
            {
                case DatabaseType.SqlServer:
                    return new SqlParameter(paramName, value);
                case DatabaseType.Postgres:
                    return new NpgsqlParameter(paramName, value);
                default:
                    throw new ArgumentOutOfRangeException(nameof(dbType), dbType, null);
            }
        }

        public static string Top(DatabaseType databaseType, string top)
        {
            switch (databaseType)
            {
                case DatabaseType.SqlServer:
                    return $"TOP ({top})";
                case DatabaseType.Postgres:
                    return String.Empty;
                default:
                    throw new ArgumentOutOfRangeException(nameof(databaseType), databaseType, null);
            }
        }

        public static string Limit(DatabaseType databaseType, string limit)
        {
            switch (databaseType)
            {
                case DatabaseType.SqlServer:
                    return String.Empty;
                case DatabaseType.Postgres:
                    return $"LIMIT {limit}";
                default:
                    throw new ArgumentOutOfRangeException(nameof(databaseType), databaseType, null);
            }
        }

        public static string SpCall(DatabaseType databaseType, string name, string paramString)
        {
            switch (databaseType)
            {
                case DatabaseType.SqlServer:
                    return $@"exec {name} {paramString}";
                case DatabaseType.Postgres:
                    return $@"call {name}({paramString});";
                default:
                    throw new ArgumentOutOfRangeException(nameof(databaseType), databaseType, null);
            }
        }


        public static string Least(DatabaseType databaseType, string first, string second)
        {
            switch (databaseType)
            {
                case DatabaseType.SqlServer:
                    return $@"IIF({first} < {second}, {first}, {second})";
                case DatabaseType.Postgres:
                    return $@"LEAST({first}, {second})";
                default:
                    throw new ArgumentOutOfRangeException(nameof(databaseType), databaseType, null);
            }
        }

        public static object BooleanToNumeric(DatabaseType dbType, bool value)
        {
            if (dbType == DatabaseType.SqlServer)
            {
                return value;
            }

            return value ? 1 : 0;
        }


    }
}
