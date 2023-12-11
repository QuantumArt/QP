using Npgsql;
using Quantumart.QP8.Constants;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;

namespace Quantumart.QP8.DAL
{
    public static class DbExtensions
    {
        public static DbParameter AddWithValue(this DbParameterCollection parameterCollection, string parameterName, object value)
        {
            switch (parameterCollection)
            {
                case SqlParameterCollection sqlParameterCollection:
                    return sqlParameterCollection.AddWithValue(parameterName, value ?? DBNull.Value);
                case NpgsqlParameterCollection npgsqlParameterCollection:
                    return npgsqlParameterCollection.AddWithValue(parameterName, value ?? DBNull.Value);
                default:
                    throw new ApplicationException("Unknown db type");
            }
        }

        public static DbParameter AddWithValue(this DbParameterCollection parameterCollection, string parameterName, object value, DbType dbType)
        {
            switch (parameterCollection)
            {
                case SqlParameterCollection sqlParameterCollection:
                    var sqlParam = sqlParameterCollection.AddWithValue(parameterName,  value ?? DBNull.Value );
                    sqlParam.DbType = dbType;
                    return sqlParam;
                case NpgsqlParameterCollection npgsqlParameterCollection:
                    var npgsqlParam = npgsqlParameterCollection.AddWithValue(parameterName, value ?? DBNull.Value);
                    npgsqlParam.DbType = dbType;
                    return npgsqlParam;

                default:
                    throw new ApplicationException("Unknown db type");
            }
        }

        public static List<DbParameter> AddWithValue(this List<DbParameter> parameterCollection, string parameterName, object value, DatabaseType dbType)
        {
            var parameter = SqlQuerySyntaxHelper.CreateDbParameter(dbType, parameterName, value);
            parameterCollection.Add(parameter);
            return parameterCollection;
        }

        public static List<DbParameter> AddWithValue(this List<DbParameter> parameterCollection, string parameterName, int[] ids, DatabaseType dbType)
        {
            var parameter = SqlQuerySyntaxHelper.GetIdsDatatableParam(parameterName, ids, dbType);
            parameterCollection.Add(parameter);
            return parameterCollection;
        }

        public static string GetIdTable(this DatabaseType dbType, string name, string alias = "i") =>
            SqlQuerySyntaxHelper.IdList(dbType, name, alias);

        public static void Fill(this DataAdapter da, DataTable dt)
        {
            switch (da)
            {
                case SqlDataAdapter sqlDataAdapter:
                    sqlDataAdapter.Fill(dt);
                    break;
                case NpgsqlDataAdapter npgsqlDataAdapter:
                    npgsqlDataAdapter.Fill(dt);
                    break;
                default:
                    throw new ApplicationException("Unknown db type");
            }
        }

        public static ICollection<DbParameter> Clone(this ICollection<DbParameter> parameters) => parameters?
            .Cast<ICloneable>()
            .Select(x => x.Clone())
            .Cast<DbParameter>()
            .ToList();
    }
}
