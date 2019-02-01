using System.Data;
using System.Data.Entity.Core.EntityClient;
using System.Data.SqlClient;
using System.Linq;

namespace Quantumart.QP8.DAL
{
    // ReSharper disable once InconsistentNaming
    public partial class QP8Entities
    {

        public QP8Entities(string nameOrConnectionString) : base(nameOrConnectionString)
        {
            Configuration.LazyLoadingEnabled = false;
            Database.CommandTimeout = SqlCommandFactory.CommandTimeout;
        }

        public QP8Entities(EntityConnection entityConnection) : base(entityConnection, false)
        {
        }

        public static readonly string CountColumn = "ROWS_COUNT";


    }
}
