using System.Data.Common;
using Microsoft.EntityFrameworkCore;

namespace Quantumart.QP8.DAL
{
    public class SqlServerQPModelDataContext : QPModelDataContext
    {
        public SqlServerQPModelDataContext(string nameOrConnectionString)
            : base(nameOrConnectionString)
        {
        }

        public SqlServerQPModelDataContext(DbConnection dbConnection)
            : base(dbConnection)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
            if (!string.IsNullOrWhiteSpace(_nameOrConnectionString))
            {
                optionsBuilder.UseSqlServer(_nameOrConnectionString);
            }
            else if (_connection != null)
            {
                optionsBuilder.UseSqlServer(_connection);
            }
        }
    }


}
