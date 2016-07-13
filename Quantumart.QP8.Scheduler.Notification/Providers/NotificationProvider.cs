using Quantumart.QP8.BLL;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Quantumart.QP8.Scheduler.Notification.Providers
{
	public class NotificationProvider : INotificationProvider
	{
		public async Task<HttpStatusCode> Notify(NotificationModel notification)
		{
			using (var client = new HttpClient())
			{
			    var response = await client.PostAsync(notification.Url, notification.Content);
				return response.StatusCode;
			}
		}

	}
}