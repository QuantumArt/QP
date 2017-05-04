using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace Quantumart.QP8.DAL.NotificationSender
{
    public class CommonSystemNotifications
    {
        private const string InsertNotificationsQuery =
            @"INSERT INTO [dbo].[SYSTEM_NOTIFICATION_QUEUE]
			(
				[TRANSACTION_DATE],
				[EVENT],
				[TYPE],
				[URL],
				[JSON]
			)
			SELECT
                col.value('(EventName)[1]','nvarchar(50)') [EVENT_NAME],
                col.value('(ArticleId)[1]','numeric(18,0)') [ARTICLE_ID],
                col.value('(Url)[1]','nvarchar(1024)') [URL],
                col.value('(NewXml)[1]','nvarchar(max)') [NEW_XML],
                col.value('(OldXml)[1]','nvarchar(max)') [OLD_XML],
                col.value('(ContentId)[1]','numeric(18,0)') [CONTENT_ID],
                col.value('(SiteId)[1]','numeric(18,0)') [SITE_ID]
			FROM
                @notifications.nodes('/Notifications/Notification') AS tbl(col)";

        private const string UpdateSentNotificationsQuery =
            @"UPDATE SYSTEM_NOTIFICATION_QUEUE SET
				SENT = 1,
				MODIFIED = getdate()
			WHERE ID IN (SELECT Id FROM @ids)";

        private const string UpdateUnsentNotificationsQuery =
            @"UPDATE SYSTEM_NOTIFICATION_QUEUE SET
				TRIES = TRIES + 1,
				MODIFIED = getdate()
			WHERE ID IN (SELECT Id FROM @ids)";

        private const string DeleteSentNotificationsQuery = @"DELETE SYSTEM_NOTIFICATION_QUEUE WHERE SENT = 1";
        private const string ExistsSentNotificationsQuery = @"SELECT COUNT(ID) FROM SYSTEM_NOTIFICATION_QUEUE WHERE SENT = 1";

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

        public static bool ExistsSentNotifications(SqlConnection connection)
        {
            using (var cmd = SqlCommandFactory.Create(ExistsSentNotificationsQuery, connection))
            {
                cmd.CommandType = CommandType.Text;
                return (int)cmd.ExecuteScalar() > 0;
            }
        }
    }
}
