using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace Quantumart.QP8.DAL.NotificationSender
{
    public class CommonExternalNotificationsDal
    {
        private const string InsertNotificationsQuery =
            @"INSERT INTO [dbo].[EXTERNAL_NOTIFICATION_QUEUE]
			(
				[EVENT_NAME],
				[ARTICLE_ID],
				[URL],
				[NEW_XML],
				[OLD_XML]
				[CONTENT_ID],
				[SITE_ID],
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

        private const string UpdateSentNotificationsQuery =
            @"UPDATE EXTERNAL_NOTIFICATION_QUEUE SET
				SENT = 1,
				MODIFIED = getdate()
			WHERE ID IN (SELECT Id FROM @ids)";

        private const string UpdateUnsentNotificationsQuery =
            @"UPDATE EXTERNAL_NOTIFICATION_QUEUE SET
				TRIES = TRIES + 1,
				MODIFIED = getdate()
			WHERE ID IN (SELECT Id FROM @ids)";

        private const string DeleteSentNotificationsQuery = @"DELETE FROM EXTERNAL_NOTIFICATION_QUEUE WHERE SENT = 1";

        private static void ExecuteIdsQuery(SqlConnection connection, string query, IEnumerable<int> ids)
        {
            using (var cmd = SqlCommandFactory.Create(query, connection))
            {
                cmd.CommandType = CommandType.Text;
                var idsTable = Common.IdsToDataTable(ids);
                var parameter = cmd.Parameters.AddWithValue("@ids", idsTable);
                parameter.SqlDbType = SqlDbType.Structured;
                parameter.TypeName = "dbo.Ids";
                cmd.ExecuteNonQuery();
            }
        }

        public static void InsertNotifications(SqlConnection connection, string notificationsXml)
        {
            using (var cmd = SqlCommandFactory.Create(InsertNotificationsQuery, connection))
            {
                cmd.CommandType = CommandType.Text;
                var parameter = cmd.Parameters.AddWithValue("@notifications", notificationsXml);
                parameter.SqlDbType = SqlDbType.Xml;
                cmd.ExecuteNonQuery();
            }
        }

        public static void UpdateSentNotifications(SqlConnection connection, IEnumerable<int> ids)
        {
            ExecuteIdsQuery(connection, UpdateSentNotificationsQuery, ids);
        }

        public static void UpdateUnsentNotifications(SqlConnection connection, IEnumerable<int> ids)
        {
            ExecuteIdsQuery(connection, UpdateUnsentNotificationsQuery, ids);
        }

        public static void DeleteSentNotifications(SqlConnection connection)
        {
            using (var cmd = SqlCommandFactory.Create(DeleteSentNotificationsQuery, connection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.ExecuteNonQuery();
            }
        }
    }
}
