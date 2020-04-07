using System.Data.Common;
using System.Data.SqlClient;
using Npgsql;
using Quantumart.QP8.Constants;

namespace QP8.Infrastructure.Helpers
{
    public static class SqlHelpers
    {
        public static bool TryParseConnectionString(string connectionString, DatabaseType DbType, out DbConnectionStringBuilder connectionStringBuilder)
        {
            connectionStringBuilder = null;

            try
            {
                var str = connectionString.Replace("Provider=SQLOLEDB;", string.Empty);
                if (DbType == DatabaseType.Postgres)
                {
                    connectionStringBuilder = new NpgsqlConnectionStringBuilder(str);
                }
                else if (DbType == DatabaseType.SqlServer)
                {
                    connectionStringBuilder = new SqlConnectionStringBuilder(str);
                }
                else
                {
                    return false;
                }

                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
