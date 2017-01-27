using System;
using System.Diagnostics.CodeAnalysis;
using Quantumart.QPublishing.Database;

namespace Quantumart.QPublishing.Helpers
{
    public class Workflow
    {
        public static bool DbContentItemHasOwnWorkflow(int itemId)
        {
            var conn = new DBConnector();
            var strSql = $"SELECT * FROM article_workflow_bind WITH(NOLOCK) WHERE content_item_id = {itemId}";
            var dt = conn.GetCachedData(strSql);
            return dt.Rows.Count > 0;
        }

        public static bool ContentItemHasOwnWorkflow(int itemId)
        {
            return DbContentItemHasOwnWorkflow(itemId);
        }

        [SuppressMessage("ReSharper", "RedundantAssignment")]
        public static bool DbWillContentItemStatusBeDecreased(int siteId, int itemId, int userId, int currentStatusWeight, ref int toStatusId, ref int toStatusWeight)
        {
            var functionReturnValue = false;
            toStatusId = DbGetIdByWeight(currentStatusWeight, siteId);
            toStatusWeight = currentStatusWeight;

            var workflowId = DbGetWorkflowId(itemId);
            if (workflowId != 0)
            {
                var userWeight = DbGetUserWeight(userId, workflowId);
                if (userWeight != 0)
                {
                    toStatusWeight = userWeight;
                    toStatusId = DbGetIdByWeight(toStatusWeight, siteId);
                    functionReturnValue = toStatusWeight < currentStatusWeight;
                }
            }

            return functionReturnValue;
        }

        public static int DbGetWorkflowId(int contentItemId)
        {
            var strSql = "select workflow_id from content_item_workflow WITH(NOLOCK) where content_item_id = " + contentItemId;
            var conn = new DBConnector();
            var dt = conn.GetCachedData(strSql);
            if (dt.Rows.Count > 0 && !ReferenceEquals(dt.Rows[0]["workflow_id"], DBNull.Value))
            {
                return DBConnector.GetNumInt(dt.Rows[0]["workflow_id"]);
            }

            return 0;
        }

        public static int DbGetUserWeight(int userId, int workflowId)
        {
            int functionReturnValue;

            var strSql = "SELECT max(st.weight) as max_weight FROM workflow_rules wr WITH(NOLOCK) " + "INNER JOIN status_type st WITH(NOLOCK) ON wr.successor_status_id = st.status_type_id " + "WHERE wr.workflow_id = " + workflowId + " and wr.user_id = " + userId;

            var strAltSql = "SELECT max(st.weight) as max_weight FROM workflow_rules wr WITH(NOLOCK) " + "INNER JOIN status_type st WITH(NOLOCK) ON wr.successor_status_id = st.status_type_id " + "WHERE wr.workflow_id = " + workflowId + " and wr.group_id in (SELECT group_id FROM user_group_bind WHERE user_id = " + userId + ")";

            var conn = new DBConnector();
            var dt = conn.GetCachedData(strSql);

            if (dt.Rows.Count > 0)
            {
                if (ReferenceEquals(dt.Rows[0]["max_weight"], DBNull.Value))
                {
                    dt = conn.GetCachedData(strAltSql);
                    functionReturnValue = ReferenceEquals(dt.Rows[0]["max_weight"], DBNull.Value) ? 0 : DBConnector.GetNumInt(dt.Rows[0]["max_weight"]);
                }
                else
                {
                    functionReturnValue = DBConnector.GetNumInt(dt.Rows[0]["max_weight"]);
                }
            }
            else
            {
                functionReturnValue = DBConnector.GetNumInt(dt.Rows[0]["max_weight"]);
            }
            return functionReturnValue;
        }

        public static int DbGetIdByWeight(int weight, int siteId)
        {
            var strSql = "select status_type_id from status_type WITH(NOLOCK) where weight = " + weight + " and site_id = " + siteId;
            var conn = new DBConnector();
            var dt = conn.GetCachedData(strSql);
            return dt.Rows.Count > 0 ? DBConnector.GetNumInt(dt.Rows[0]["status_type_id"]) : 0;
        }

        public static int GetContentWorkflowId(int contentId)
        {
            return DbGetContentWorkflowId(contentId);
        }

        public static int DbGetContentWorkflowId(int contentId)
        {
            var strSql = "select workflow_id from content_workflow_bind cwb WITH(NOLOCK) where content_id = " + contentId;
            var conn = new DBConnector();
            var dt = conn.GetCachedData(strSql);
            if (dt.Rows.Count > 0)
            {
                return DBConnector.GetNumInt(dt.Rows[0]["workflow_id"]);
            }

            return 0;
        }

        public static int GetNoneId(int siteId)
        {
            return GetIdByWeight(GetNoneWeight(siteId), siteId);
        }

        public static int GetIdByWeight(int weight, int siteId)
        {
            return DbGetIdByWeight(weight, siteId);
        }

        public static int GetNoneWeight(int siteId)
        {
            var strSql = "select min(weight) as min_weight from status_type WITH(NOLOCK) where site_id = " + siteId;
            var conn = new DBConnector();
            var dt = conn.GetCachedData(strSql);
            if (dt.Rows.Count > 0)
            {
                return DBConnector.GetNumInt(dt.Rows[0]["min_weight"]);
            }

            return 0;
        }

        public static int GetWorkflowMaxWeight(decimal workflowId)
        {
            var strSql = " select max(st.weight) as max_weight from workflow_rules wr WITH(NOLOCK) " +
                         " inner join status_type st on wr.successor_status_id = st.status_type_id " +
                         $" where wr.workflow_id = {workflowId}";
            var conn = new DBConnector();
            var dt = conn.GetCachedData(strSql);
            return dt.Rows.Count != 0 ? DBConnector.GetNumInt(dt.Rows[0]["max_weight"]) : 0;

        }

        public static decimal GetPublishedId(decimal siteId)
        {
            return GetIdByWeight((int)GetPublishedWeight(siteId), (int)siteId);
        }

        public static decimal GetPublishedWeight(decimal siteId)
        {
            var strSql = $"select max(weight) as max_weight from status_type WITH(NOLOCK) where site_id = {siteId}";
            var conn = new DBConnector();
            var dt = conn.GetCachedData(strSql);
            if (dt.Rows.Count != 0)
            {
                return (decimal)dt.Rows[0]["max_weight"];
            }

            return 0;
        }
    }
}
