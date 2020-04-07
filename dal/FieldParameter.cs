using System.Data;
using NpgsqlTypes;

namespace Quantumart.QP8.DAL
{
    public class FieldParameter
    {
        public string Name { get; set; }

        public DbType DbType { get; set; }

        public NpgsqlDbType NpgsqlDbType { get; set; }

        public object ObjectValue { get; set; }
    }
}
