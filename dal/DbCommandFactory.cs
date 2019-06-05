using System;
using System.Data.Common;
using System.Data.SqlClient;
using Npgsql;
using Quantumart.QP8.Configuration;
using Quantumart.QP8.Constants;

namespace Quantumart.QP8.DAL
{
    public static class DbCommandFactory
    {
        public static DbCommand Create(DatabaseType dbType = DatabaseType.SqlServer)
        {
            switch (dbType)
            {
                case DatabaseType.Unknown:
                    throw new ApplicationException("Database type unknown");
                case DatabaseType.SqlServer:
                    return Init(new SqlCommand());
                case DatabaseType.Postgres:
                    return Init(new NpgsqlCommand());
                default:
                    throw new ArgumentOutOfRangeException(nameof(dbType), dbType, null);
            }
        }

        public static DbCommand Create(string cmdText, DatabaseType dbType = DatabaseType.SqlServer)
        {
            switch (dbType)
            {
                case DatabaseType.Unknown:
                    throw new ApplicationException("Database type unknown");
                case DatabaseType.SqlServer:
                    return Init(new SqlCommand(cmdText));
                case DatabaseType.Postgres:
                    return Init(new NpgsqlCommand(cmdText));
                default:
                    throw new ArgumentOutOfRangeException(nameof(dbType), dbType, null);
            }
        }

        public static DbCommand Create(string cmdText, DbConnection connection)
        {
            switch (connection)
            {
                case SqlConnection sqlConnection:
                    return Init(new SqlCommand(cmdText, sqlConnection));
                case NpgsqlConnection npgsqlConnection:
                    return Init(new NpgsqlCommand(cmdText, npgsqlConnection));

                default:
                    throw new ApplicationException("Database type unknown");
            }
        }

        public static DbCommand Create(string cmdText, DbConnection connection, DbTransaction transaction)
        {
            switch (connection)
            {
                case SqlConnection sqlConnection:
                    return Init(new SqlCommand(cmdText, sqlConnection, transaction as SqlTransaction));
                case NpgsqlConnection npgsqlConnection:
                    return Init(new NpgsqlCommand(cmdText, npgsqlConnection, transaction as NpgsqlTransaction));
                default:
                    throw new ApplicationException("Database type unknown");
            }
        }

        public static int CommandTimeout => (QPConfiguration.CommandTimeout != 0) ? QPConfiguration.CommandTimeout : 120;

        private static DbCommand Init(DbCommand cmd)
        {
            cmd.CommandTimeout = CommandTimeout;
            return cmd;
        }
    }
}
