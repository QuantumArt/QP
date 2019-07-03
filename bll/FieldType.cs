using System;
using System.Collections.Generic;
using System.Data;
using NpgsqlTypes;
using Quantumart.QP8.BLL.Repository.FieldRepositories;

namespace Quantumart.QP8.BLL
{
    public class FieldType
    {
        private static readonly Lazy<IEnumerable<FieldType>> AllFieldTypesLazy = new Lazy<IEnumerable<FieldType>>(FieldRepository.GetAllFieldTypes, true);

        public int Id { get; set; }

        public string Name { get; set; }

        public string DatabaseType { get; set; }

        public string Icon { get; set; }

        public DbType DbType
        {
            get
            {
                switch (DatabaseType)
                {
                    case "NUMERIC":
                        return DbType.Decimal;
                    case "NTEXT":
                        return DbType.String;
                    case "DATETIME":
                        return DbType.DateTime;
                }

                return DbType.String;
            }
        }

        public NpgsqlDbType NpgsqlDbType
        {
            get
            {
                switch (DatabaseType)
                {
                    case "NUMERIC":
                        return NpgsqlDbType.Numeric;
                    case "NTEXT":
                        return NpgsqlDbType.Text;
                    case "DATETIME":
                        return NpgsqlDbType.Timestamp;
                }

                return NpgsqlDbType.Text;
            }
        }



        public static IEnumerable<FieldType> AllFieldTypes => AllFieldTypesLazy.Value;
    }
}
