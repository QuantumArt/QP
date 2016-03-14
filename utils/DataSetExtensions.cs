using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Web.Script.Serialization;

namespace Quantumart.QP8.Utils
{
	public static class DataSetExtensions
	{
		public static SimpleDataRow ToSimpleDataRow(this DataRow dr)
		{
			SimpleDataRow sr = new SimpleDataRow();

			if (dr != null)
			{
				DataTable dt = dr.Table;
				if (dt.Columns.Count > 0)
				{
					sr = new SimpleDataRow();

					foreach (DataColumn dc in dt.Columns)
					{
						string columnName = dc.ColumnName;
						object columnValue = dr[columnName];

						sr.Add(columnName, columnValue);
					}
				}
			}

			return sr;
		}

		public static List<SimpleDataRow> ToSimpleDataRowList(this DataSet ds)
		{
			List<SimpleDataRow> list = new List<SimpleDataRow>();

			if (ds != null && ds.Tables.Count > 0)
			{
				list = ToSimpleDataRowList(ds.Tables[0]);
			}

			return list;
		}

		public static List<SimpleDataRow> ToSimpleDataRowList(this DataTable dt)
		{
			List<SimpleDataRow> list = new List<SimpleDataRow>();

			if (dt != null && dt.Rows.Count > 0)
			{
				foreach (DataRow dr in dt.Rows)
				{
					list.Add(dr.ToSimpleDataRow());
				}
			}

			return list;
		}
	}

	public class SimpleDataRow : Dictionary<string, object>
	{
	}
}
