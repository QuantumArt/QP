using System;
using System.Collections;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Net.Mail;
using System.Web.Caching;
using System.Globalization;
using System.Reflection;
using System.Xml;
using System.Linq;
using Microsoft.Win32;
using Microsoft.VisualBasic;
using Quantumart.QPublishing.Helpers;

using QA_Assembling;
using System.Xml.Linq;
using System.Data.SqlTypes;


namespace Quantumart.QPublishing.Database
{

    public partial class DBConnector
    {

		[Obsolete]
        public string GetContentItemLinkQuery(int site_id, string linkFieldName, string itemID)
        {
            return GetContentItemLinkQuery(linkFieldName, itemID);
        }

        [Obsolete]
        public DataColumnCollection GetContentFields(int content_id)
        {
            return GetCachedData("select TOP 1 * from CONTENT_" + content_id + " WITH(NOLOCK)").Columns;
        }

        [Obsolete]
        public string GetContentItemLinksFilter(int site_id, int itemID, string contentName)
        {
            string name = contentName.Replace("'", "''");
            DataTable dt;
            int contentID = 0;
            string returnString = null;

            dt = GetCachedData("SELECT * FROM content WHERE site_id= " + site_id + " AND content_name=N'" + name + "'");

            if (dt.Rows.Count == 0)
            {
                returnString = "0";
            }
            else
            {
                contentID = DBConnector.GetNumInt(dt.Rows[0]["content_id"]);

                dt = GetCachedData("SELECT link.* FROM item_link AS link LEFT OUTER JOIN content_item AS item ON item.content_item_id = link.linked_item_id WHERE link.item_id = " + itemID + " AND item.content_id=" + contentID);
                returnString = "0";

                foreach (DataRow dr in dt.Rows)
                {
                    returnString += "," + dr["linked_item_id"].ToString();
                }
            }

            return returnString;
        }

        [Obsolete]
        public string GetContentItemLinkQuery(int site_id, string linkFieldName, long itemID)
        {
            return GetContentItemLinkQuery(linkFieldName, itemID);
        }

        [Obsolete]
        public string GetContentItemLinkIDs(int site_id, string linkFieldName, string itemID)
        {
            return GetContentItemLinkIDs(linkFieldName, itemID);
        }


        [Obsolete]
        public string GetContentItemLinkIDsInternal(int site_id, string linkFieldName, string itemID)
        {
            return GetRealContentItemLinkIDs(linkFieldName, itemID);
        }

        [Obsolete]
        public string GetRealContentItemLinkIDs(int site_id, string linkFieldName, string itemID)
        {
            return GetRealContentItemLinkIDs(linkFieldName, itemID);
        }

        [Obsolete]
        public string GetContentItemLinkIDs(int site_id, string linkFieldName, long itemID)
        {
            return GetContentItemLinkIDs(linkFieldName, itemID);
        }

        [Obsolete]
        public string GetContentItemLinkQueries(int site_id, string linkFieldName, string itemID)
        {
            return GetContentItemLinkQueries(linkFieldName, itemID);
        }

        [Obsolete]
        public string GetContentItemLinkQueries(string linkFieldName, string itemId)
        {
            int linkID = GetLinkIDForItem(linkFieldName, itemId);

            if (linkID != LegacyNotFound)
            {
                StringBuilder sb = new StringBuilder();
                string table = (IsStage) ? "item_link_united" : "item_link";
                foreach (string id in itemId.Split(','))
                {
                    sb.AppendFormat("EXEC sp_executesql N'SELECT linked_item_id FROM {2} WHERE item_id = @itemId AND link_id = @linkId', N'@itemId NUMERIC, @linkId NUMERIC', @itemId = {0}, @linkId = {1};", Int64.Parse(id), linkID, table);
                }
                return sb.ToString();
            }
            else
            {
                return string.Empty;
            }
        }

        [Obsolete]
        public string GetContentUploadURL(int site_id, string content_name)
        {
            return GetContentUploadUrl(site_id, content_name);
        }

        [Obsolete("Use GetUploadDir or GetSiteLibraryDirectory instead")]
        public string GetImagesUploadDir(int site_id)
        {
            return GetUploadDir(site_id);
        }

        [Obsolete("Use overloded version without site_id")]
        public string GetContentFieldValue(int site_id, int itemID, string fieldName)
        {
            return GetContentFieldValue(itemID, fieldName);
        }

        [Obsolete]
        public void SetContentSecurityParamsForCommand(SqlCommand cmd, long lngUserID, long lngGroupID, int intStartLevel, int intEndLevel)
        {
            SetContentSecurityParamsForCommand(cmd, lngUserID, lngGroupID, intStartLevel, intEndLevel, true);
        }

        [Obsolete]
        public void SetContentSecurityParamsForCommand(SqlCommand cmd, long lngUserID, long lngGroupID, int intStartLevel, int intEndLevel, bool blnFilterRecords)
        {

            {
                cmd.Parameters.Add("@use_security", SqlDbType.Bit, 1);
                cmd.Parameters.Add("@user_id", SqlDbType.Decimal);
                cmd.Parameters.Add("@group_id", SqlDbType.Decimal);
                cmd.Parameters.Add("@start_level", SqlDbType.Int);
                cmd.Parameters.Add("@end_level", SqlDbType.Int);
                cmd.Parameters.Add("@filter_records", SqlDbType.Bit, 1);

                cmd.Parameters["@use_security"].Value = true;
                cmd.Parameters["@user_id"].Value = lngUserID;
                cmd.Parameters["@group_id"].Value = lngGroupID;
                cmd.Parameters["@start_level"].Value = intStartLevel;
                cmd.Parameters["@end_level"].Value = intEndLevel;
                cmd.Parameters["@filter_records"].Value = blnFilterRecords;
            }
        }

        [Obsolete]
        public DataTable GetControlDataTable(string siteName, string templateName, string pageName, string objectName, Hashtable values)
        {

            var outSelect = string.Empty;
            var outFrom = string.Empty;
            var outWhere = string.Empty;
            var outOrderBy = string.Empty;
            long outStartRow = 0;
            long outPageSize = 0;
            byte outGetCount = 0;
            long pOutTotalRecords = 0;

            var dt = GetContainerData(siteName, templateName, pageName, objectName);
            if ((dt.Rows.Count == 0))
            {
                return new DataTable();
            }

            GetContainerSQLPieces(dt, ref outSelect, ref outFrom, ref outWhere, ref outOrderBy, ref outStartRow, ref outPageSize, ref outGetCount, values);



            return GetPageData(outSelect, outFrom, outWhere, outOrderBy, outStartRow, outPageSize, outGetCount, out pOutTotalRecords);
        }


        [Obsolete]
        public DataTable GetContainerData(string siteName, string templateName, string pageName, string objectName)
        {

            var selectClause = string.Empty;
            if ((pageName == string.Empty))
            {

                selectClause = " select c.*, s.site_id from container as c " + " inner join object as o on c.object_id = o.object_id " + " inner join page_template as pt on o.page_template_id = pt.page_template_id " + " inner join site as s on s.site_id = pt.site_id " + " where " + " o.object_name = '{0}' " + " and o.page_id is null " + " and pt.template_name = '{1}' " + " and s.site_name = '{2}'";

                selectClause = string.Format(selectClause, objectName, templateName, siteName);
            }
            else
            {

                selectClause = " select c.*, s.site_id from container as c " + " inner join object as o on c.object_id = o.object_id " + " inner join page as p on o.page_id = p.page_id " + " inner join page_template as pt on p.page_template_id = pt.page_template_id " + " inner join site as s on s.site_id = pt.site_id " + " where " + " o.object_name = '{0}' " + " and p.page_name = '{1}' " + " and pt.template_name = '{2}' " + " and s.site_name = '{3}'";
                selectClause = string.Format(selectClause, objectName, pageName, templateName, siteName);
            }
            return GetCachedData(selectClause);
        }


        [Obsolete]
        public void GetContainerSQLPieces(DataTable containerData, ref string outSelect, ref string outFrom, ref string outWhere, ref string outOrderBy, ref long outStartRow, ref long outPageSize, ref byte outGetCount, Hashtable values)
        {

            string arcFilter = "", visiFilter = "", statusFilter = "";
            var randomPos = false;
            decimal start = 0, recCnt = 0;
            string sqlSource = "", sqlWhere = "", sqlOrder = "";

            GetContainerCommonParameters(containerData, ref visiFilter, ref arcFilter, ref statusFilter, ref sqlSource, ref randomPos, ref start, ref recCnt);

            sqlWhere = visiFilter + " " + statusFilter + " " + arcFilter;
            if (ReferenceEquals(containerData.Rows[0]["FILTER_VALUE"], DBNull.Value))
            {
                sqlWhere = sqlWhere + GetContainerFilterExpression("");
            }
            else
            {
                //sqlWhere = sqlWhere && GetContainerFilterExpression(CType(containerData.Rows[0]("FILTER_VALUE"), String))
                sqlWhere = sqlWhere + GetContainerFilterExpression(ParseValues(containerData.Rows[0]["FILTER_VALUE"].ToString(), values));
            }

            if (!randomPos)
            {

                var orderDynamic = string.Empty;
                var orderStatic = string.Empty;

                if ((!ReferenceEquals(containerData.Rows[0]["ALLOW_ORDER_DYNAMIC"], DBNull.Value)) && (decimal)containerData.Rows[0]["ALLOW_ORDER_DYNAMIC"] == 1)
                {
                    //orderDynamic = CType(containerData.Rows[0]("ORDER_DYNAMIC"), String)
                    orderDynamic = ParseValues(containerData.Rows[0]["ORDER_DYNAMIC"].ToString(), values);
                }

                if ((!ReferenceEquals(containerData.Rows[0]["ORDER_STATIC"], DBNull.Value)))
                {
                    orderStatic = containerData.Rows[0]["ORDER_STATIC"].ToString();
                }


                sqlOrder = GetContainerOrderExpression(orderStatic, orderDynamic);
            }
            else
            {
                sqlOrder = "NewId()";
            }

            outSelect = "c.*";
            outFrom = sqlSource + " AS c WITH (NOLOCK)";
            outWhere = sqlWhere;
            outOrderBy = sqlOrder;
            outStartRow = (long)start;
            outGetCount = 1;

            if ((recCnt > 0))
            {
                outPageSize = (long)recCnt;
            }
            else
            {
                outPageSize = 0;
            }

            if (randomPos)
            {
                outStartRow = 1;
            }

            if (outStartRow == 1 && (outPageSize == 1 || outPageSize == 0))
            {
                outGetCount = 0;

            }
        }

        [Obsolete]
        public void GetContainerCommonParameters(DataTable containerData, ref string visiFilter, ref string arcFilter, ref string statusFilter, ref string sqlSource, ref bool randomPos, ref decimal start, ref decimal recCnt)
        {

            decimal extSiteId = 0;
            var objectId = (decimal)containerData.Rows[0]["SITE_ID"];
            var contentId = (decimal)containerData.Rows[0]["CONTENT_ID"];
            var siteId = (decimal)containerData.Rows[0]["SITE_ID"];

            //Get Container Statuses
            var containerStatuses = Status.GetContainerStatuses(objectId);
            var arrStatuses = new StringBuilder(string.Empty);
            foreach (long statusTypeId in containerStatuses.Keys)
            {
                arrStatuses.Append(statusTypeId + ",");
            }
            if ((arrStatuses.ToString() != string.Empty))
            {
                arrStatuses.Remove(arrStatuses.Length - 1, 1);
            }
            var strStatuses = arrStatuses.ToString();

            //if content is from another site
            var dt = GetCachedData($"SELECT site_id FROM content WHERE contentId={contentId} and site_id <> {siteId}");
            if (dt.Rows.Count != 0) extSiteId = (decimal)dt.Rows[0]["site_id"];

            //Check for max status in Workflow
            decimal workflowId = Workflow.GetContentWorkflowId((int)contentId);
            var workflowAssigned = (workflowId != null);
            var maxStatusId = default(decimal);
            if (workflowAssigned && !string.IsNullOrEmpty(strStatuses))
            {
                maxStatusId = Workflow.GetIdByWeight(Workflow.GetWorkflowMaxWeight(workflowId), (int)siteId);
            }
            else if (extSiteId != 0)
            {
                maxStatusId = Workflow.GetPublishedId(extSiteId);
            }
            else
            {
                maxStatusId = Workflow.GetPublishedId(siteId);
            }

            var containsMaxStatus = containerStatuses.ContainsKey(maxStatusId) || containerStatuses.Count == 0;

            //Get Container Parameters
            if (ReferenceEquals(containerData.Rows[0]["SELECT_START"], DBNull.Value))
            {
                start = 1;
            }
            else
            {
                start = (decimal)containerData.Rows[0]["SELECT_START"];
            }

            var strSql = $"select * from content where contentId = {contentId}";
            var contentData = GetCachedData(strSql);

            if (ReferenceEquals(containerData.Rows[0]["SELECT_TOTAL"], DBNull.Value))
            {
                recCnt = 0;
            }
            else
            {
                recCnt = (decimal)containerData.Rows[0]["SELECT_TOTAL"];
            }

            var scheduleDependent = (decimal)containerData.Rows[0]["SCHEDULE_DEPENDENCE"];
            randomPos = ((decimal)containerData.Rows[0]["ROTATE_CONTENT"] > 0);
            var showArchived = GetNumBool(containerData.Rows[0]["SHOW_ARCHIVED"]);
            var virtualType = (decimal)contentData.Rows[0]["VIRTUAL_TYPE"];

            if (scheduleDependent == 0)
            {
                visiFilter = "c.visible IS NOT NULL";
            }
            else
            {
                visiFilter = "c.visible = 1";
            }

            if (showArchived)
            {
                arcFilter = "";
            }
            else
            {
                arcFilter = "and c.archive = 0";
            }

            if (containsMaxStatus || virtualType == 3)
            {
                sqlSource = "CONTENT_" + contentId.ToString();
            }
            else
            {
                var dtsys = GetCachedData(
                    $"SELECT count(*) as cnt FROM sysobjects WHERE id = OBJECT_ID('CONTENT_{contentId}_UNITED') AND OBJECTPROPERTY(id, N'IsView') = 1");
                if (dtsys.Rows.Count > 0)
                {
                    if ((long)dtsys.Rows[0]["cnt"] > 0)
                    {
                        sqlSource = "CONTENT_" + contentId.ToString() + "_UNITED";
                    }
                    else
                    {
                        sqlSource = "CONTENT_" + contentId.ToString();
                    }
                }
            }

            if (workflowAssigned && !string.IsNullOrEmpty(strStatuses))
            {
                statusFilter = "AND c.status_type_id in (" + strStatuses + ")";
            }
            else
            {
                statusFilter = "AND c.status_type_id = " + maxStatusId;

            }
        }

        [Obsolete]
        public string ParseValues(string text, Hashtable values)
        {

            var pattrn = "((\"?\\s*(\\+|\\&)?)\\s*(Value[s]?|NumValue|StrValue)\\s*\\(\\s*\"(?<key>[^\"]+)\"\\s*\\)\\s*(\\+|\\&)?(\\s*)(\"?))";
            var regEx = new Regex(pattrn, RegexOptions.IgnoreCase | RegexOptions.Multiline);
            var result = new StringBuilder();

            foreach (Match match in regEx.Matches(text))
            {
                var curKey = match.Groups["key"].ToString();
                var expr = match.ToString();
                if (values.ContainsKey(curKey))
                {
                    text = text.Replace(expr, values[curKey].ToString());
                }
            }


            return text.Trim().Trim("\"".ToCharArray());
        }

        [Obsolete]
        public string GetContainerFilterExpression(string filterSQL)
        {
            return QPageEssential.GetContainerFilterExpression(filterSQL);
        }

        [Obsolete]
        public string GetContainerOrderExpression(string staticSQL, string dynamicSQL)
        {
            return QPageEssential.GetContainerOrderExpression(staticSQL, dynamicSQL);
        }

        [Obsolete]
        public string GetContainerOrderExpression(string staticSQL, string dynamicSQL, bool apply_security)
        {
            return QPageEssential.GetContainerOrderExpression(staticSQL, dynamicSQL, apply_security);
        }

        [Obsolete]
        public string GetContainerOrderExpression(string staticSQL, string dynamicSQL, bool apply_security, bool addOrderBy)
        {
            return QPageEssential.GetContainerOrderExpression(staticSQL, dynamicSQL, apply_security, addOrderBy);
        }

        


    }
}
