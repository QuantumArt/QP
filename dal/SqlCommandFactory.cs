using System.Data.SqlClient;
using Quantumart.QP8.Configuration;

namespace Quantumart.QP8.DAL
{
    public static class SqlCommandFactory
    {
        public static SqlCommand Create() => Init(new SqlCommand());

        public static SqlCommand Create(string cmdText) => Init(new SqlCommand(cmdText));

        public static SqlCommand Create(string cmdText, SqlConnection connection) => Init(new SqlCommand(cmdText, connection));

        public static SqlCommand Create(string cmdText, SqlConnection connection, SqlTransaction transaction) => Init(new SqlCommand(cmdText, connection, transaction));

        public static int CommandTimeout => (QPConfiguration.CommandTimeout != 0) ? QPConfiguration.CommandTimeout : 120;

        private static SqlCommand Init(SqlCommand cmd)
        {
            cmd.CommandTimeout = CommandTimeout;
            return cmd;
        }
    }
}
