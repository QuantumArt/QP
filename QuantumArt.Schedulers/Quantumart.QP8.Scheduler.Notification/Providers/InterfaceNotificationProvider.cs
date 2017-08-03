using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Quantumart.QP8.Scheduler.Notification.Data;

namespace Quantumart.QP8.Scheduler.Notification.Providers
{
    public class InterfaceNotificationProvider : IInterfaceNotificationProvider
    {
        public async Task<HttpStatusCode> Notify(InterfaceNotificationModel notification)
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
