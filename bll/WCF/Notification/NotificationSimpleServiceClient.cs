using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Net;
using Quantumart.QP8.Security;
using System.Web.Security;
using System.Web;
using Quantumart.QPublishing.Database;
using Microsoft.Practices.EnterpriseLibrary.Common.Configuration;
using Microsoft.Practices.EnterpriseLibrary.ExceptionHandling;
using System.Threading;

namespace Quantumart.QP8.BLL.WCF.Notification
{
	internal partial class NotificationSimpleServiceClient : INotificationService
	{

		public NotificationSimpleServiceClient()
		{
		}

		public void SendNotificationOneWay(string connectionString, int id, string code)
		{
			ThreadPool.QueueUserWorkItem((o) => {
				SimpleSendNotification(connectionString, id, code);
			});
		}

		public void SendNotification(string connectionString, int id, string code)
		{
			ManualResetEvent evt = new ManualResetEvent(false);
			ThreadPool.QueueUserWorkItem((o) =>
			{
				SimpleSendNotification(connectionString, id, code);
				evt.Set();
			});
			evt.WaitOne();
		}

		private void SimpleSendNotification(string connectionString, int id, string code)
		{
			ExceptionManager handler = EnterpriseLibraryContainer.Current.GetInstance<ExceptionManager>();
			try
			{
				DBConnector cnn = new DBConnector(connectionString);
				cnn.CacheData = false;
				foreach (var simpleCode in code.Split(';'))
				{
					cnn.SendNotification(id, simpleCode);
				}
			}
			catch (Exception exp)
			{
				handler.HandleException(exp, "Policy");
			}
		}

		public void Dispose()
		{
		}

	}

}
