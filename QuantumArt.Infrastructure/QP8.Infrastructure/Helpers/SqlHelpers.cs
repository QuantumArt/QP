using System.Data.SqlClient;

namespace QP8.Infrastructure.Helpers
{
    public class SqlHelpers
    {
        public static bool TryParseConnectionString(string connectionString, out SqlConnectionStringBuilder connectionStringBuilder)
        {
            connectionStringBuilder = null;

            try
            {
                connectionStringBuilder = new SqlConnectionStringBuilder(connectionString.Replace("Provider=SQLOLEDB;", string.Empty));
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
