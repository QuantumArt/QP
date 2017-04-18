using System.Net;
using System.Threading.Tasks;
using Quantumart.QP8.Scheduler.Notification.Data;

namespace Quantumart.QP8.Scheduler.Notification.Providers
{
    public interface INotificationProvider
    {
        Task<HttpStatusCode> Notify(NotificationModel notification);
    }
}
