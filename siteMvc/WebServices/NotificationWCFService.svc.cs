using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using Quantumart.QP8.BLL.WCF.Notification;
using System.ServiceModel.Activation;
using Microsoft.Practices.EnterpriseLibrary.Common.Configuration;
using Microsoft.Practices.EnterpriseLibrary.ExceptionHandling;
using B = Quantumart.QP8.BLL.Services;

namespace Quantumart.QP8.WebMvc.WebServices
{
	[AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Required)]
	public class NotificationWCFService : INotificationService
	{
		#region INotificationWCFService Members

		public void SendNotificationOneWay(string connectionString, int id, string code)
		{
			SendNotificationProcess(connectionString, id, code);
		}

		public void SendNotification(string connectionString, int id, string code)
		{
			SendNotificationProcess(connectionString, id, code);
		}

		public void Dispose()
		{

		}

		private void SendNotificationProcess(string connectionString, int id, string code)
		{
			try
			{
				new B.NotificationService().SendNotification(connectionString, id, code);
			}
			catch (Exception exp)
			{
				EnterpriseLibraryContainer.Current
					.GetInstance<ExceptionManager>()
					.HandleException(exp, "Policy");
			}
		}

		#endregion
	}
}
