using System.Collections;
using System.Data;
using System.Linq;
using System.Text;
using Quantumart.QPublishing.Database;

namespace Quantumart.QPublishing.Helpers
{
    public class Status
    {
        public static string GetPreviousStatus(int id)
        {
            var cnn = new DBConnector();
            var row = GetPreviousStatusHistoryRecord(id, cnn);
            return row?["status_type_name"].ToString() ?? "None";
        }

        internal static DataRow GetPreviousStatusHistoryRecord(int id, DBConnector cnn)
        {
            var strSql = string.Format("SELECT Top 1 * FROM CONTENT_ITEM_STATUS_HISTORY AS c WITH (NOLOCK) INNER JOIN status_type AS s WITH (NOLOCK) ON c.status_type_id = s.status_type_id WHERE content_item_id = {0} AND status_history_id NOT IN ( SELECT Top 1 status_history_id FROM CONTENT_ITEM_STATUS_HISTORY WITH (NOLOCK) WHERE content_item_id = {0} AND status_type_id is not null ORDER BY status_history_id DESC ) ORDER BY status_history_id DESC", id);
            var dt = cnn.GetRealData(strSql);
            return dt.Rows.Count > 0 ? dt.Rows[0] : null;
        }

        public static string GetStatusName(int id)
        {
            var strSql = "select status_type_name from status_type where status_type_id = " + id;

            var conn = new DBConnector();
            var dt = conn.GetCachedData(strSql);

            var functionReturnValue = dt.Rows.Count > 0 ? dt.Rows[0]["status_type_name"].ToString() : "";

            return functionReturnValue;
        }

        public static string GetLastModifiedLogin(int userId)
        {
            var strSql = "SELECT * from USERS WHERE USER_ID = " + userId;

            var conn = new DBConnector();
            var dt = conn.GetCachedData(strSql);

            if (dt.Rows.Count > 0)
            {
                return dt.Rows[0]["LOGIN"].ToString();
            }

            return "";
        }

        public static string GetUserName(int userId)
        {
            var strSql = "SELECT * from USERS WHERE USER_ID = " + userId;

            var conn = new DBConnector();
            var dt = conn.GetCachedData(strSql);

            if (dt.Rows.Count > 0)
            {
                return GetUserName(dt.Rows[0]);
            }

            return "";
        }
        public static string GetUserName(DataRow row)
        {
            return $"{row["first_name"]} {row["last_name"]}";
        }

        public static Hashtable GetContainerStatuses(decimal objectId)
        {
            var containerStatuses = new Hashtable();
            var conn = new DBConnector();

            var dtObjectParameters = conn.GetCachedData($"select * from container_statuses where object_id = {objectId}");
            foreach (DataRow dr in dtObjectParameters.Rows)
            {
                containerStatuses.Add((long)dr["status_type_id"], "1");
            }
            return containerStatuses;
        }

        public static string GetParallelApproved(int contentItemId, int statusTypeId)
        {
            var conn = new DBConnector();
            var workflowId = Workflow.DbGetWorkflowId(contentItemId);
            var sb = new StringBuilder();
            sb.Append("select u.* from users u inner join user_group_bind ug WITH(NOLOCK) on u.user_id = ug.user_id ");
            sb.Append(" inner join workflow_rules wr on ug.group_id = wr.group_id ");
            sb.Append(string.Format("where wr.workflow_id = {1} and wr.successor_status_id = {2} and ug.user_id not in (select user_id from waiting_for_approval WITH(NOLOCK) where content_item_id = {0})", contentItemId, workflowId, GetNextWorkflowStatus(workflowId, statusTypeId)));
            var dtApproving = conn.GetRealData(sb.ToString());
            return GetUserCommaList(dtApproving);
        }

        public static int GetNextWorkflowStatus(int workflowId, int statusTypeId)
        {
            var sb = new StringBuilder();
            sb.Append("SELECT st.status_type_id FROM workflow_rules wr WITH(NOLOCK) ");
            sb.Append("INNER JOIN status_type st WITH(NOLOCK) ON wr.successor_status_id = st.status_type_id ");
            sb.Append(
                $"WHERE wr.workflow_id = {workflowId} And st.weight > (select weight from status_type where status_type_id = {statusTypeId}) ORDER BY st.weight ");
            var conn = new DBConnector();
            var dtStatus = conn.GetRealData(sb.ToString());
            if (dtStatus.Rows.Count > 0)
            {
                return DBConnector.GetNumInt(dtStatus.Rows[0]["STATUS_TYPE_ID"]);
            }

            return 0;
        }

        public static string GetParallelWaitingForApproval(int contentItemId)
        {
            var conn = new DBConnector();
            var strSql = "select u.* from waiting_for_approval wa WITH(NOLOCK) inner join users u WITH(NOLOCK) on wa.user_id = u.user_id where content_item_id = " + contentItemId;
            var dtApproving = conn.GetRealData(strSql);
            return GetUserCommaList(dtApproving);
        }

        public static string GetUserCommaList(DataTable dt)
        {
            if (dt.Rows.Count == 0)
            {
                return "none";
            }

            return string.Join(", ", (from DataRow row in dt.Rows select $"<strong>{GetUserName(row)}</strong>").ToArray());
        }
    }
}
