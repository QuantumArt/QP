using System;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using Npgsql;

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
    }
}
