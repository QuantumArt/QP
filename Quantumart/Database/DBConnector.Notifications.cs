using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Xml.Linq;
using Microsoft.VisualBasic;
using QA_Assembling;
using Quantumart.QPublishing.Info;
using VersionNotFoundException = Quantumart.QPublishing.Info.VersionNotFoundException;

namespace Quantumart.QPublishing.Database
{
    // ReSharper disable once InconsistentNaming
    public partial class DBConnector
    {
        #region Notifications

        private void ProceedExternalNotification(int id, string eventName, string externalUrl, ContentItem item, bool useService)
        {
            eventName = eventName.ToLowerInvariant();
            if (!String.IsNullOrEmpty(externalUrl))
            {
                if (useService)
                {

                    EnqueueNotification(id, eventName, externalUrl, item);
                }
                else
                {
                    MakeExternalCall(id, eventName, externalUrl, item);
                }
            }
        }

        private void EnqueueNotification(int id, string eventName, string externalUrl, ContentItem item)
        {
            var newDoc = item.GetXDocument();
            XDocument oldDoc = null;

            if (eventName == NotificationEvent.Modify || eventName == NotificationEvent.StatusChanged || eventName == NotificationEvent.StatusPartiallyChanged || eventName == NotificationEvent.DelayedPublication)
            {
                try
                {
                    oldDoc = ContentItem.ReadLastVersion(item.Id, this).GetXDocument();
                }
                catch (VersionNotFoundException) { }
            }
            else if (eventName == NotificationEvent.Remove)
            {
                oldDoc = newDoc;
                newDoc = null;
            }

            using (var cmd = new SqlCommand())
            {
                cmd.CommandType = CommandType.Text;
                var sb = new StringBuilder();
                sb.AppendLine("insert into EXTERNAL_NOTIFICATION_QUEUE(ARTICLE_ID, EVENT_NAME, URL, NEW_XML, OLD_XML)");
                sb.AppendLine("values (@id, @eventName, @url, @newXml, @oldXml)");
                cmd.CommandText = sb.ToString();
                cmd.Parameters.AddWithValue("@id", id);
                cmd.Parameters.Add(new SqlParameter("@eventName", SqlDbType.NVarChar, 50) { Value = eventName });
                cmd.Parameters.Add(new SqlParameter("@url", SqlDbType.NVarChar, 1024) { Value = externalUrl });
                cmd.Parameters.Add(new SqlParameter("@newXml", SqlDbType.NVarChar, -1) { Value = newDoc == null ? DBNull.Value : (object)newDoc.ToString() });
                cmd.Parameters.Add(new SqlParameter("@oldXml", SqlDbType.NVarChar, -1) { Value = oldDoc == null ? DBNull.Value : (object)oldDoc.ToString() });
                ProcessData(cmd);
            }
        }

        private void MakeExternalCall(int id, string eventName, string externalUrl, ContentItem item)
        {
            var queryParams = new Dictionary<string, object>
            {
                {"eventName", eventName},
                {"id", id},
                {"contentId", item.ContentId},
                {"siteId", GetSiteIdByContentId(item.ContentId)},
                {"visible", item.Visible},
                {"archive", item.Archive},
                {"splitted", item.Splitted},
                {"statusName", item.StatusName}
            };

            if (eventName == NotificationEvent.Modify || eventName == NotificationEvent.StatusChanged || eventName == NotificationEvent.StatusPartiallyChanged)
            {
                var row = GetPreviousStatusHistoryRecord(id);
                queryParams.Add("oldVisible", (bool)row["visible"]);
                queryParams.Add("oldArchive", (bool)row["archive"]);
                queryParams.Add("oldStatusName", row["status_type_name"].ToString());
            }

            var arr =
                queryParams
                .AsEnumerable()
                .Select(n => $"{n.Key}={n.Value.ToString().ToLowerInvariant()}")
                .ToArray();

            var queryString = String.Join("&", arr);
            var delimiter = externalUrl.Contains("?") ? "&" : "?";
            var fullUrl = String.Concat(externalUrl, delimiter, queryString);
            var request = (HttpWebRequest)WebRequest.Create(fullUrl);
            request.BeginGetResponse(ExternalNotificationCallback, request);
        }

        private void ExternalNotificationCallback(IAsyncResult iar)
        {
            try
            {
                var request = (HttpWebRequest)iar.AsyncState;
                request.EndGetResponse(iar);
            }
            catch (Exception ex)
            {
                var errorMessage =
                    $"{"DbConnector.cs, ExternalNotificationCallback(IAsyncResult iar)"}, MESSAGE: {ex.Message} STACK TRACE: {ex.StackTrace}";
                System.Diagnostics.EventLog.WriteEntry("Application", errorMessage);
            }

        }

        public void SendNotification(int contentItemId, string notificationOn)
        {
            var contentId = GetContentIdForItem(contentItemId);
            if (contentId == 0)
                throw new Exception($"Article (ID = {contentItemId}) is not found");
            var siteId = GetSiteIdByContentId(contentId);
            SendNotification(siteId, notificationOn, contentItemId, "", !IsStage);
        }

        public void SendNotification(int siteId, string notificationOn, int contentItemId, string notificationEmail, bool isLive)
        {
            ValidateNotificationEvent(notificationOn);
            try
            {
                var rstNotifications = GetNotificationsTable(notificationOn, contentItemId);
                var hasUseServiceColumn = rstNotifications.Columns.Contains("USE_SERVICE");
                var notifications = rstNotifications.Rows.Cast<DataRow>();
                var enumerable = notifications as DataRow[] ?? notifications.ToArray();
                var externalNotifications = enumerable.Where(n => (bool)n["is_external"]);
                var dataRows = externalNotifications as DataRow[] ?? externalNotifications.ToArray();
                if (dataRows.Any())
                {
                    var item = ContentItem.Read(contentItemId, this);
                    foreach (var row in dataRows)
                    {
                        var useService = hasUseServiceColumn && (bool)row["USE_SERVICE"];
                        var url = GetString(row["EXTERNAL_URL"], String.Empty);
                        ProceedExternalNotification(contentItemId, notificationOn, url, item, useService);
                    }

                }

                var internalNotifications = enumerable.Except(dataRows);

                if (Strings.LCase(AppSettings["MailComponent"]) == "qa_mail" || string.IsNullOrEmpty(AppSettings["MailHost"]))
                {
                    if (internalNotifications.Any())
                    {
                        LoadUrl(GetAspWrapperUrl(siteId, notificationOn, contentItemId, notificationEmail, isLive));
                    }
                }
                else
                {
                    var strSqlRegisterNotifForUsers = "";
                    var platform = GetSitePlatform(siteId);

                    foreach (var notifyRow in internalNotifications)
                    {
                        if (!ReferenceEquals(notifyRow["OBJECT_ID"], DBNull.Value))
                        {

                            var objectId = GetNumInt(notifyRow["OBJECT_ID"]);
                            var contentId = GetNumInt(notifyRow["CONTENT_ID"]);
                            var objectFormatId = GetNumInt(notifyRow["FORMAT_ID"]);

                            if (Strings.LCase(AppSettings["MailAssemble"]) == "yes")
                            {
                                switch (platform)
                                {
                                    case SitePlatform.Aspnet:
                                        var fixedLocation = isLive ? AssembleLocation.Live : AssembleLocation.Stage;

                                        var cnt = new AssembleFormatController(objectFormatId, AssembleMode.Notification, InstanceConnectionString, false, fixedLocation);
                                        cnt.Assemble();
                                        break;
                                    case SitePlatform.Asp:
                                        AssembleFormatToFile(siteId, objectFormatId);
                                        break;
                                }
                            }

                            var targetUrl = GetNotificationBodyUrl(siteId, objectId, contentItemId, isLive, notificationOn);

                            if (GetNumBool(notifyRow["NO_EMAIL"]))
                            {
                                LoadUrl(targetUrl);
                            }
                            else
                            {
                                var mailMess = new MailMessage
                                {
                                    Subject = notifyRow["NOTIFICATION_NAME"].ToString(),
                                    From = GetFromAddress(notifyRow)
                                };


                                //set To
                                SetToMail(notifyRow, contentItemId, notificationOn, notificationEmail, mailMess, ref strSqlRegisterNotifForUsers);

                                mailMess.IsBodyHtml = true;
                                mailMess.BodyEncoding = Encoding.GetEncoding(GetFormatCharset(objectFormatId));
                                string body;

                                var doAttachFiles = (bool)notifyRow["SEND_FILES"];
                                try
                                {
                                    body = GetUrlContent(targetUrl);
                                }
                                catch (Exception ex)
                                {
                                    body =
                                        $"An error has occurred while getting url data: {targetUrl}. Error message: {ex.Message}";
                                    var errorMessage =
                                        $"{"DbConnector.cs, SendNotification"}, MESSAGE: {ex.Message} STACK TRACE: {ex.StackTrace}";
                                    System.Diagnostics.EventLog.WriteEntry("Application", errorMessage);
                                    doAttachFiles = false;
                                }

                                mailMess.Body = body;

                                if (doAttachFiles)
                                {
                                    AttachFiles(mailMess, siteId, contentId, contentItemId);
                                }

                                SendMail(mailMess);

                                //register sent notifications
                                if (!string.IsNullOrEmpty(strSqlRegisterNotifForUsers + ""))
                                {
                                    ProcessData(strSqlRegisterNotifForUsers);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                var errorMessage =
                    $"{"DbConnector.cs, SendNotification"}, MESSAGE: {ex.Message} STACK TRACE: {ex.StackTrace}";
                System.Diagnostics.EventLog.WriteEntry("Application", errorMessage);
            }
        }

        private void ValidateNotificationEvent(string notificationOn)
        {
            string[] events =
            {
                NotificationEvent.Create,
                NotificationEvent.Modify,
                NotificationEvent.Remove,
                NotificationEvent.StatusChanged,
                NotificationEvent.StatusPartiallyChanged,
                NotificationEvent.FrontendRequest,
                NotificationEvent.DelayedPublication
            };

            var ok = events.Contains(notificationOn.ToLowerInvariant());

            if (!ok)
            {
                throw new Exception("notificationOn parameter is not valid. Choose it from the following range: " + string.Join(", ", events));
            }
        }

        #region Loading Url

        public string GetUrlContent(string url)
        {
            return GetUrlContent(url, true);
        }

        public string GetUrlContent(string url, bool throwException)
        {
            string functionReturnValue;
            var response = GetUrlResponse(url);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                var reader = new StreamReader(response.GetResponseStream(), Encoding.GetEncoding(response.CharacterSet));
                functionReturnValue = reader.ReadToEnd();
                response.Close();
            }
            else
            {
                if (throwException)
                {
                    throw new Exception(response.StatusDescription + "(" + response.StatusCode + "): " + url);
                }
                else
                {
                    functionReturnValue = url + "<br>" + response.StatusCode + "<br>" + response.StatusDescription;
                }
            }
            return functionReturnValue;
        }

        public void LoadUrl(string url)
        {
            GetUrlContent(url);
        }

        private HttpWebResponse GetUrlResponse(string url)
        {
            HttpWebResponse functionReturnValue;
            try
            {
                var request = (HttpWebRequest)WebRequest.Create(url);
                functionReturnValue = (HttpWebResponse)request.GetResponse();
            }
            catch (WebException ex)
            {
                functionReturnValue = (HttpWebResponse)ex.Response;
            }
            return functionReturnValue;
        }

        private string GetObjectFolderUrl(int siteId, bool isLive)
        {
            //Return String.Format("{0}/{1}/", GetSiteUrl(siteId, isLive), "temp/notifications")
            return $"{GetSiteUrl(siteId, isLive)}/{"qp_notifications"}/";
        }


        private string GetNotificationBodyUrl(int siteId, int objectId, int contentItemId, bool isLive, string notificationOn)
        {
            return
                $"{GetObjectFolderUrl(siteId, isLive || ForceLive(siteId))}{objectId}.{GetSiteFileExtension(siteId)}?id={contentItemId}&on={notificationOn}";
        }

        #endregion

        #region Assembling

        public void AssembleFormatToFile(int siteId, int objectFormatId)
        {
            LoadUrl(GetNotifyDirectory(siteId) + "AssembleFormat.asp?objectFormatId=" + objectFormatId);
        }

        private string GetNotifyDirectory(int siteId)
        {
            var notifyUrl = GetSiteUrl(siteId, true) + AppSettings["RelNotifyUrl"];
            return notifyUrl.ToLowerInvariant().Replace("notify.asp", "");
        }

        private enum SitePlatform
        {
            Asp,
            Aspnet
        }

        private SitePlatform GetSitePlatform(int siteId)
        {
            var site = GetSite(siteId);
            return site != null && site.ScriptLanguage.ToLowerInvariant() == "vbscript" ? SitePlatform.Asp : SitePlatform.Aspnet;
        }

        private string GetFileExtension(SitePlatform platform)
        {
            switch (platform)
            {
                case SitePlatform.Asp:
                    return "asp";
                case SitePlatform.Aspnet:
                    return "aspx";
            }
            return "aspx";
        }

        private string GetSiteFileExtension(int siteId)
        {
            return GetFileExtension(GetSitePlatform(siteId));
        }

        private string GetFormatCharset(int objectFormatId)
        {

            string functionReturnValue;
            var sb = new StringBuilder();
            sb.Append("EXEC sp_executesql N'select p.charset as page_charset, pt.charset as template_charset from object_format objf ");
            sb.Append(" inner join object o on o.objectId = objf.objectId ");
            sb.Append(" inner join page_template pt on o.page_template_id = pt.page_template_id ");
            sb.Append(" left join page p on o.page_id = p.page_id ");
            sb.AppendFormat(" where objf.objectFormatId = @formatId', N'@formatId NUMERIC', @formatId = {0} ", objectFormatId);
            var table = GetCachedData(sb.ToString());
            if (table.Rows.Count > 0)
            {
                var pageCharset = ConvertToString(table.Rows[0]["page_charset"]);
                var templateCharset = ConvertToString(table.Rows[0]["template_charset"]);
                functionReturnValue = !string.IsNullOrEmpty(pageCharset) ? pageCharset : templateCharset;
            }
            else
            {
                functionReturnValue = "";
            }
            return functionReturnValue;
        }

        private string GetAspWrapperUrl(int siteId, string notificationOn, int contentItemId, string notificationEmail, bool isLive)
        {
            var liveString = isLive ? "1" : "0";

            return
                $"{GetSiteUrl(siteId, isLive)}{AppSettings["RelNotifyUrl"]}?id={contentItemId.ToString()}&target={notificationOn}&email={notificationEmail}&is_live={liveString}";
        }

        #endregion

        #region Get notification data

        private DataTable GetNotificationsTable(string notificationOn, int contentItemId)
        {
            var testcmd =
                new SqlCommand(
                    "select count(*) From information_schema.columns where column_name = 'USE_SERVICE' and table_name = 'NOTIFICATIONS'")
                {
                    CommandType = CommandType.Text
                };
            var colCount = (int)GetRealScalarData(testcmd);
            var serviceString = colCount == 0 ? "" : ", n.USE_SERVICE";

            var contentId = GetContentIdForItem(contentItemId);
            var sb = new StringBuilder();
            sb.Append("EXEC sp_executesql N'");
            sb.AppendFormat("select n.NOTIFICATION_ID{0}, n.NOTIFICATION_NAME, n.CONTENT_ID, n.FORMAT_ID, n.USER_ID, n.GROUP_ID, n.NOTIFY_ON_STATUS_TYPE_ID, n.EMAIL_ATTRIBUTE_ID, n.NO_EMAIL, n.SEND_FILES, n.FROM_BACKENDUSER_ID, n.FROM_BACKENDUSER, n.FROM_DEFAULT_NAME, n.FROM_USER_EMAIL, n.FROM_USER_NAME, ", serviceString);
            sb.Append(" f.objectId, c.siteId, n.is_external, coalesce(n.external_url, s.external_url) as external_url FROM notifications AS n with(nolock)");
            sb.Append(" INNER JOIN content AS c with(nolock) ON c.contentId = n.contentId");
            sb.Append(" INNER JOIN site AS s with(nolock) ON c.siteId = s.siteId");
            sb.Append(" LEFT OUTER JOIN object_format AS f with(nolock) ON f.objectFormatId = n.format_id");
            sb.Append(" WHERE n.contentId = @contentId");
            sb.AppendFormat(" AND n.{0} = 1", notificationOn);
            if (notificationOn.ToLowerInvariant() == NotificationEvent.StatusChanged)
            {
                var dt = GetRealData(
                    $"EXEC sp_executesql N'select status_type_id from content_item where content_item_id = @id', N'@id NUMERIC', @id = {contentItemId}");
                var status = dt.Rows[0]["status_type_id"].ToString();
                sb.AppendFormat(" AND (n.notify_on_status_type_id IS NULL OR n.notify_on_status_type_id = {0})", status);
            }
            sb.AppendFormat("', N'@contentId NUMERIC', @contentId = {0}", contentId);
            return GetCachedData(sb.ToString());
        }

        private DataTable GetRecipientTable(DataRow notifyRow, int contentItemId)
        {
            var userId = notifyRow["USER_ID"];
            var groupId = notifyRow["GROUP_ID"];
            var eMailAttrId = notifyRow["EMAIL_ATTRIBUTE_ID"];

            string strSql;

            if (!ReferenceEquals(userId, DBNull.Value))
            {
                // Notify User
                strSql = "EXEC sp_executesql N'SELECT EMAIL, USER_ID FROM users WHERE user_id = @userId'";
            }
            else if (!ReferenceEquals(groupId, DBNull.Value))
            {
                // Notify Group
                strSql = "EXEC sp_executesql N'SELECT U.EMAIL, U.USER_ID FROM users AS u LEFT OUTER JOIN user_group_bind AS ub ON ub.user_id = u.user_id WHERE ub.group_id = @groupId'";
            }
            else if (!ReferenceEquals(eMailAttrId, DBNull.Value))
            {
                // Notify E-Mail from Content Item Field Value
                strSql = "EXEC sp_executesql N'SELECT DATA AS EMAIL, NULL AS USER_ID FROM content_data WHERE contentItemId = @itemId AND attribute_id = @fieldId'";
            }
            else
            {
                // Notify Everyone in History
                strSql = "EXEC sp_executesql N'SELECT DISTINCT(U.EMAIL), U.USER_ID FROM content_item_status_history AS ch LEFT OUTER JOIN users AS u ON ch.user_id = u.user_id WHERE ch.contentItemId=@itemId'";
            }


            return GetCachedData(
                $"{strSql}, N'@itemId NUMERIC, @userId NUMERIC, @groupId NUMERIC, @fieldId NUMERIC', @itemId = {contentItemId}, @userId = {ConvertToNullString(userId)}, @groupId = {ConvertToNullString(groupId)}, @fieldId = {ConvertToNullString(eMailAttrId)}");
        }

        private static string GetSqlRegisterNotificationsForUsers(DataTable toTable, int contentItemId, int notificationId, string notificationOn)
        {
            var sb = new StringBuilder();
            sb.AppendLine();
            foreach (DataRow dr in toTable.Rows)
            {
                if (!ReferenceEquals(dr["EMAIL"], DBNull.Value))
                {
                    if (!ReferenceEquals(dr["USER_ID"], DBNull.Value))
                    {
                        sb.AppendFormat("EXEC sp_executesql N'INSERT INTO notifications_sent VALUES (@userId, @notifyId, @itemId, DEFAULT, @notifyOn)', N'@userId NUMERIC, @notifyId NUMERIC, @itemId NUMERIC, @notifyOn NVARCHAR(50)', @userId = {0}, @notifyId = {1}, @itemId = {2}, @notifyOn = N'{3}';", dr["USER_ID"], notificationId, contentItemId, Strings.LCase(notificationOn));
                    }
                }
            }
            return sb.ToString();
        }


        private string ConvertToString(object obj)
        {
            return obj == DBNull.Value ? "" : obj.ToString();
        }

        private string ConvertToNullString(object obj)
        {
            return ReferenceEquals(obj, DBNull.Value) ? "NULL" : obj.ToString();
        }

        #endregion

        #region Working with mail message

        private MailAddress GetFromAddress(DataRow notifyRow)
        {
            MailAddress functionReturnValue;
            string fromName;
            var from = "";
            if ((bool)notifyRow["FROM_DEFAULT_NAME"])
            {
                fromName = AppSettings["MailFromName"];
            }
            else
            {
                fromName = ConvertToString(notifyRow["FROM_USER_NAME"]);
            }
            if ((bool)notifyRow["FROM_BACKENDUSER"])
            {
                var rstUsers = GetCachedData(
                    $"EXEC sp_executesql N'SELECT EMAIL FROM USERS WHERE USER_ID = @userId', N'@userId NUMERIC', @userId = {notifyRow["FROM_BACKENDUSER_ID"]}");
                if (rstUsers.Rows.Count > 0)
                {
                    from = rstUsers.Rows[0]["EMAIL"].ToString();
                }
            }
            else
            {
                from = ConvertToString(notifyRow["FROM_USER_EMAIL"]);
            }
            if (!string.IsNullOrEmpty(from))
            {
                functionReturnValue = !string.IsNullOrEmpty(fromName) ? new MailAddress(from, fromName) : new MailAddress(from);
            }
            else
            {
                throw new Exception("Mail sender is not defined");
            }

            return functionReturnValue;
        }

        public void AttachFiles(MailMessage mailMess, int siteId, int contentId, int contentItemId)
        {
            var strDataSql =
                $"EXEC sp_executesql N'select cd.data from content_data cd inner join content_attribute ca on cd.attribute_id = ca.attribute_id where ca.contentId = @contentId and ca.attribute_type_id in (7,8) and cd.content_item_id = @itemId', N'@contentId NUMERIC, @itemId NUMERIC' , @contentId = {contentId}, @itemId = {contentItemId}";
            var rstData = GetRealData(strDataSql);
            var currentDir = GetUploadDir(siteId) + "\\contents\\" + contentId;

            foreach (DataRow fileRow in rstData.Rows)
            {
                var fileName = currentDir + "\\" + fileRow["data"];
                if (File.Exists(fileName)) mailMess.Attachments.Add(new Attachment(fileName));
            }
        }

        private void SendMail(MailMessage mailMess)
        {
            var mailHost = AppSettings["MailHost"];
            var smtpMail = new SmtpClient {UseDefaultCredentials = false};

            if (string.IsNullOrEmpty(mailHost))
            {
                throw new Exception("MailHost configuration parameter is not defined");
            }
            else
            {
                smtpMail.Host = mailHost;
            }
            if (!string.IsNullOrEmpty(AppSettings["MailLogin"]))
            {
                var credentials = new NetworkCredential
                {
                    UserName = AppSettings["MailLogin"],
                    Password = AppSettings["MailPassword"]
                };
                smtpMail.Credentials = credentials;
            }
            smtpMail.Send(mailMess);
        }

        private void SetToMail(MailMessage mailMess, DataTable toTable)
        {
            foreach (DataRow dr in toTable.Rows)
            {
                SetToMail(mailMess, ConvertToString(dr["EMAIL"]));
            }
        }

        private void SetToMail(MailMessage mailMess, string allEmails)
        {
            var emails = allEmails.Split(';');
            foreach (var email in emails)
            {
                if (!string.IsNullOrEmpty(email))
                {
                    mailMess.To.Add(new MailAddress(email));
                }
            }
        }

        private void SetToMail(DataRow notifyRow, int contentItemId, string notificationOn, string notificationEmail, MailMessage mailMess, ref string strSqlRegisterNotificationsForUsers)
        {

            var notificationId = GetNumInt(notifyRow["NOTIFICATION_ID"]);
            if (Strings.Len(notificationEmail) > 0)
            {
                SetToMail(mailMess, notificationEmail);
            }
            else
            {
                var toTable = GetRecipientTable(notifyRow, contentItemId);
                SetToMail(mailMess, toTable);
                strSqlRegisterNotificationsForUsers = GetSqlRegisterNotificationsForUsers(toTable, contentItemId, notificationId, notificationOn);
            }
        }


        #endregion

        #endregion
    }
}
