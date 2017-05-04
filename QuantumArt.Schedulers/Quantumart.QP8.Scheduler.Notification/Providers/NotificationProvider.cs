using Quantumart.QP8.Scheduler.Notification.Data;
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
                var content = new MultipartFormDataContent();
                foreach (var param in notification.Parameters)
                {
                    content.Add(new StringContent(param.Value), param.Key);
                }

                var response = await client.PostAsync(notification.Url, content);
                return response.StatusCode;
            }
        }
    }
}
