using System;
using System.Data.Common;
using System.Data.SqlClient;
using Npgsql;

namespace Quantumart.QP8.DAL
{
    public static class DatabaseTypeHelper
    {
        public static DatabaseType ResolveDatabaseType(string connectionString)
        {
            #warning временный костыль. нужно реализовать нормальное определение sqlServer/postgres
            return connectionString.IndexOf("MSCPGSQL01", StringComparison.InvariantCultureIgnoreCase) != -1
                ? DatabaseType.Postgres
                : DatabaseType.SqlServer;
        }

        public static DatabaseType ResolveDatabaseType(QPModelDataContext context)
        {
            switch (context)
            {
                case SqlServerQPModelDataContext _:
                    return DatabaseType.SqlServer;
                case NpgSqlQPModelDataContext _:
                    return DatabaseType.Postgres;
                default:
                    return DatabaseType.Unknown;
            }
        }

        public static DatabaseType ResolveDatabaseType(DbConnection connection)
        {
            switch (connection)
            {
                case SqlConnection _:
                    return DatabaseType.SqlServer;
                case NpgsqlConnection _:
                    return DatabaseType.Postgres;
                default:
                    return DatabaseType.Unknown;
            }
        }



    }
}
