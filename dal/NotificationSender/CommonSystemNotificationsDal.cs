using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace Quantumart.QP8.DAL.NotificationSender
{
    public class CommonSystemNotificationsDal
    {
        private const string InsertNotificationsQuery =
            @"INSERT INTO [dbo].[SYSTEM_NOTIFICATION_QUEUE]
			(
                [CdcLastExecutedLsnId],
				[TRANSACTION_LSN],
				[TRANSACTION_DATE],
				[URL],
				[JSON]
			)
			SELECT
                col.value('(CdcLastExecutedLsnId)[1]','int') [CdcLastExecutedLsnId],
                col.value('(TransactionLsn)[1]','varchar(22)') [TRANSACTION_LSN],
                col.value('(TransactionDate)[1]','datetime') [TRANSACTION_DATE],
                col.value('(Url)[1]','nvarchar(max)') [URL],
                col.value('(Json)[1]','nvarchar(max)') [JSON]
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

        private const string DeleteSentNotificationsQuery = @"DELETE FROM SYSTEM_NOTIFICATION_QUEUE WHERE SENT = 1";

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
