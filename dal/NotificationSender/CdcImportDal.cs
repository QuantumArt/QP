using System.Data;
using System.Data.SqlClient;

namespace Quantumart.QP8.DAL.NotificationSender
{
    public class CdcImportDal
    {
        private static void ExecuteIdsQuery(SqlConnection connection)
        {
            //using (var cmd = SqlCommandFactory.Create(query, connection))
            //{
            //    cmd.CommandType = CommandType.Text;
            //    var idsTable = Common.IdsToDataTable(ids);
            //    var parameter = cmd.Parameters.AddWithValue("@ids", idsTable);
            //    parameter.SqlDbType = SqlDbType.Structured;
            //    parameter.TypeName = "dbo.Ids";
            //    cmd.ExecuteNonQuery();
            //}

            using (var cmd = SqlCommandFactory.Create("qp_get_default_article", connection))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@content_id", contentId);
                var ds = new DataSet();
                new SqlDataAdapter(cmd).Fill(ds);
                return 0 == ds.Tables.Count || 0 == ds.Tables[0].Rows.Count ? null : ds.Tables[0].Rows[0];
            }
        }
    }
}
