using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
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
                LastExceptionMessage = NULL,
				MODIFIED = getdate()
			WHERE ID IN (SELECT Id FROM @ids)";

        private const string UpdateUnsentNotificationsQuery =
            @"UPDATE SYSTEM_NOTIFICATION_QUEUE SET
				TRIES = TRIES + 1,
                LastExceptionMessage = @lastExceptionMessage,
				MODIFIED = getdate()
			WHERE ID IN (SELECT Id FROM @ids)";

        private const string DeleteSentNotificationsQuery = @"DELETE FROM SYSTEM_NOTIFICATION_QUEUE WHERE SENT = 1";

        private static void ExecuteIdsQuery(DbConnection connection, string query, IEnumerable<int> ids, string lastExceptionMessage = null)
        {
            using (var cmd = DbCommandFactory.Create(query, connection))
            {
                cmd.CommandType = CommandType.Text;
                var idsTable = Common.IdsToDataTable(ids);

                switch (cmd)
                {
                    case SqlCommand sqlCommand:
                        var parameter = sqlCommand.Parameters.AddWithValue("@ids", idsTable);
                        parameter.SqlDbType = SqlDbType.Structured;
                        parameter.TypeName = "dbo.Ids";
                        sqlCommand.Parameters.Add(new SqlParameter("@lastExceptionMessage", SqlDbType.NVarChar, -1) { Value = (object)lastExceptionMessage ?? DBNull.Value });
                        break;
                    default:
                        throw new NotImplementedException("Not implemented for postgres");
                }



                cmd.ExecuteNonQuery();
            }
        }

        public static void InsertNotifications(DbConnection connection, string notificationsXml)
        {
            using (var cmd = DbCommandFactory.Create(InsertNotificationsQuery, connection))
            {
                cmd.CommandType = CommandType.Text;
                var parameter = cmd.Parameters.AddWithValue("@notifications", notificationsXml);
                switch (parameter)
                {
                    case SqlParameter sqlParameter:
                        sqlParameter.SqlDbType = SqlDbType.Xml;
                        break;
                    default:
                        throw new NotImplementedException("Not implemented for postgres");
                }
                cmd.ExecuteNonQuery();
            }
        }

        public static void UpdateSentNotifications(DbConnection connection, IEnumerable<int> ids)
        {
            ExecuteIdsQuery(connection, UpdateSentNotificationsQuery, ids);
        }

        public static void UpdateUnsentNotifications(DbConnection connection, IEnumerable<int> ids, string lastExceptionMessage = null)
        {
            ExecuteIdsQuery(connection, UpdateUnsentNotificationsQuery, ids, lastExceptionMessage);
        }

        public static void DeleteSentNotifications(DbConnection connection)
        {
            using (var cmd = DbCommandFactory.Create(DeleteSentNotificationsQuery, connection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.ExecuteNonQuery();
            }
        }
    }
}
