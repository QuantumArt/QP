using Quantumart.QP8.Scheduler.Notification.Data;
using System.Net;
using System.Threading.Tasks;

namespace Quantumart.QP8.Scheduler.Notification.Providers
{
    public interface INotificationProvider
    {
        Task<HttpStatusCode> Notify(NotificationModel notification);
    }
}
