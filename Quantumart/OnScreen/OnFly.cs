using System;
using System.Data;
using System.Globalization;
using System.Web;
using Microsoft.VisualBasic;
using Quantumart.QPublishing.Database;
using Quantumart.QPublishing.Helpers;
using Quantumart.QPublishing.Info;

namespace Quantumart.QPublishing.OnScreen
{
    
    public class OnFly
    {
        
        private static int _defaultLanguageId = 1;
        
        private static long GetUid()
        {
            var obj = HttpContext.Current.Session["UID"];
            long result;
            if ((obj != null) && long.TryParse(obj.ToString(), out result)) {
                return result;
            }
            else {
                return 0;
            }
        }
        
        private static string UnauthorizedMessage()
        {
            return TranslateManager.Translate("You are not authorized as QP7 user");
        }
        
        public static string DecreaseStatus(int itemId)
        {
            
            string functionReturnValue;
            int contentId;
            int currentStatusId;
            var ownWorkflow = false;
            var decreaseToStatusId = 0;
            var decreaseToStatusWeight = 0;
            
            var uid = (int)GetUid();
            if (uid == 0) return UnauthorizedMessage(); 
            
            string sql = $" SELECT c.site_id, i.content_id, i.status_type_id, st.weight FROM content_item AS i LEFT OUTER JOIN content AS c ON c.content_id = i.content_id LEFT OUTER JOIN status_type AS st ON st.status_type_id= i.status_type_id WHERE i.content_item_id = {itemId}";
            var conn = new DBConnector();
            var dt = conn.GetRealData(sql);
            
            if (dt.Rows.Count > 0) {
                var siteId = DBConnector.GetNumInt(dt.Rows[0]["site_id"]);
                var currentStatusWeight = DBConnector.GetNumInt(dt.Rows[0]["weight"]);
                var decrease = Workflow.DbWillContentItemStatusBeDecreased(siteId, itemId, uid, currentStatusWeight, ref decreaseToStatusId, ref decreaseToStatusWeight);
                functionReturnValue = decrease ? TranslateManager.Translate(@"While article updating status will be decreased. Click ""OK"" to proceed") : "0";
            }
            else {
                functionReturnValue = "0";
            }
            return functionReturnValue;
        }
        
        public static string UpdateArticle(int itemId, string attrName, string uploadUrl, string siteUrl, string attrValue)
        {
            var uid = (int)GetUid();
            if (uid == 0) return UnauthorizedMessage(); 
            
            if (QScreen.DbGetUserAccess("content_item", itemId, uid) < 3) {
                return TranslateManager.Translate("You have no enough permissions to update");
             }
            
            var lockUid = GetLockRecordUid(itemId);
            if ((lockUid != 0) && (lockUid != uid)) {
                return string.Format(TranslateManager.Translate("Cannot update article because it is locked by {0}"), GetUserName(lockUid));
            }
            

            string sql = $" SELECT a.attribute_id, t.type_name, t.database_type, a.required, c.site_id, i.content_id,  i.status_type_id, st.weight FROM content_item AS i LEFT OUTER JOIN content_attribute AS a ON a.content_id = i.content_id AND a.attribute_name = N'{attrName.Replace("'", "''")}' LEFT OUTER JOIN attribute_type AS t ON t.attribute_type_id = a.attribute_type_id LEFT OUTER JOIN content AS c ON c.content_id = i.content_id LEFT OUTER JOIN status_type AS st ON st.status_type_id= i.status_type_id WHERE i.content_item_id = {itemId}";
            
            var conn = new DBConnector();
            var dt = conn.GetRealData(sql);
            
            if (dt.Rows.Count > 0) {
                var siteId = DBConnector.GetNumInt(dt.Rows[0]["site_id"]);
                DBConnector.GetNumInt(dt.Rows[0]["content_id"]);
                var attrId = DBConnector.GetNumInt(dt.Rows[0]["attribute_id"]);
                var attrDbType = dt.Rows[0]["database_type"].ToString();
                var attrRequired = DBConnector.GetNumInt(dt.Rows[0]["required"]);
                var currentStatusId = DBConnector.GetNumInt(dt.Rows[0]["status_type_id"]);
                var currentStatusWeight = DBConnector.GetNumInt(dt.Rows[0]["weight"]);
                
                if ((attrRequired != 0) && string.IsNullOrEmpty(attrValue)) {
                    return TranslateManager.Translate("Field is required");
                }
                
                if (attrDbType == "DATETIME") {
                    if (!Information.IsDate(attrValue)) {
                        return TranslateManager.Translate("Value is not a date");
                     }
                    
                    var culture = new CultureInfo("ru-RU", true);
                        
                    attrValue = DateTime.ParseExact(attrValue, "yyyy-MM-dd HH:mm:ss", culture).ToString("yyyy-MM-dd HH:mm:ss");
                }
                
                // *************************************
                // *** Create Backup Copy of the Article
                // *************************************
                string backupSql = $" IF EXISTS(SELECT * FROM system_info WHERE field_name LIKE 'Version Control Add-on%') BEGIN EXEC create_content_item_version {uid}, {itemId} END";
                conn.ProcessData(backupSql);
                
                // ************
                // *** Workflow
                // ************
                var ownWorkflow = false;
                int decreaseToStatusId = 0, decreaseToStatusWeight = 0;
                var decreaseStatus = Workflow.DbWillContentItemStatusBeDecreased(siteId, itemId, uid, currentStatusWeight, ref decreaseToStatusId, ref decreaseToStatusWeight);
                var newStatus = decreaseStatus ? decreaseToStatusId : currentStatusId;
                
                conn.ProcessData(" UPDATE content_item SET" + " last_modified_by = " + uid + "," + " modified = GETDATE(), " + " status_type_id = " + newStatus + " WHERE content_item_id = " + itemId);

                string dataField;
                if (attrDbType == "NTEXT") {
                    // *** Upload URL is different for frontend and backend
                    // *** UploadURL = GetImagesUploadUrl(SiteID)
                    uploadUrl = HttpContext.Current.Server.UrlDecode(uploadUrl);
                    siteUrl = HttpContext.Current.Server.UrlDecode(siteUrl);
                    attrValue = attrValue.Replace(uploadUrl, "<" + "%=upload_url%" + ">");
                    attrValue = attrValue.Replace(siteUrl, "<" + "%=site_url%" + ">");
                    dataField = "BLOB_DATA";
                }
                else {
                    dataField = "DATA";
                }
                attrValue = attrValue.Replace("'", "''");
                sql =
                    $"UPDATE content_data SET {dataField} = N'{attrValue}' WHERE content_item_id = {itemId} AND attribute_id = {attrId}";
                conn.ProcessData(sql);
                
                if (decreaseStatus) {
                    //fill other fields
                    var updateSql = "update content_data set {0} = {1} where attribute_id = {2} and content_item_id = {3}";
                    sql =
                        $"select cd.attribute_id, cd.data, cd.blob_data, ca.attribute_name, at.database_type from content_data cd  join content_attribute ca on cd.attribute_id = ca.attribute_id  join attribute_type at on ca.attribute_type_id = at.attribute_type_id  where cd.content_item_id = {itemId} and cd.attribute_id <> {attrId}";
                    dt = conn.GetRealData(sql);
                    foreach (DataRow curRow in dt.Rows) {
                        string dataType = curRow["database_type"] == "NTEXT" ? "BLOB_DATA" : "DATA";
                        var dataObj = curRow[dataType];
                        var dataValue = ReferenceEquals(dataObj, DBNull.Value) ? "NULL" : $"'{dataObj}'";
                        
                        conn.ProcessData(string.Format(updateSql, dataType, dataValue, curRow["attribute_id"], itemId));
                    }
                }
                
                if (decreaseToStatusWeight > currentStatusWeight) {
                    return "1|" + TranslateManager.Translate(@"Your changes were saved, but the required workflow was not applied. To apply, click ""Edit in Form View"" and follow the instructions on the screen");
                }
            }
            return "1";
        }
        
        public static string CreateLikeArticle(int itemId, int contentId, int siteId)
        {
            var uid = GetUid();
            if (uid == 0) return UnauthorizedMessage();

            if (QScreen.DbGetUserAccess("content", contentId, (int)uid) < 3)
            {
                return TranslateManager.Translate("You have no enough permissions to clone");
            }
            
            var createdId = 0;
            var cloneResult = CloneArticle(itemId, siteId, (int)uid, ref createdId);
            
            if (cloneResult != "1") {
                return "0|" + cloneResult;
            }
            else {
                var conn = new DBConnector();
                conn.SendNotification(siteId, NotificationEvent.Create, createdId, "", false);
                return "1|" + createdId;
            }
        }
        
        private static string CloneArticle(int id, int siteId, int userId, ref int createdId)
        {
            string functionReturnValue = null;

            string strSql = $"SELECT CONTENT_ID, STATUS_TYPE_ID, VISIBLE, ARCHIVE FROM CONTENT_ITEM WHERE CONTENT_ITEM_ID = {id}";
            var conn = new DBConnector();
            var dt = conn.GetRealData(strSql);
            
            if (dt.Rows.Count > 0) {
                var statusTypeId = int.Parse(dt.Rows[0]["STATUS_TYPE_ID"].ToString());
                var contentId = int.Parse(dt.Rows[0]["CONTENT_ID"].ToString());
                var visible = int.Parse(dt.Rows[0]["VISIBLE"].ToString());
                var archive = int.Parse(dt.Rows[0]["ARCHIVE"].ToString());

                strSql = $"SELECT * FROM CONTENT_CONSTRAINT WHERE CONTENT_ID = {contentId}";
                dt = conn.GetRealData(strSql);

                if (dt.Rows.Count > 0)
                {
                    return TranslateManager.Translate("Cannot clone article because of constraint on the content");
                }
                else
                {

                    //set None status for article with workflow'
                    if (Workflow.ContentItemHasOwnWorkflow(id) || Workflow.GetContentWorkflowId(contentId) != 0)
                    {
                        statusTypeId = Workflow.GetNoneId(siteId);
                    }

                    strSql =
                        $"insert into content_item(CONTENT_ID, STATUS_TYPE_ID, VISIBLE, ARCHIVE, LAST_MODIFIED_BY) values({contentId}, {statusTypeId}, {visible}, {archive}, {userId})";
                    var newId = conn.InsertDataWithIdentity(strSql);
                    createdId = newId;

                    if (newId == 0)
                    {
                        return TranslateManager.Translate("Error while cloning article");
                    }
                    HttpContext.Current.Session["newCloneArticleID"] = newId;

                    //inserting other fields'
                    strSql =
                        $"SELECT AT.TYPE_NAME AS TYPE_NAME, AT.INPUT_TYPE AS INPUTTYPE,AT.DATABASE_TYPE, CA.* FROM ATTRIBUTE_TYPE AT, CONTENT_ATTRIBUTE CA WHERE AT.ATTRIBUTE_TYPE_ID=CA.ATTRIBUTE_TYPE_ID AND CA.CONTENT_ID = {contentId}";
                    dt = conn.GetRealData(strSql);

                    if (dt.Rows.Count > 0)
                    {

                        foreach (DataRow curRow in dt.Rows)
                        {
                            var typeName = curRow["TYPE_NAME"].ToString();
                            var attributeId = DBConnector.GetNumInt(curRow["ATTRIBUTE_ID"]);
                            var linkId = curRow["link_id"] == DBNull.Value ? 0 : DBConnector.GetNumInt(curRow["link_id"]);
                            //Return "OK"
                            strSql = "UPDATE cd SET cd.data = {0}, cd.blob_data = cd2.blob_data FROM content_data cd INNER JOIN content_data cd2 ON cd2.attribute_id = {1} AND cd2.content_item_id = {2} WHERE cd.attribute_id= {1} AND cd.content_item_id = {3}";

                            if (typeName != "Relation")
                            {
                                conn.ProcessData(string.Format(strSql, "cd2.data", attributeId, id, newId));
                            }
                            else if (linkId == 0)
                            {
                                string s2 = $"CASE cd2.data WHEN '{id}' THEN '{newId}' ELSE cd2.data END";
                                conn.ProcessData(string.Format(strSql, s2, attributeId, id, newId));
                            }
                            else
                            {
                                CloneManyToMany(conn, id, newId, linkId, linkId);
                            }

                        }
                    }

                    conn.ProcessData("INSERT INTO article_workflow_bind " + "SELECT " + newId + ", workflow_id, is_async " + "FROM article_workflow_bind " + "WHERE content_item_id =" + id);

                    AdjustManyToManySelfRelation(conn, id, newId);

                    functionReturnValue = "1";

                }
            }
            return functionReturnValue;
        }
        
        private static void CloneManyToMany(DBConnector cnn, int id, int newId, int linkId, int newLinkId)
        {
            var sql = "INSERT INTO item_to_item SELECT " + newLinkId + ", " + newId + ", r_item_id FROM item_to_item WHERE link_id = " + linkId + " AND l_item_id = " + id;
            cnn.ProcessData(sql);
            
            sql = "INSERT INTO item_to_item SELECT " + newLinkId + ", l_item_id, " + newId + " FROM item_to_item WHERE link_id = " + linkId + " AND r_item_id = " + id + " AND l_item_id <> " + newId + " AND l_item_id <> " + id;
                
            cnn.ProcessData(sql);
        }
        
        private static void AdjustManyToManySelfRelation(DBConnector cnn, int id, int newId)
        {
            string sql = "update item_to_item set l_item_id = " + newId + " where l_item_id = " + id + " and r_item_id = " + newId + "";
            cnn.ProcessData(sql);
            
            sql = "delete from item_to_item where r_item_id = " + id + " and l_item_id = " + newId + "";
            cnn.ProcessData(sql);
        }
        
        private static int GetLockRecordUid(int itemId)
        {
            var res = 0;
            var strSql = "SELECT locked_by FROM content_item WHERE content_item_id=" + itemId;
            var conn = new DBConnector();
            var dt = conn.GetRealData(strSql);
            
            if (dt.Rows.Count > 0) {
                if (!ReferenceEquals(dt.Rows[0]["locked_by"], DBNull.Value)) {
                    res = DBConnector.GetNumInt(dt.Rows[0]["locked_by"]);
                }
            }
            return res;
        }
        
        private static string GetUserName(int uid)
        {
            var strSql = "SELECT * FROM users WHERE user_id=" + uid;
            var conn = new DBConnector();
            var dt = conn.GetCachedData(strSql);
            return dt.Rows.Count > 0 ? dt.Rows[0]["first_name"] + " " + dt.Rows[0]["last_name"] : "";
        }
        
    }
}
