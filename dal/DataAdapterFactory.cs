using System;
using System.Data.Common;
using System.Data.SqlClient;
using Npgsql;

namespace Quantumart.QP8.DAL
{
    public static class DataAdapterFactory
    {
        public static DataAdapter Create(DbCommand cmd)
        {
            switch (cmd)
            {
                case SqlCommand sqlCmd:
                    return new SqlDataAdapter(sqlCmd);
                case NpgsqlCommand npgsqlCmd:
                    return new NpgsqlDataAdapter(npgsqlCmd);
                default:
                    throw new ApplicationException("Unknown command type");
            }
        }
    }
}
