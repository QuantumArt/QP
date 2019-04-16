using System;

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

    }
}
