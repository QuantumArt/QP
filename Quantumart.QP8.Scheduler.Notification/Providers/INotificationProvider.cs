using Quantumart.QP8.BLL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Quantumart.QP8.Scheduler.Notification.Providers
{
	public interface INotificationProvider
	{
		Task<HttpStatusCode> Notify(NotificationModel notification);
	}
}