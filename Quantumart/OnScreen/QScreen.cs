using System;
using System.Data;
using System.Web;
using Microsoft.VisualBasic;
using Quantumart.QPublishing.Database;
using Quantumart.QPublishing.Info;

namespace Quantumart.QPublishing.OnScreen
{
    public class QScreen
    {
        internal static readonly string AuthenticationKey = "QA.dll.CustomTabAuthUser";
        private DataView _sites;

        public int FieldBorderMode { get; set; }

        public int ObjectBorderMode { get; set; }

        public int ObjectBorderTypeMask { get; set; }

        public static string QpBackendUrl
        {
            get
            {
                var obj = HttpContext.Current.Session["qp_backend_url"];
                return obj?.ToString() ?? string.Empty;
            }
        }

        public int OnFlyObjCount { get; set; } = 0;

        internal void SetSiteBorderModes(Site site)
        {
            if (site != null)
            {
                FieldBorderMode = site.FieldBorderMode;
            }
        }

        internal static bool SessionEnabled()
        {
            return HttpContext.Current.Session != null;
        }

        private static void SaveInSession(string key, object value)
        {
            HttpContext.Current.Session[key] = value;
        }

        private static string GetQueryParameter(string key)
        {
            object obj = HttpContext.Current.Request[key];
            return obj?.ToString() ?? string.Empty;
        }

        public static int AuthenticateForCustomTab(DBConnector cnn)
        {
            var result = 0;
            var backendSid = GetQueryParameter("backend_sid").Replace("'", "''");
            if (!string.IsNullOrEmpty(backendSid))
            {
                string sql = $"EXEC sp_executesql N'SELECT user_id from sessions_log WHERE sid = @sid', N'@sid nvarchar(255)', @sid = '{backendSid}'";
                var dt = cnn.GetRealData(sql);
                if (dt.Rows.Count > 0)
                {
                    result = (int)(decimal)dt.Rows[0]["user_id"];
                    sql = $"EXEC sp_executesql N'UPDATE sessions_log SET sid = NULL WHERE sid = @sid', N'@sid nvarchar(255)', @sid = '{backendSid}'";
                    cnn.ProcessData(sql);
                }
            }

            return result;
        }

        public static int AuthenticateForCustomTab()
        {
            return AuthenticateForCustomTab(new DBConnector());
        }

        public static bool CheckCustomTabAuthentication(DBConnector cnn)
        {
            var backendSid = GetQueryParameter("backend_sid").Replace("'", "''");
            if (string.IsNullOrEmpty(backendSid))
            {
                return HttpContext.Current.Session[AuthenticationKey] != null;
            }

            var result = AuthenticateForCustomTab(cnn);
            if (result == 0)
            {
                return false;
            }

            HttpContext.Current.Session[AuthenticationKey] = result;
            return true;
        }

        public static bool CheckCustomTabAuthentication()
        {
            return CheckCustomTabAuthentication(new DBConnector());
        }

        public static int GetCustomTabUserId()
        {
            var result = 0;
            if (HttpContext.Current != null
                && HttpContext.Current.Session != null
                && HttpContext.Current.Session[AuthenticationKey] != null)
            {
                result = (int)HttpContext.Current.Session[AuthenticationKey];
            }
            return result;
        }

        internal void GetBackendAuthentication()
        {
            if (SessionEnabled())
            {
                var backendSid = GetQueryParameter("backend_sid");
                if (!string.IsNullOrEmpty(backendSid))
                {
                    backendSid = backendSid.Replace("'", "''");
                    var backendUrl = GetQueryParameter("qp_backend_url");
                    if (!string.IsNullOrEmpty(backendUrl))
                    {
                        SaveInSession("qp_backend_url", backendUrl);
                        string sql = $" SELECT s.*, u.language_id, u.allow_stage_edit_object, u.allow_stage_edit_field FROM sessions_log AS s LEFT OUTER JOIN users AS u ON u.user_id = s.user_id WHERE sid='{backendSid}'";
                        var cnn = new DBConnector();

                        var dt = cnn.GetRealData(sql);
                        if (dt.Rows.Count > 0)
                        {

                            var row = dt.Rows[0];
                            SaveInSession("uid", int.Parse(row["user_id"].ToString()));
                            SaveInSession("allow_stage_edit_field", int.Parse(row["allow_stage_edit_field"].ToString()));
                            SaveInSession("allow_stage_edit_object", int.Parse(row["allow_stage_edit_object"].ToString()));
                            SaveInSession("CurrentLanguageID", int.Parse(row["language_id"].ToString()));
                            cnn.ProcessData($"UPDATE sessions_log SET sid=NULL WHERE sid='{backendSid}'");
                        }
                    }
                }
            }
        }

        public string GetSiteDns(int siteId)
        {
            var conn = new DBConnector();
            return "http://" + conn.GetDns(siteId, false);
        }

        public string GetObjectStageRedirectHref(string redirect, int templateId, int pageId, int objectId, int formatId)
        {
            var retUrl = HttpContext.Current.Request.Url.Query;
            return QpBackendUrl + "?redirect=" + redirect + "&page_template_id=" + templateId + "&page_id=" + pageId + "&object_id=" + objectId + "&format_id=" + formatId + "&ret_stage_url=" + RemoveIisErrorCode(retUrl);
        }

        public int GetObjectTypeIdByObjectId(int objectId)
        {

            var strSql = "select o.object_type_id from object as o where o.object_id=" + objectId;
            var conn = new DBConnector();
            var dt = conn.GetCachedData(strSql);
            return dt.Rows.Count > 0 ? DBConnector.GetNumInt(dt.Rows[0]["object_type_id"]) : 0;
        }

        public int GetAllowStageEditByObjectId(int objectId)
        {

            var strSql = "select o.allow_stage_edit from object as o where o.object_id=" + objectId;
            var conn = new DBConnector();
            var dt = conn.GetCachedData(strSql);

            if (dt.Rows.Count > 0)
            {
                return DBConnector.GetNumInt(dt.Rows[0]["allow_stage_edit"]);
            }

            return 0;
        }

        public static string RemoveIisErrorCode(string queryString)
        {

            if (queryString.StartsWith("?"))
            {
                var errorCodeEndIndex = queryString.IndexOf(";", StringComparison.Ordinal);
                queryString = queryString.Remove(0, errorCodeEndIndex + 1);
            }


            return queryString;
        }

        public string GetButtonHtml()
        {
            return " <td width=\"28\" id=\"onfly_obj_<@2@>_btn_<@0@>\" <@3@> style=\"margin:0;padding:0\"" + " ><div style=\"cursor:hand;margin:0;padding:0\" border=\"0\"" + " ><img src=\"/rs/images/onfly/onfly_obj_<@2@>_over.jpg\" width=\"0\" height=\"0\" border=\"0\"" + " style=\"margin:0;padding:0\"" + " ><img src=\"/rs/images/onfly/onfly_obj_<@2@>.jpg\"" + " picture_name=\"onfly_obj_<@2@>\" border=\"0\"" + " title=\"<@1@>\" width=\"28\" height=\"26\"" + " onmouseover=\"onfly_obj_div_<@0@>.btnMouseOver(this)\"" + " onmouseout=\"onfly_obj_div_<@0@>.btnMouseOut(this)\"" + " onclick=\"onfly_obj_div_<@0@>.btnClick(this)\"" + " id=\"onfly_obj_<@2@>_btn_img_<@0@>\"" + " style=\"margin:0;padding:0\"" + " ></div></td>";
        }

        public string GetHrefHtml()
        {
            return "<td width=\"0\" style=\"margin:0;padding:0\"><a id=\"onfly_obj_<@1@>_href_<@0@>\"" + " href=\"<@2@>\"" + " target=\"main\"" + " obj_removed=\"<@2@>\"" + " ></a></td>";
        }

        public static int DbGetUserAccess(string table, int id, int gid)
        {
            return GetAccessInternal(table, id, gid, true, false);
        }

        public static int GetAccessInternal(string table, int id, int uid, bool isUser, bool useDictionary)
        {
            string sql;
            int groupId = 0, userId = 0;
            int contentId = 0, allowItemPermission = 0;

            if (isUser)
            {
                userId = uid;
            }
            else
            {
                groupId = uid;
            }

            if (uid == 1)
            {
                return 4;
            }

            if (Strings.LCase(table) == "content_item")
            {
                sql = $" SELECT c.* FROM content_item AS i with(nolock) LEFT OUTER JOIN content AS c ON c.content_id = i.content_id WHERE i.content_item_id = {id}";

                var conn = new DBConnector();
                var dt = conn.GetCachedData(sql);
                if (dt.Rows.Count > 0)
                {
                    contentId = DBConnector.GetNumInt(dt.Rows[0]["content_id"]);
                    allowItemPermission = DBConnector.GetNumInt(dt.Rows[0]["allow_items_permission"]);
                }

                if (allowItemPermission == 0 && !string.IsNullOrEmpty(contentId.ToString()))
                {
                    return GetAccessInternal("content", contentId, uid, isUser, useDictionary);
                }
            }

            sql = $"select dbo.qp_is_entity_accessible('{table}', {id}, {userId}, {groupId}, 0, 0, 1) as level";

            var conn1 = new DBConnector();
            var dt1 = conn1.GetCachedData(sql);
            var result = (int)dt1.Rows[0]["level"];
            if (result < 0)
            {
                result = 0;
            }

            return result;
        }

        public string GetUrlPort()
        {
            string functionReturnValue = null;
            if (!string.IsNullOrEmpty(HttpContext.Current.Request.ServerVariables["SERVER_PORT"]) && HttpContext.Current.Request.ServerVariables["SERVER_PORT"] != "80")
            {
                return ":" + HttpContext.Current.Request.ServerVariables["SERVER_PORT"];
            }

            return string.Empty;
        }

        public string GetReturnStageUrl()
        {
            return HttpContext.Current.Server.UrlEncode("http://" + HttpContext.Current.Request.ServerVariables["SERVER_NAME"] + GetUrlPort() + HttpContext.Current.Request.ServerVariables["SCRIPT_NAME"] + "?" + HttpContext.Current.Request.ServerVariables["QUERY_STRING"]);
        }

        public static bool IsBrowseServerMode()
        {
            return SessionEnabled() && Information.IsNumeric(HttpContext.Current.Session["BrowseServerSessionID"]);
        }

        public static string GetBrowserInfo()
        {
            var agent = HttpContext.Current.Request.ServerVariables["HTTP_USER_AGENT"];
            if (agent.IndexOf("Opera", StringComparison.Ordinal) >= 0)
            {
                return "opera";
            }

            if (agent.IndexOf("MSIE", StringComparison.Ordinal) >= 0)
            {
                return "ie";
            }

            if (agent.IndexOf("Firefox", StringComparison.Ordinal) >= 0)
            {
                return "firefox";
            }

            if (agent.IndexOf("Netscape/7", StringComparison.Ordinal) >= 0)
            {
                return "ns7";
            }

            if (agent.IndexOf("Mozilla/5", StringComparison.Ordinal) >= 0 && agent.IndexOf("Netscape", StringComparison.Ordinal) < 0)
            {
                return "mozilla";
            }

            if (agent.IndexOf("Mozilla", StringComparison.Ordinal) >= 0)
            {
                return "ns";
            }

            return "unknown";
        }

        public static bool UserAuthenticated()
        {
            return SessionEnabled() && HttpContext.Current.Session["uid"] != null;
        }
    }
}
