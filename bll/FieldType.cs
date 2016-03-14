using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using Quantumart.QP8.Constants;
using Quantumart.QP8.BLL.Repository;


namespace Quantumart.QP8.BLL
{
    public class FieldType
    {

        private static Lazy<IEnumerable<FieldType>> allFieldTypes = new Lazy<IEnumerable<FieldType>>(FieldRepository.GetAllFieldTypes, true);	

        public int Id
        {
            get;
            set;
        }

        public string Name
        {
            get;
            set;
        }

        public string DatabaseType
        {
            get;
            set;
        }

		public string Icon { get; set; }

        public DbType DbType
        {
            get
            {
                if (DatabaseType == "NUMERIC")
                {
                    return DbType.Decimal;
                }
                else if (DatabaseType == "NTEXT")
                {
                    return DbType.String;
                }
                else if (DatabaseType == "DATETIME")
                {
                    return DbType.DateTime;
                }
                else
                {
                    return DbType.String;
                }

            }
        }

        public static IEnumerable<FieldType> AllFieldTypes
        {
            get 
            {
                return allFieldTypes.Value;
            }
        }
    }


}
