using System.Data.SqlClient;
using Quantumart.QP8.Configuration;

namespace Quantumart.QP8.DAL
{
	public static class SqlCommandFactory
	{
		public static SqlCommand Create()
		{
			return Init(new SqlCommand());
		}

		public static SqlCommand Create(string cmdText)
		{
			return Init(new SqlCommand(cmdText));
		}

		public static SqlCommand Create(string cmdText, SqlConnection connection)
		{
			return Init(new SqlCommand(cmdText, connection));
		}

        public static SqlCommand Create(string cmdText, SqlConnection connection, SqlTransaction transaction)
		{
			return Init(new SqlCommand(cmdText, connection, transaction));
		}

		public static int CommandTimeout => QPConfiguration.WebConfigSection != null ? QPConfiguration.WebConfigSection.CommandTimeout : 120;

	    private static SqlCommand Init(SqlCommand cmd)
		{
			cmd.CommandTimeout = CommandTimeout;
			return cmd;
		}
	}
}
