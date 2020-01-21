using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using Quantumart.QP8.Constants;

namespace Quantumart.QP8.DAL.NotificationSender
{
    public class CommonSystemNotificationsDal
    {
        private const string SqlInsertNotificationsQuery =
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

        private const string PgInsertNotificationsQuery =
            @"
WITH v AS (
    SELECT CdcLastExecutedLsnId, TRANSACTION_LSN, TRANSACTION_DATE, URL, JSON
    FROM XMLTABLE('Notifications/Notification' passing @notifications COLUMNS
        CdcLastExecutedLsnId integer PATH 'CdcLastExecutedLsnId',
        TRANSACTION_LSN text PATH 'TransactionLsn',
        TRANSACTION_DATE timestamp with time zone PATH 'TransactionDate',
        URL text PATH 'Url',
        JSON text PATH 'Json'
        ) x
    )
    INSERT INTO system_notification_queue (CdcLastExecutedLsnId, TRANSACTION_LSN, TRANSACTION_DATE, URL, JSON)
    SELECT v.CdcLastExecutedLsnId, v.TRANSACTION_LSN, v.TRANSACTION_DATE, v.URL, v.JSON";

        private static void ExecuteIdsQuery(DbConnection connection, string query, IEnumerable<int> ids, string lastExceptionMessage = null)
        {
            using (var cmd = DbCommandFactory.Create(query, connection))
            {
                var dbType = DatabaseTypeHelper.ResolveDatabaseType(connection);
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Add(SqlQuerySyntaxHelper.GetIdsDatatableParam("@ids", ids, dbType));
                cmd.Parameters.AddWithValue("@lastExceptionMessage", lastExceptionMessage);

                cmd.ExecuteNonQuery();
            }
        }

        public static void InsertNotifications(DbConnection connection, string notificationsXml)
        {
            var dbType = DatabaseTypeHelper.ResolveDatabaseType(connection);
            var query = dbType == DatabaseType.Postgres ? PgInsertNotificationsQuery : SqlInsertNotificationsQuery;
            using (var cmd = DbCommandFactory.Create(query, connection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Add(SqlQuerySyntaxHelper.GetXmlParameter("@notifications", notificationsXml, dbType));
                cmd.ExecuteNonQuery();
            }
        }

        public static void UpdateSentNotifications(DbConnection connection, IEnumerable<int> ids)
        {
            var dbType = DatabaseTypeHelper.ResolveDatabaseType(connection);
            var query = $@"UPDATE SYSTEM_NOTIFICATION_QUEUE SET
				SENT = {SqlQuerySyntaxHelper.ToBoolSql(dbType, true)},
                LastExceptionMessage = NULL,
				MODIFIED = {SqlQuerySyntaxHelper.Now(dbType)}
			WHERE ID IN (SELECT Id FROM {SqlQuerySyntaxHelper.IdList(dbType, "@ids", "i")})";
            ExecuteIdsQuery(connection, query, ids);
        }

        public static void UpdateUnsentNotifications(DbConnection connection, IEnumerable<int> ids, string lastExceptionMessage = null)
        {
            var dbType = DatabaseTypeHelper.ResolveDatabaseType(connection);
            var query = $@"UPDATE SYSTEM_NOTIFICATION_QUEUE SET
				TRIES = TRIES + 1,
                LastExceptionMessage = @lastExceptionMessage,
				MODIFIED = {SqlQuerySyntaxHelper.Now(dbType)}
			WHERE ID IN (SELECT Id FROM {SqlQuerySyntaxHelper.IdList(dbType, "@ids", "i")})";
            ExecuteIdsQuery(connection, query, ids, lastExceptionMessage);
        }

        public static void DeleteSentNotifications(DbConnection connection)
        {
            var dbType = DatabaseTypeHelper.ResolveDatabaseType(connection);
            var query = $"DELETE FROM SYSTEM_NOTIFICATION_QUEUE WHERE SENT = {SqlQuerySyntaxHelper.ToBoolSql(dbType, true)}";
            using (var cmd = DbCommandFactory.Create(query, connection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.ExecuteNonQuery();
            }
        }
    }
}
