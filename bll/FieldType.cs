using System;
using System.Collections.Generic;
using System.Data;
using Quantumart.QP8.BLL.Repository;

namespace Quantumart.QP8.BLL
{
    public class FieldType
    {
        private static readonly Lazy<IEnumerable<FieldType>> allFieldTypes = new Lazy<IEnumerable<FieldType>>(FieldRepository.GetAllFieldTypes, true);

        public int Id { get; set; }

        public string Name { get; set; }

        public string DatabaseType { get; set; }

        public string Icon { get; set; }

        public DbType DbType
        {
            get
            {
                if (DatabaseType == "NUMERIC")
                {
                    return DbType.Decimal;
                }

                if (DatabaseType == "NTEXT")
                {
                    return DbType.String;
                }

                if (DatabaseType == "DATETIME")
                {
                    return DbType.DateTime;
                }

                return DbType.String;
            }
        }

        public static IEnumerable<FieldType> AllFieldTypes => allFieldTypes.Value;
    }
}
