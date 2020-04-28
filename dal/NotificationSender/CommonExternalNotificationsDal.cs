using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using Microsoft.EntityFrameworkCore.Storage;
using Quantumart.QP8.Constants;

namespace Quantumart.QP8.DAL.NotificationSender
{
    public class CommonExternalNotificationsDal
    {
        private const string SqlInsertNotificationsQuery =
            @"INSERT INTO [dbo].[EXTERNAL_NOTIFICATION_QUEUE]
			(
				[EVENT_NAME],
				[ARTICLE_ID],
				[URL],
				[NEW_XML],
				[OLD_XML],
				[CONTENT_ID],
				[SITE_ID]
			)
			SELECT
                col.value('(EventName)[1]','nvarchar(max)') [EVENT_NAME],
                col.value('(ArticleId)[1]','numeric') [ARTICLE_ID],
                col.value('(Url)[1]','nvarchar(max)') [URL],
                col.value('(NewXml)[1]','nvarchar(max)') [NEW_XML],
                col.value('(OldXml)[1]','nvarchar(max)') [OLD_XML],
                col.value('(ContentId)[1]','numeric') [CONTENT_ID],
                col.value('(SiteId)[1]','numeric') [SITE_ID]
			FROM
				@notifications.nodes('/Notifications/Notification') AS tbl(col)";

        private const string PgInsertNotificationsQuery =
            @"
        WITH v AS (
            SELECT EventName, ArticleId, Url, NewXml, OldXml, ContentId, SiteId
            FROM XMLTABLE('Notifications/Notification' passing @notifications COLUMNS
                EventName text PATH 'EventName',
                ArticleId numeric PATH 'ArticleId',
                Url text PATH 'Url',
                NewXml text PATH 'NewXml',
                OldXml text PATH 'OldXml',
                ContentId numeric PATH 'ContentId',
                SiteId numeric PATH 'SiteId'
            ) x
        )
        INSERT INTO external_notification_queue (event_name, article_id, url, new_xml, old_xml, content_id, site_id)
        SELECT v.EventName, v.ArticleId, v.Url, v.NewXml, v.OldXml, v.ContentId, v.SiteId FROM v";

        private static void ExecuteIdsQuery(DbConnection connection, string query, IEnumerable<int> ids)
        {
            using (var cmd = DbCommandFactory.Create(query, connection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Add(SqlQuerySyntaxHelper.GetIdsDatatableParam("@ids", ids, DatabaseTypeHelper.ResolveDatabaseType(connection)));

                cmd.ExecuteNonQuery();
            }
        }

        public static void InsertNotifications(DbConnection connection, string notificationsXml)
        {
            var dbType = DatabaseTypeHelper.ResolveDatabaseType(connection);
            var sql = dbType == DatabaseType.Postgres ? PgInsertNotificationsQuery : SqlInsertNotificationsQuery;
            using (var cmd = DbCommandFactory.Create(sql, connection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Add(SqlQuerySyntaxHelper.GetXmlParameter("@notifications", notificationsXml, dbType));
                cmd.ExecuteNonQuery();
            }
        }

        public static void UpdateSentNotifications(DbConnection connection, IEnumerable<int> ids)
        {
            var dbType = DatabaseTypeHelper.ResolveDatabaseType(connection);
            var query = $@"UPDATE EXTERNAL_NOTIFICATION_QUEUE SET
				SENT = {SqlQuerySyntaxHelper.ToBoolSql(dbType, true)},
				MODIFIED = {SqlQuerySyntaxHelper.Now(dbType)}
			WHERE ID IN (SELECT Id FROM {SqlQuerySyntaxHelper.IdList(dbType, "@ids", "i")})";
            ExecuteIdsQuery(connection, query, ids);
        }

        public static void UpdateUnsentNotifications(DbConnection connection, IEnumerable<int> ids)
        {
            var dbType = DatabaseTypeHelper.ResolveDatabaseType(connection);
            var query = $@"UPDATE EXTERNAL_NOTIFICATION_QUEUE SET
				TRIES = TRIES + 1,
				MODIFIED = {SqlQuerySyntaxHelper.Now(dbType)}
			WHERE ID IN (SELECT Id FROM {SqlQuerySyntaxHelper.IdList(dbType, "@ids", "i")})";
            ExecuteIdsQuery(connection, query, ids);
        }

        public static void DeleteSentNotifications(DbConnection connection)
        {
            var dbType = DatabaseTypeHelper.ResolveDatabaseType(connection);
            var query = $"DELETE FROM EXTERNAL_NOTIFICATION_QUEUE WHERE SENT = {SqlQuerySyntaxHelper.ToBoolSql(dbType, true)}";
            var dbCommand = DbCommandFactory.Create(query, connection);
            using (var cmd = dbCommand)
            {
                cmd.CommandType = CommandType.Text;
                cmd.ExecuteNonQuery();
            }
        }
    }
}
