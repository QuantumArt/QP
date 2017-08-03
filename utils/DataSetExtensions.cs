using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Quantumart.QP8.Utils
{
    public static class DataSetExtensions
    {
        public static SimpleDataRow ToSimpleDataRow(this DataRow dr)
        {
            var sr = new SimpleDataRow();
            var dt = dr?.Table;
            if (dt?.Columns.Count > 0)
            {
                sr = new SimpleDataRow();
                foreach (DataColumn dc in dt.Columns)
                {
                    var columnName = dc.ColumnName;
                    var columnValue = dr[columnName];

                    sr.Add(columnName, columnValue);
                }
            }

            return sr;
        }

        public static List<SimpleDataRow> ToSimpleDataRowList(this DataSet ds)
        {
            var list = new List<SimpleDataRow>();
            if (ds != null && ds.Tables.Count > 0)
            {
                list = ToSimpleDataRowList(ds.Tables[0]);
            }

            return list;
        }

        public static List<SimpleDataRow> ToSimpleDataRowList(this DataTable dt)
        {
            var list = new List<SimpleDataRow>();
            if (dt != null && dt.Rows.Count > 0)
            {
                list.AddRange(from DataRow dr in dt.Rows select dr.ToSimpleDataRow());
            }

            return list;
        }
    }

    public class SimpleDataRow : Dictionary<string, object>
    {
    }
}
