using Quantumart.QP8.Constants;

namespace Quantumart.QP8.Configuration
{
    public class QpConnectionInfo
    {

        public QpConnectionInfo(string cnnString, DatabaseType dbType)
        {
            ConnectionString = cnnString;
            DbType = dbType;
        }

        public string ConnectionString { get; set; }

        public DatabaseType DbType { get; set; }
    }
}
