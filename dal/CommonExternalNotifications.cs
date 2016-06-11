using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using Quantumart.QP8.Constants;
using Quantumart.QP8.Utils;

namespace Quantumart.QP8.DAL
{
	public class CommonExternalNotifications
	{
		#region External Notification
		private const string InsertNotificationsQuery =
			@"INSERT INTO [dbo].[EXTERNAL_NOTIFICATION_QUEUE]
			(
				[EVENT_NAME],
				[ARTICLE_ID],
				[URL],
				[NEW_XML],
				[OLD_XML]
			)
			SELECT
				  col.value('(EventName)[1]','nvarchar(50)') [EVENT_NAME],
				  col.value('(ArticleId)[1]','numeric(18,0)') [ARTICLE_ID],
				  col.value('(Url)[1]','nvarchar(1024)') [URL],
				  col.value('(NewXml)[1]','nvarchar(max)') [NEW_XML],
				  col.value('(OldXml)[1]','nvarchar(max)') [OLD_XML]      
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
		private const string DeleteSentNotificationsQuery = @"DELETE EXTERNAL_NOTIFICATION_QUEUE WHERE SENT = 1";

		private static void ExecuteIdsQuery(SqlConnection connection, string Query, IEnumerable<int> ids)
		{
			using (SqlCommand cmd = SqlCommandFactory.Create(Query, connection))
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
			using (SqlCommand cmd = SqlCommandFactory.Create(InsertNotificationsQuery, connection))
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
			using (SqlCommand cmd = SqlCommandFactory.Create(DeleteSentNotificationsQuery, connection))
			{
				cmd.CommandType = CommandType.Text;
				cmd.ExecuteNonQuery();
			}
		}
		#endregion
	}
}